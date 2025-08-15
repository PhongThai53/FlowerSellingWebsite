using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace FlowerSellingWebsite.Infrastructure.Swagger
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Get all [Authorize] attributes declared on the endpoint method or controller class
            var authorizeAttributes = context.MethodInfo.GetCustomAttributes<AuthorizeAttribute>(true);
            
            // Add attributes from the controller class if available
            if (context.MethodInfo.DeclaringType != null)
            {
                authorizeAttributes = authorizeAttributes.Union(context.MethodInfo.DeclaringType.GetCustomAttributes<AuthorizeAttribute>(true));
            }

            if (!authorizeAttributes.Any())
            {
                // No [Authorize] attributes found, no need to add security requirements
                return;
            }

            // Initialize the security requirements collection if needed
            operation.Security ??= new List<OpenApiSecurityRequirement>();

            // Add JWT bearer token security requirement
            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            };

            operation.Security.Add(securityRequirement);

            // Add lock icon to endpoints that require authorization
            operation.Description = $"{operation.Description}\n\n**Authorization required**";
        }
    }
}
