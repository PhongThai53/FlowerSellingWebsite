using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace FlowerSellingWebsite.Infrastructure.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public RoleAttribute(string role)
        {
            _role = role;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            
            // Check if user is authenticated
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            
            // Check role
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == null || !userRole.Contains(_role))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
