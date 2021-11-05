using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Caching.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using StarWars.UI.Blazor.Data;
using StarWars.UI.Blazor.Services;

namespace StarWars.UI.Blazor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                    .AddMicrosoftIdentityUI();

            services.AddRazorPages();

            services.AddScoped<StarWarsDataService>();

            if (string.Equals(Configuration["AzureB2C_Blazor_UI:TokenCache:Type"], "local", StringComparison.OrdinalIgnoreCase))
            {
                // NOTE: DO NOT USE THIS IN PRODUCTION, EVER
                services.AddInsecureDistributedTokenCache();
            }
            else
            {
                services.AddCosmosCache(options => {
                    var cacheConfig = Configuration.GetSection("AzureB2C_Blazor_UI:TokenCache");
                    options.DatabaseName = cacheConfig["Database"];
                    options.ContainerName = cacheConfig["Container"];
                    options.ClientBuilder = new CosmosClientBuilder(cacheConfig["ConnectionString"]);
                    options.CreateIfNotExists = true;
                });
            }

            // UI level auth (Cookie)
            services.AddMicrosoftIdentityWebAppAuthentication(
                        Configuration, 
                        "AzureB2C_Blazor_UI", 
                        OpenIdConnectDefaults.AuthenticationScheme)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddDistributedTokenCaches();

            services.AddServerSideBlazor()
                    .AddMicrosoftIdentityConsentHandler();
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
