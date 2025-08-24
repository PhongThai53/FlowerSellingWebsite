using Microsoft.Extensions.FileProviders;

namespace FlowerSellingWebsite_FE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            
            // Configure static files to serve HTML files with correct content type
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    if (context.File.Name.EndsWith(".html"))
                    {
                        context.Context.Response.Headers.Append("Content-Type", "text/html");
                    }
                }
            });
            
            // Add routing
            app.UseRouting();
            
            // Default route - redirect to login page
            app.MapGet("/", () => Results.Redirect("/html/auth/login-register.html"));

            app.Run();
        }
    }
}
