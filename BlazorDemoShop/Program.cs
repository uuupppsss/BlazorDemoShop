using BlazorDemoShop.Components;
using BlazorDemoShop.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BlazorDemoShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "BlazorDemoShop.Auth";
                    options.LoginPath = "/login";
                    options.AccessDeniedPath = "/login";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                });
            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();

            builder.Services.AddHttpClient<ApiAuthClientService>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var baseUrl = configuration["ApiSettings:BaseUrl"];
                client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(baseUrl) ? "https://localhost:7299/" : baseUrl);
            });
            builder.Services.AddHttpClient<ApiProductsClientService>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var baseUrl = configuration["ApiSettings:BaseUrl"];
                client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(baseUrl) ? "https://localhost:7299/" : baseUrl);
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapPost("/auth/session/signin", async (SessionSignInRequest request, HttpContext context) =>
            {
                if (string.IsNullOrWhiteSpace(request.UserName) || request.UserId <= 0)
                {
                    return Results.BadRequest("Некорректные данные пользователя.");
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, request.UserId.ToString()),
                    new(ClaimTypes.Name, request.UserName)
                };

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, request.Email));
                }

                if (!string.IsNullOrWhiteSpace(request.Role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, request.Role));
                }

                if (!string.IsNullOrWhiteSpace(request.Token))
                {
                    claims.Add(new Claim(AuthClaimTypes.AccessToken, request.Token));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var expiresUtc = request.TokenExpiration == default
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : new DateTimeOffset(DateTime.SpecifyKind(request.TokenExpiration, DateTimeKind.Utc));

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = expiresUtc
                };

                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
                return Results.Ok();
            }).AllowAnonymous();

            app.MapPost("/auth/session/signout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Ok();
            }).AllowAnonymous();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}

