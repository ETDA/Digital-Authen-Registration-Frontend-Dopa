using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using UAF_Frontend_Registration.Settings;

namespace UAF_Frontend_Registration
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            #region Comment New OIDC
            /*services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";
            })
                 .AddCookie("cookie")
                 .AddOpenIdConnect("oidc", options =>
                 {
                     //options.Authority = "https://authen-server-uat.teda.th";
                     //options.Authority = "https://api-uat.teda.th/uafserver/oidp";
                     options.Authority = "https://localhost:44365";
                     //options.Authority = "https://imauth.bora.dopa.go.th/";
                     //options.ClientId = "Z3VMcHdteWRKWGJjQXNRRnlNdGwxeUtkRFdqN2tZYnE=";
                     //options.ClientSecret = "a2FkaHdmenVUVXR5MjR6cXJNS2JoR1R3VEx2OHhUcThkTFlBbUN1Vw==";
                     options.ClientId = "BackOfficeTest";
                     options.ClientSecret = "2PD0BM3CNX1f0phU";
                     //options.ClientId = "275e65ba-d0b0-4d37-a779-9c8fbc4d3927";
                     //options.ClientSecret = "82ecb975-a1da-4a86-9162-687a91d80cfb";

                     options.ResponseType = "code";
                     //options.UsePkce = true;
                     options.ResponseMode = "query";

                     //options.Scope.Add("backofficeApi.read");
                     options.Scope.Clear();
                     //options.Scope.Add("th_fname");
                     //options.Scope.Add("en_fname");
                     //options.Scope.Add("th_lname");
                     //options.Scope.Add("en_lname");
                     //options.Scope.Add("pid");
                     options.Scope.Add("openid");
                     options.SaveTokens = true;

                     //options.TokenValidationParameters = new TokenValidationParameters
                     //{
                     //    NameClaimType = "name",
                     //    RoleClaimType = "role"
                     //};
                 });*/
            #endregion

            services.AddSession(options =>
            {
                options.Cookie.Name = ".dopaAuthen.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });


            services.AddHttpClient();

            //Callback Settings
            services.Configure<CallbackSettings>(
                Configuration.GetSection(nameof(CallbackSettings)));

            services.AddSingleton<ICallbackSettings>(provider =>
                provider.GetRequiredService<IOptions<CallbackSettings>>().Value);

            //LDAP Settings
            services.Configure<LDAPSettings>(
                Configuration.GetSection(nameof(LDAPSettings)));

            services.AddSingleton<ILDAPSettings>(provider =>
                provider.GetRequiredService<IOptions<LDAPSettings>>().Value);

            //EndPoint Settings
            services.Configure<EndpointSettings>(
                Configuration.GetSection(nameof(EndpointSettings)));

            services.AddSingleton<IEndpointSettings>(provider =>
                provider.GetRequiredService<IOptions<EndpointSettings>>().Value);

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
                app.UseHsts();
            }
            //app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
