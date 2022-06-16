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
