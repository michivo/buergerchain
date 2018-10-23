using System;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FreieWahl.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ForwardedRequireHttpsAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentException("Filter Context");
            }

            if (filterContext.HttpContext != null)
            {
                if (filterContext.HttpContext.Request.IsHttps)
                {
                    return;
                }

                var currentUrl = filterContext.HttpContext.Request.GetUri();
                if (currentUrl.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.CurrentCultureIgnoreCase))
                {
                    return;
                }

                if (string.Equals(filterContext.HttpContext.Request.Headers["X-Forwarded-Proto"], "https", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                if (filterContext.HttpContext.Request.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }

            base.OnAuthorization(filterContext);
        }
    }
}
