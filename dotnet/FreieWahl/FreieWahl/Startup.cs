using Google.Cloud.Diagnostics.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using FreieWahl.Application.Authentication;
using FreieWahl.Application.Registrations;
using FreieWahl.Common;
using FreieWahl.Mail;
using FreieWahl.Mail.SendGrid;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.Signing.Buergerkarte;
using FreieWahl.Security.Signing.VotingTokens;
using FreieWahl.Security.TimeStamps;
using FreieWahl.Security.UserHandling;
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
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

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

            // TODO: put root CAs in config

            var buergerkarteRootCa5 =
    "MIIFyTCCA7GgAwIBAgIDD820MA0GCSqGSIb3DQEBCwUAMIGLMQswCQYDVQQGEwJBVDFIMEYGA1UECgw/QS1UcnVzdCBHZXMuIGYuIFNpY2hlcmhlaXRzc3lzdGVtZSBpbSBlbGVrdHIuIERhdGVudmVya2VociBHbWJIMRgwFgYDVQQLDA9BLVRydXN0LVJvb3QtMDUxGDAWBgNVBAMMD0EtVHJ1c3QtUm9vdC0wNTAeFw0xMzA5MjMxMzI0MTFaFw0yMzA5MjAxMTI0MTFaMIGLMQswCQYDVQQGEwJBVDFIMEYGA1UECgw/QS1UcnVzdCBHZXMuIGYuIFNpY2hlcmhlaXRzc3lzdGVtZSBpbSBlbGVrdHIuIERhdGVudmVya2VociBHbWJIMRgwFgYDVQQLDA9BLVRydXN0LVJvb3QtMDUxGDAWBgNVBAMMD0EtVHJ1c3QtUm9vdC0wNTCCAiAwDQYJKoZIhvcNAQEBBQADggINADCCAggCggIBAOT7jFImpWeBhGjdgsnNqHIBWSI/JOkSpJKXxVDO8kU/a0QFGLp7ca/mjbtt9uTz5dy85HgTI7IKRJ23vTdA1iVEUInOaNLDYqdEoSNFr18GcXZG4Wn/4iHgP88yleqIJqcgrMJxXTDJDOxELc7FZXzXB3419g0YFk17q/OqD33e6IyULpPQt25IOMQCIhrfIKWCY79T1UQVBjukO3rctu6Qi0ACtJ/A9nEzWaYi07BoIz/9hMiWsPlwSy80hv0lVZnRzXcnOMRtXBnq634ThgGgEEAmRx++FL5fpbg/YKFu4SGOEyV4Lqd6zVivflusP84Ps/JXfNV7bcnT/K2VrRu/h5hPJ+YLqWg75Cws9RRH16ldgvbim7cg4eUaayx4CI1sdYzqN5aJnVnpdDIvGDAYOgQlSwbtxmdnJoBqX4F3MB6e0XSPX4zAVGrspBhhmXod+Z356Pnx73K+zi8ZknzjKK/RuLhv0GC+eFikLjc6sieJEVGiXom8HcxXZUtJTBMQAq5Xvkwh8SKqHqCS1FQsuJt8M2gnECodS/8GCgKTgIcZr7+ogxIQjn0QpSuQ6A7gFIZF9tflVnOWH4+ePCqjGl4skGaFbwF2vbPwKcgniqmpI7DV8vDK1b22MnDMLxxZv+rDBqRg36uJbkcU74WQa2gjlk4G07EnowPDudm9AgEDozYwNDAPBgNVHRMBAf8EBTADAQH/MBEGA1UdDgQKBAhA+blnvgPSCDAOBgNVHQ8BAf8EBAMCAQYwDQYJKoZIhvcNAQELBQADggIBAOIqZcZrWivIqDTLlxEdJh+jss64PCshn5j0Fx8NtnuuyxBtg/JjwYiu6cBSQq43nwuZV1LoRX6YlOkpR5/xB8FCCPNzPKprNbNsFSuRMRvkfpLnw8WmITjfG77Rn5YNULb1e5SjLaqvt43SOy18ghDUakrJYaOmj6eyoNlUw5d/0YnMY/jZ3zhYlboBUMwK84tJPH8/PajzaMzHmNPZNTD3DoJe+BBhrrxO8Cs0eqKa9tuNr+sDTCfD3q5s3VUUrz8d64+atnhJ7rz5HndgAiTc3t7ppfuRphx6skng978dB66Gy7vZANfLARjv6MOPDAcwcFjB8mPqjP22rePoBzw9WwWHdMs15e8Jt7ughGm8QXFj2zKcQeFfftp2bZOjroX65YzJUqwny2CzNixJqQTeuCcrCTHEkpPpjNGkS/2+VlGw2LfOnUXDG0gv0bMw935cqVsxP+UFm+F2qdf1KYZzVxy9L9vXGRb0JTTxgxa0MlgLsVlO44vQoyuLG0DC9+NSqE5K7nXp7WOZGwb7MI38HleZ7M4UKOOgjS3r7wceDAKOjEjMiNqmrXmUtKzpDDC2/wY7FHGVhfuwesuLSFly21AA8reNeSvNBJWSdUkCllSiHVSFu2CvfX2qs735cDxZesGB/KxQABgS5LXcXdilWF4dXydpjszb76pXGquE";
            var certRaw = System.Convert.FromBase64String(buergerkarteRootCa5);
            X509Certificate2 cert = new X509Certificate2(certRaw);

            services.AddSingleton<IJwtAuthentication, FirebaseJwtAuthentication>();
            services.AddSingleton<IUserHandler, UserHandler>();
            services.AddSingleton<ITimestampService>(p =>
            {
                var timestampServers = _GetTimestampServers();
                var logger = LogFactory.CreateLogger("FreieWahl.Security.TimeStamps.TimestampService");
                return new TimestampService(timestampServers, logger);
            });
            services.AddSingleton<IVotingStore>(p => new VotingStore(Configuration["Datastore:ProjectId"]));
            services.AddSingleton<IAuthenticationManager, AuthenticationManager>();
            services.AddSingleton<IMailProvider>(p => new SendGridMailProvider(Configuration["SendGrid:ApiKey"],
                Configuration["SendGrid:FromMail"], Configuration["SendGrid:FromName"]));
            services.AddSingleton<ISignatureHandler>(p => new SignatureHandler(new[] { cert }));
            services.AddSingleton<IVotingKeyStore>(p => new VotingKeyStore(Configuration["Datastore:ProjectId"]));
            services.AddSingleton<IRegistrationHandler, RegistrationHandler>();
            services.AddSingleton<IAuthorizationHandler, AuthorizationHandler>();
            services.AddSingleton<IRemoteTokenStore>(p => new RemoteTokenStore(Configuration["RemoteTokenStore:Url"]));
            services.AddSingleton<IRegistrationStore, RegistrationStore>();
            var sp = services.BuildServiceProvider();
            services.AddSingleton<IVotingTokenHandler>(p => new VotingTokenHandler(sp.GetService<IVotingKeyStore>(),
                int.Parse(Configuration["VotingSettings:MaxNumQuestions"])));
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
                var x509Cert = (Org.BouncyCastle.X509.X509Certificate) obj;
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
                    Configuration["JwtAuthentication:Domain"],
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
