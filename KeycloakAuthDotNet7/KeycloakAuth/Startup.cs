using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace KeycloakAuth
{
    public class Startup
    {
        public static class Settings
        {
            public static IConfiguration Configuration;
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Settings.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            
            services.AddAuthentication(options =>
            {
                //Sets cookie authentication scheme
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })

            .AddCookie(cookie =>
            {
                //Sets the cookie name and maxage, so the cookie is invalidated.
                cookie.Cookie.Name = "keycloak.cookie";
                cookie.Cookie.MaxAge = TimeSpan.FromMinutes(60);
                cookie.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                cookie.SlidingExpiration = true;
            })
            .AddOpenIdConnect(options =>
            {
                /*
                 * ASP.NET core uses the http://*:5000 and https://*:5001 ports for default communication with the OIDC middleware
                 * The app requires load balancing services to work with :80 or :443
                 * These needs to be added to the keycloak client, in order for the redirect to work.
                 * If you however intend to use the app by itself then,
                 * Change the ports in launchsettings.json, but beware to also change the options.CallbackPath and options.SignedOutCallbackPath!
                 * Use LB services whenever possible, to reduce the config hazzle :)
                */

                //Use default signin scheme
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //Keycloak server
                options.Authority = Configuration.GetSection("Keycloak")["ServerRealm"];
                //Keycloak client ID
                options.ClientId = Configuration.GetSection("Keycloak")["ClientId"];
                //Keycloak client secret
                options.ClientSecret = Configuration.GetSection("Keycloak")["ClientSecret"];
                //Keycloak .wellknown config origin to fetch config
                options.MetadataAddress = Configuration.GetSection("Keycloak")["Metadata"];
                //Require keycloak to use SSL
                options.RequireHttpsMetadata = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                //Save the token
                options.SaveTokens = true;
                //Token response type, will sometimes need to be changed to IdToken, depending on config.
                options.ResponseType = OpenIdConnectResponseType.Code;
                //SameSite is needed for Chrome/Firefox, as they will give http error 500 back, if not set to unspecified.
                options.NonceCookie.SameSite = SameSiteMode.Unspecified;
                options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = ClaimTypes.Role,
                    ValidateIssuer = true
                };


            });

            /*
             * For roles, that are defined in the keycloak, you need to use ClaimTypes.Role
             * You also need to configure keycloak, to set the correct name on each token.
             * Keycloak Admin Console -> Client Scopes -> roles -> mappers -> create
             * Name: "role client mapper" or whatever you prefer
             * Mapper Type: "User Client Role"
             * Multivalued: True
             * Token Claim Name: role
             * Add to access token: True
             */

            
            /*
             * Policy based authentication
             */

            services.AddAuthorization(options =>
            {
                //Create policy with more than one claim
                options.AddPolicy("users", policy =>
                policy.RequireAssertion(context =>
                context.User.HasClaim(c =>
                        (c.Value == "user") || (c.Value == "admin"))));
                //Create policy with only one claim
                options.AddPolicy("admins", policy =>
                    policy.RequireClaim(ClaimTypes.Role, "admin"));
                //Create a policy with a claim that doesn't exist or you are unauthorized to
                options.AddPolicy("noaccess", policy =>
                    policy.RequireClaim(ClaimTypes.Role, "noaccess"));
            });


            /*
             * Non policy based authentication
             * Uncomment below and comment the policy section
             */
           
            //services.AddAuthorization();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                
                //Enable https only for http service behind a load balancer
                //Disable for local testing on http only
                app.Use((context, next) =>
                {
                    context.Request.Scheme = "https";
                    return next();
                });
                
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //Uses the defined policies and customizations from configure services
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
