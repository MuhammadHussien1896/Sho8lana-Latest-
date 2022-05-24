using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Sho8lana.Hangfire
{
    public class AuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.IsInRole("Admin");
        }
    }
}
