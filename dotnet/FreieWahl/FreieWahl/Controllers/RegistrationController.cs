using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using FreieWahl.Application.Authentication;
using FreieWahl.Application.Registrations;
using FreieWahl.Application.Voting;
using FreieWahl.Common;
using FreieWahl.Helpers;
using FreieWahl.Security.Signing.Buergerkarte;
using FreieWahl.Voting.Common;
using FreieWahl.Voting.Registrations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FreieWahl.Controllers
{
    [ForwardedRequireHttps]
    public class RegistrationController : Controller
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly ISignatureHandler _signatureHandler;
        private readonly IRegistrationStore _registrationStore;
        private readonly IAuthorizationHandler _authHandler;
        private readonly IRegistrationHandler _registrationHandler;
        private readonly IVotingManager _votingManager;
        private readonly IHostingEnvironment _environment;
        private readonly IChallengeHandler _challengeService;
        private readonly string _regUrl;
        private readonly int _tokenCount;
        private readonly string _redirectUrl;
        private readonly string _errorRedirectUrl;

        public RegistrationController(ILogger<RegistrationController> logger,
            ISignatureHandler signatureHandler,
            IRegistrationStore registrationStore,
            IAuthorizationHandler authHandler,
            IRegistrationHandler registrationHandler,
            IConfiguration configuration,
            IVotingManager votingManager,
            IHostingEnvironment environment,
            IChallengeHandler challengeHandler)
        {
            _logger = logger;
            _signatureHandler = signatureHandler;
            _registrationStore = registrationStore;
            _authHandler = authHandler;
            _registrationHandler = registrationHandler;
            _votingManager = votingManager;
            _environment = environment;
            _challengeService = challengeHandler;
            _regUrl = configuration["RemoteTokenStore:Url"];
            _tokenCount = int.Parse(configuration["VotingSettings:MaxNumQuestions"]);
            _redirectUrl = configuration["Registration:RedirectUrl"];
            _errorRedirectUrl = configuration["Registration:ErrorUrl"];
        }

        [HttpPost]
        public async Task<IActionResult> SendSmsChallenge(string name, string phone, string registrationId, string votingId)
        {
            var voting = await _votingManager.GetById(votingId);
            if (voting == null)
            {
                return BadRequest("Ungültige Abstimmung!");
            }
            await _challengeService.CreateChallenge(name, phone, voting, registrationId, ChallengeType.Sms);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> VerifySmsChallenge(string registrationId, string challengeResponse)
        {
            var challenge = await _challengeService.GetChallengeForRegistration(registrationId);
            if (challenge.Value.Equals(challengeResponse.Trim(), StringComparison.OrdinalIgnoreCase) == false)
            {
                return BadRequest();
            }

            await _registrationStore.AddOpenRegistration(new OpenRegistration
            {
                VotingId = challenge.VotingId,
                VoterIdentity = challenge.RecipientAddress,
                VoterName = challenge.RecipientName,
                RegistrationTime = DateTime.UtcNow,
                Id = registrationId,
                RegistrationType = RegistrationType.Sms
            });

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string regUid)
        {
            string dataContent = "x";
            try
            {
                _logger.LogInformation("Received response form: ");
                foreach (var formPart in Request.Form)
                {
                    _logger.LogInformation(formPart.Key + " -> " +
                                           formPart.Value.Aggregate("", (s, s1) => s + ", " + s1));
                }

                var response = Request.Form["XMLResponse"];
                _logger.LogInformation("Received response: " + response);
                var doc = new XmlDocument();
                doc.LoadXml(response);
                XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
                manager.AddNamespace("sl", "http://www.buergerkarte.at/namespaces/securitylayer/1.2#");
                var signedData = doc.SelectSingleNode("//sl:CreateCMSSignatureResponse/sl:CMSSignature", manager)
                    .InnerText;
                var data = _signatureHandler.GetSignedContent(signedData);
                _logger.LogInformation("Received signed data: " + data.Data);

                dataContent = data.Data;

                if (!await _registrationStore.IsRegistrationUnique(data.SigneeId, dataContent))
                {
                    return new ContentResult
                    {
                        ContentType = "text/html",
                        StatusCode = 200,
                        Content = "<!DOCTYPE html><html><head><script>window.top.location.href = '" + _errorRedirectUrl +
                                  "?reason=duplicate&votingId=" + dataContent + "';</script></head></html>"
                    };
                }

                await _registrationStore.AddOpenRegistration(new OpenRegistration
                {
                    VotingId = dataContent,
                    VoterIdentity = data.SigneeId,
                    VoterName = data.SigneeName,
                    RegistrationTime = DateTime.UtcNow,
                    Id = regUid,
                    RegistrationType = RegistrationType.Buergerkarte
                });

                return new ContentResult
                { // all is good
                    ContentType = "text/html",
                    StatusCode = 200,
                    Content = "<!DOCTYPE html><html><head><script>window.top.location.href = '" + _redirectUrl +
                              "?regUid=" + regUid + "';</script></head></html>"
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing signature");
                return new ContentResult
                {
                    ContentType = "text/html",
                    StatusCode = 200,
                    Content = "<!DOCTYPE html><html><head><script>window.top.location.href = '" + _errorRedirectUrl +
                              "?reason=error&votingId=" + dataContent + "';</script></head></html>"
                };
            }
            finally
            {
                _logger.LogInformation("Finished processing signature");
            }
        }

        public async Task<IActionResult> FakeRegistrations(string votingId, int count)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            string[] firstNames = { "Anton", "Berta", "Christoph", "Dora", "Emil", "Franz", "Greta", "Heinz", "Ida", "Jörg", "Karl", "Leo", "Michael", "Norbert", "Oskar", "Paula", "Rosi", "Susi", "Thomas", "Uwe", "Viktor", "Werner", "Xaver", "Zacharias" };
            string[] lastNames = { "Almer", "Brunner", "Degen", "Eisenberger", "Faschinger", "Gruber", "Huber", "Jaklitsch", "Konrad", "Lechner", "Maier", "Nachbagauer", "Ortner", "Pospisil", "Rüdiger", "Schiffkowitz", "Timischl", "Unterasinger", "Vogel", "Wiener", "Richter" };
            Random rnd = new Random();

            for (int i = 0; i < count; i++)
            {
                var firstName = firstNames[rnd.Next(0, firstNames.Length)];
                var lastName = lastNames[rnd.Next(0, lastNames.Length)];
                var fullName = firstName + " " + lastName;
                var regId = Guid.NewGuid().ToString();

                await _registrationStore.AddOpenRegistration(new OpenRegistration
                {
                    VotingId = votingId,
                    VoterIdentity = rnd.Next(1000000000).ToString(),
                    VoterName = fullName,
                    RegistrationTime = DateTime.UtcNow,
                    Id = regId,
                    RegistrationType = RegistrationType.Buergerkarte
                });

                var request = WebRequest.CreateHttp("https://tokenstore-210111.appspot.com/saveRegistrationDetails");
                request.ContentType = "application/json";
                request.Method = WebRequestMethods.Http.Post;

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    var resultData = new
                    {
                        id = regId,
                        mail = "michfasch@gmx.at",
                        password = "123",
                        tokenCount = 20,
                        votingId
                    };

                    var json = JsonConvert.SerializeObject(resultData);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = await request.GetResponseAsync();
                string result;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
            }

            return new ContentResult()
            {
                Content = "OK",
                StatusCode = 200,
                ContentType = "text/html"
            };
        }

        public async Task<IActionResult> RegistrationDetails(string regUid)
        {
            var registration = await _registrationStore.GetOpenRegistration(regUid);
            var id = registration.VotingId;

            var voting = await _votingManager.GetById(id);
            ViewData["RegistrationStoreId"] = Guid.NewGuid().ToString("D");
            ViewData["VotingTitle"] = voting.Title;
            ViewData["VotingDescription"] = voting.Description;
            ViewData["ImageData"] = voting.ImageData ?? string.Empty;
            ViewData["StartDate"] = voting.StartDate.ToSecondsSinceEpoch();
            ViewData["EndDate"] = voting.EndDate.ToSecondsSinceEpoch();
            ViewData["RegistrationStoreId"] = regUid;
            ViewData["RegistrationStoreSaveRegUrl"] = _regUrl + "saveRegistrationDetails";
            ViewData["TokenCount"] = _tokenCount;
            ViewData["VotingId"] = registration.VotingId.ToString(CultureInfo.InvariantCulture);
            return View();
        }

        public async Task<IActionResult> RegistrationError(string reason, string votingId)
        {
            try
            {
                var voting = await _votingManager.GetById(votingId);
                if (voting != null)
                {
                    ViewData["RegistrationStoreId"] = Guid.NewGuid().ToString("D");
                    ViewData["VotingTitle"] = voting.Title;
                    ViewData["VotingDescription"] = voting.Description;
                    ViewData["ImageData"] = voting.ImageData ?? string.Empty;
                    ViewData["StartDate"] = voting.StartDate.ToString("HH:mm, dd.MM.yyyy");
                    ViewData["EndDate"] = voting.EndDate.ToString("HH:mm, dd.MM.yyyy");
                    ViewData["RegistrationStoreSaveRegUrl"] = _regUrl + "saveRegistrationDetails";
                    ViewData["TokenCount"] = _tokenCount;
                    ViewData["VotingId"] = votingId;
                }

                ViewData["ErrorTitle"] = _GetErrorTitle(reason);
                ViewData["ErrorMessage"] = _GetErrorMessage(reason);

                return View();
            }
            catch (Exception)
            {
                // TODO logging
                ViewData["ErrorTitle"] = _GetErrorTitle(reason);
                ViewData["ErrorMessage"] = _GetErrorMessage(reason);

                return View();
            }
        }

        private static string _GetErrorTitle(string reason)
        {
            if (reason.Equals("duplicate", StringComparison.OrdinalIgnoreCase))
                return "Bereits registriert";
            if (reason.Equals("smsVerificationFailed"))
                return "Ungültiger Verifikationscode";
            return "Fehler beim Identitätsnachweis";
        }

        private static string _GetErrorMessage(string reason)
        {
            if (reason.Equals("duplicate", StringComparison.OrdinalIgnoreCase))
                return "Sie haben sich für diese Wahl bereits registriert. Eine erneute Registrierung ist nicht möglich.";
            if (reason.Equals("smsVerificationFailed"))
                return "Der eingegebene Verifikationscode ist nicht korrekt. Bitte registrieren Sie sich noch einmal!";
            return "Beim Verarbeiten des Identitätsnachweises ist es zu einem unerwarteten Fehler gekommen. Wenn dieser Fehler erneut auftritt, kontaktieren Sie bitte den Administrator/die Administratorin Ihrer Wahl.";
        }


        [HttpPost]
        public async Task<IActionResult> GrantRegistration(string[] registrationIds, int utcOffsetMinutes, string timezoneName)
        {
            foreach (var regId in registrationIds)
            {
                var registration = await _registrationStore.GetOpenRegistration(regId);
                string vid = registration.VotingId.ToString(CultureInfo.InvariantCulture);
                var auth = await _authHandler.CheckAuthorization(vid,
                    Operation.GrantRegistration, Request.Cookies["session"]);
                if (!auth.IsAuthorized)
                {
                    return Unauthorized();
                }

                var link = Url.Action("Vote", "Voting", new { }, HttpContext.Request.Scheme);

                await _registrationHandler.GrantRegistration(regId, auth.User.UserId, link,
                    TimeSpan.FromMinutes(utcOffsetMinutes), timezoneName);
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DenyRegistration(string[] registrationIds)
        {
            foreach (var regId in registrationIds)
            {
                var registration = await _registrationStore.GetOpenRegistration(regId);
                string vid = registration.VotingId.ToString(CultureInfo.InvariantCulture);
                var auth = await _authHandler.CheckAuthorization(vid,
                    Operation.GrantRegistration, Request.Cookies["session"]);
                if (!auth.IsAuthorized)
                {
                    return Unauthorized();
                }


                await _registrationHandler.DenyRegistration(regId, auth.User.UserId);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> GetRegistrations(string votingId)
        {
            if ((await _authHandler.CheckAuthorization(votingId,
                    Operation.GrantRegistration, Request.Cookies["session"])).IsAuthorized == false)
            {
                return Unauthorized();
            }

            var registrations = await _registrationStore.GetOpenRegistrationsForVoting(votingId);

            var result = registrations.Select(x =>
                new
                {
                    x.VoterName,
                    x.VoterIdentity,
                    RegistrationId = x.Id,
                    Date = x.RegistrationTime.ToSecondsSinceEpoch(),
                    RegistrationType = (int)x.RegistrationType
                }
            ).ToArray();

            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetCompletedRegistrations(string votingId)
        {
            if ((await _authHandler.CheckAuthorization(votingId,
                    Operation.GrantRegistration, Request.Cookies["session"])).IsAuthorized == false)
            {
                return Unauthorized();
            }

            var completedRegistrations = await _registrationStore.GetCompletedRegistrations(votingId);

            var result = completedRegistrations.Select(x =>
                new
                {
                    x.VoterName,
                    x.VoterIdentity,
                    Decision = (int)x.Decision
                }
            ).ToArray();

            return new JsonResult(result);
        }


    }
}
