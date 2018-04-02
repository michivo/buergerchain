using Google.Cloud.Diagnostics.AspNetCore;
using System;
using FreieWahl.Common;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.TimeStamps;
using FreieWahl.Security.UserHandling;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            services.AddSingleton<IJwtAuthentication, FirebaseJwtAuthentication>();
            services.AddSingleton<IUserHandler, UserHandler>();
            services.AddSingleton<ITimestampService>(p => new TimestampService(Configuration["TimestampServer:Url"]));
            services.AddSingleton<IVotingStore>(p => new VotingStore(Configuration["Google:ProjectId"]));
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
