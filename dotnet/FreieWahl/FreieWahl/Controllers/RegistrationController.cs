using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using FreieWahl.Application.Authentication;
using FreieWahl.Application.Registrations;
using FreieWahl.Common;
using FreieWahl.Security.Signing.Buergerkarte;
using FreieWahl.Voting.Registrations;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FreieWahl.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly ISignatureHandler _signatureHandler;
        private readonly IRegistrationStore _registrationStore;
        private readonly IAuthorizationHandler _authHandler;
        private readonly IRegistrationHandler _registrationHandler;
        private readonly IVotingStore _votingStore;
        private readonly string _regUrl;
        private readonly int _tokenCount;
        private readonly string _redirectUrl;
        private string _duplicateRedirectUrl;
        private string _errorRedirectUrl;

        public RegistrationController(ILogger<RegistrationController> logger,
            ISignatureHandler signatureHandler,
            IRegistrationStore registrationStore,
            IAuthorizationHandler authHandler,
            IRegistrationHandler registrationHandler,
            IConfiguration configuration,
            IVotingStore votingStore)
        {
            _logger = logger;
            _signatureHandler = signatureHandler;
            _registrationStore = registrationStore;
            _authHandler = authHandler;
            _registrationHandler = registrationHandler;
            _votingStore = votingStore;
            _regUrl = configuration["RemoteTokenStore:Url"];
            _tokenCount = int.Parse(configuration["VotingSettings:MaxNumQuestions"]);
            _redirectUrl = configuration["Registration:RedirectUrl"];
            _errorRedirectUrl = configuration["Registration:ErrorUrl"];
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
                var votingId = dataContent.ToId();
                if (votingId == null)
                    return BadRequest();

                if (!await _registrationStore.IsRegistrationUnique(data.SigneeId, votingId.Value))
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
                    VotingId = votingId.Value,
                    VoterIdentity = data.SigneeId,
                    VoterName = data.SigneeName,
                    RegistrationTime = DateTime.UtcNow,
                    RegistrationStoreId = regUid
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

        public async Task<IActionResult> RegistrationDetails(string regUid)
        {
            var registration = await _registrationStore.GetOpenRegistration(regUid);
            var id = registration.VotingId;

            var voting = await _votingStore.GetById(id);
            ViewData["RegistrationStoreId"] = Guid.NewGuid().ToString("D");
            ViewData["VotingTitle"] = voting.Title;
            ViewData["VotingDescription"] = voting.Description;
            ViewData["ImageData"] = voting.ImageData ?? string.Empty;
            ViewData["StartDate"] = voting.StartDate.ToString("HH:mm, dd.MM.yyyy");
            ViewData["EndDate"] = voting.EndDate.ToString("HH:mm, dd.MM.yyyy");
            ViewData["RegistrationStoreId"] = regUid;
            ViewData["RegistrationStoreSaveRegUrl"] = _regUrl + "saveRegistrationDetails";
            ViewData["TokenCount"] = _tokenCount;
            ViewData["VotingId"] = registration.VotingId.ToString(CultureInfo.InvariantCulture);
            return View();
        }

        public async Task<IActionResult> RegistrationError(string reason, string votingId)
        {
            var id = votingId.ToId();
            if (id.HasValue)
            {
                var voting = await _votingStore.GetById(id.Value);
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

            ViewData["ErrorTitle"] = reason.Equals("duplicate", StringComparison.OrdinalIgnoreCase)
                ? "Bereits registriert"
                : "Fehler beim Identitätsnachweis";
            ViewData["ErrorMessage"] = reason.Equals("duplicate", StringComparison.OrdinalIgnoreCase)
                ? "Sie haben sich für diese Wahl bereits registriert. Eine erneute Registrierung ist nicht möglich."
                : "Beim Verarbeiten der Handysignatur ist es zu einem unerwarteten Fehler gekommen. Wenn dieser Fehler erneut auftritt, kontaktieren Sie bitte den Administrator/die Administratorin Ihrer Wahl.";

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GrantRegistration(string rid)
        {
            var regId = rid.ToId();
            if (regId == null)
            {
                return BadRequest();
            }

            var registration = await _registrationStore.GetOpenRegistration(regId.Value);
            string vid = registration.VotingId.ToString(CultureInfo.InvariantCulture);
            var user = await _authHandler.GetAuthorizedUser(vid,
                Operation.GrantRegistration, Request.Cookies["session"]);
            if (user == null)
            {
                return Unauthorized();
            }

            var link = Url.Action("Vote", "Voting", new{}, HttpContext.Request.Scheme);

            await _registrationHandler.GrantRegistration(regId.Value, user.UserId, link);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DenyRegistration(string rid)
        {
            var regId = rid.ToId();
            if (regId == null)
            {
                return BadRequest();
            }

            var registration = await _registrationStore.GetOpenRegistration(regId.Value);
            string vid = registration.VotingId.ToString(CultureInfo.InvariantCulture);
            var user = await _authHandler.GetAuthorizedUser(vid,
                Operation.GrantRegistration, Request.Cookies["session"]);
            if (user == null)
            {
                return Unauthorized();
            }


            await _registrationHandler.DenyRegistration(regId.Value, user.UserId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> GetRegistrations(string votingId)
        {
            var votingIdVal = votingId.ToId();
            if (votingIdVal == null)
            {
                return BadRequest("Invalid voting id");
            }

            if (await _authHandler.CheckAuthorization(votingId,
                    Operation.GrantRegistration, Request.Cookies["session"]) == false)
            {
                return Unauthorized();
            }

            var registrations = await _registrationStore.GetOpenRegistrationsForVoting(votingIdVal.Value);
            var result = registrations.Select(x =>
                new
                {
                    x.VoterName,
                    x.VoterIdentity,
                    RegistrationId = x.RegistrationId.ToString(CultureInfo.InvariantCulture)
                }
            ).ToArray();

            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetCompletedRegistrations(string votingId)
        {
            var votingIdVal = votingId.ToId();
            if (votingIdVal == null)
            {
                return BadRequest("Invalid voting id");
            }

            if (await _authHandler.CheckAuthorization(votingId,
                    Operation.GrantRegistration, Request.Cookies["session"]) == false)
            {
                return Unauthorized();
            }

            var completedRegistrations = await _registrationStore.GetCompletedRegistrations(votingIdVal.Value);

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
