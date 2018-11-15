using Google.Cloud.Diagnostics.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using FreieWahl.Application.Authentication;
using FreieWahl.Application.Registrations;
using FreieWahl.Application.Voting;
using FreieWahl.Application.VotingResults;
using FreieWahl.Common;
using FreieWahl.Mail;
using FreieWahl.Mail.SendGrid;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.Signing.Buergerkarte;
using FreieWahl.Security.Signing.Common;
using FreieWahl.Security.Signing.VotingTokens;
using FreieWahl.Security.TimeStamps;
using FreieWahl.Security.UserHandling;
using FreieWahl.UserData.Store;
using FreieWahl.Voting.Registrations;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.OpenSsl;

namespace FreieWahl
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            CurrentEnvironment = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        private IHostingEnvironment CurrentEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string projectId = GetProjectId();

            services.AddMvc();

            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

            // Enables Stackdriver Trace.
            services.AddGoogleTrace(options => options.ProjectId = projectId);
            // Sends Exceptions to Stackdriver Error Reporting.
            services.AddGoogleExceptionLogging(
                options =>
                {
                    options.ProjectId = projectId;
                    options.ServiceName = GetServiceName();
                    options.Version = GetVersion();
                });

            var buergerkarteRootCa5 = Configuration["Buergerkarte:RootCertificate"];
            var buergerkarteMobile05 = Configuration["Buergerkarte:CertificateMobile05"];

            var certRaw = Convert.FromBase64String(buergerkarteRootCa5);
            X509Certificate2 cert = new X509Certificate2(certRaw);
            var certRawMob = Convert.FromBase64String(buergerkarteMobile05);
            X509Certificate2 certMobile = new X509Certificate2(certRawMob);

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IJwtAuthentication, FirebaseJwtAuthentication>();
            services.AddSingleton<IUserHandler, UserHandler>();
            services.AddSingleton<ITimestampService>(p =>
            {
                var timestampServers = _GetTimestampServers();
                var logger = LogFactory.CreateLogger("FreieWahl.Security.TimeStamps.TimestampService");
                return new TimestampService(timestampServers, logger);
            });
            services.AddSingleton<IVotingStore>(p => new VotingFireStore(Configuration["Google:ProjectId"], Configuration["Buckets:UserImages"]));
            services.AddSingleton<IAuthenticationManager, AuthenticationManager>();
            services.AddSingleton<IMailProvider>(p => new SendGridMailProvider(Configuration["SendGrid:ApiKey"],
                Configuration["SendGrid:FromMail"], Configuration["SendGrid:FromName"]));
            services.AddSingleton<ISignatureHandler>(p => new SignatureHandler(new[] { cert, certMobile }));
            //services.AddSingleton<IVotingKeyStore>(p => new VotingKeyStore(Configuration["Datastore:ProjectId"]));
            services.AddSingleton<IVotingKeyStore>(p => new VotingKeyFireStore(Configuration["Google:ProjectId"]));
            services.AddSingleton<IRegistrationHandler, RegistrationHandler>();
            services.AddSingleton<IAuthorizationHandler, AuthorizationHandler>();
            services.AddSingleton<IRemoteTokenStore>(p => new RemoteTokenStore(Configuration["RemoteTokenStore:Url"]));
            //services.AddSingleton<IRegistrationStore>(p => new RegistrationStore(Configuration["Datastore:ProjectId"]));
            services.AddSingleton<IRegistrationStore>(p => new RegistrationFireStore(Configuration["Google:ProjectId"]));
            services.AddSingleton<ISignatureProvider>(p => new SignatureProvider(Configuration["Registration:AuthKey"]));
            services.AddSingleton<IVotingResultManager, VotingResultManager>();
            services.AddSingleton<IVotingChainBuilder, VotingChainBuilder>();
            //services.AddSingleton<IVotingResultStore>(p => new VotingResultStore(Configuration["Datastore:ProjectId"]));
            services.AddSingleton<IVotingResultStore>(p => new VotingResultFireStore(Configuration["Google:ProjectId"]));
            services.AddSingleton<IVotingManager, VotingManager>();
            services.AddSingleton<ISessionCookieProvider>(p =>
                new SessionCookieProvider(Configuration["SessionCookies:ProviderUrl"]));
            var sp = services.BuildServiceProvider();
            services.AddSingleton<IVotingTokenHandler>(p => new VotingTokenHandler(sp.GetService<IVotingKeyStore>(),
                int.Parse(Configuration["VotingSettings:MaxNumQuestions"])));
            services.AddSingleton<IUserDataStore>(p => new UserDataFireStore(Configuration["Google:ProjectId"], Configuration["Buckets:UserImages"]));
        }

        private List<TimestampServer> _GetTimestampServers()
        {
            var servers = Configuration.GetSection("TimestampServers:Servers").GetChildren();
            var result = new List<TimestampServer>();
            foreach (var serverInfo in servers)
            {
                var url = serverInfo["Url"];
                var cert = serverInfo["Certificate"];
                var prio = int.Parse(serverInfo["Priority"]);
                var reader = new PemReader(new StringReader(cert));
                var obj = reader.ReadObject();
                var x509Cert = (Org.BouncyCastle.X509.X509Certificate)obj;
                result.Add(new TimestampServer(url, x509Cert, prio));
            }

            return result;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Only use Console and Debug logging during development.
            LogFactory.Setup(loggerFactory);
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                loggerFactory.AddConsole((x, y) => true);
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseGoogleExceptionLogging();
                // Send logs to Stackdriver Logging.
                loggerFactory.AddGoogle(GetProjectId());
            }
            
            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseGoogleTrace();

            app.ApplicationServices.GetService<IJwtAuthentication>()
                .Initialize(
                    Configuration["JwtAuthentication:PublicKeyUrl"],
                    Configuration["JwtAuthentication:Issuer"],
                    Configuration["JwtAuthentication:Audience"]);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private string GetProjectId()
        {
            var instance = Google.Api.Gax.Platform.Instance();
            var projectId = instance?.ProjectId ?? Configuration["Google:ProjectId"];
            if (string.IsNullOrEmpty(projectId))
            {
                throw new Exception(
                    "The logging, tracing and error reporting libraries need a project ID. " +
                    "Update appsettings.json by setting the ProjectId property with your " +
                    "Google Cloud Project ID, then recompile.");
            }
            return projectId;
        }

        private string GetServiceName()
        {
            var instance = Google.Api.Gax.Platform.Instance();
            // An identifier of the service. See https://cloud.google.com/error-reporting/docs/formatting-error-messages#FIELDS.service.
            var serviceName =
                instance?.GaeDetails?.ServiceId ??
                Configuration["Google:ErrorReporting:ServiceName"];
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new Exception(
                    "The error reporting library needs a service name. " +
                    "Update appsettings.json by setting the Google:ErrorReporting:ServiceName property with your " +
                    "Service Id, then recompile.");
            }
            return serviceName;
        }

        private string GetVersion()
        {
            var instance = Google.Api.Gax.Platform.Instance();
            // The source version of the service. See https://cloud.google.com/error-reporting/docs/formatting-error-messages#FIELDS.version.
            var versionId =
                instance?.GaeDetails?.VersionId ??
                Configuration["Google:ErrorReporting:Version"];
            if (string.IsNullOrEmpty(versionId))
            {
                throw new Exception(
                    "The error reporting library needs a version id. " +
                    "Update appsettings.json by setting the Google:ErrorReporting:Version property with your " +
                    "service version id, then recompile.");
            }
            return versionId;
        }
    }
}
