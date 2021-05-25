using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using StarWars.API.Schema;
using StarWars.API.Security;
using StarWars.API.Services;

namespace StarWars.API
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
            // Application Services
            services.AddSingleton<IStarWarsDataService, StarWarsDataService>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            // API level auth (Bearer Token)
            services.AddMicrosoftIdentityWebApiAuthentication(
                Configuration, 
                "AzureB2C-Demo-API", 
                JwtBearerDefaults.AuthenticationScheme);

            // UI level auth (Cookie)
            services.AddMicrosoftIdentityWebAppAuthentication(
                        Configuration, 
                        "AzureB2C-Demo-UI", 
                        OpenIdConnectDefaults.AuthenticationScheme)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddInMemoryTokenCaches();

            services.AddControllersWithViews()
                    .AddMicrosoftIdentityUI();
            services.AddRazorPages();

            services.AddOptions();
            services.Configure<OpenIdConnectOptions>(Configuration.GetSection("AzureB2C-Demo-UI"));

            // Schema
            services.AddScoped<IDocumentWriter, GraphQL.SystemTextJson.DocumentWriter>();
            services.AddScoped<StarWarsSchema>(services => new StarWarsSchema(new SelfActivatingServiceProvider(services)));

            // GraphQL
            services.AddScoped<GraphQLUserContextBuilder>();
            services.AddGraphQL()
                    .AddUserContextBuilder(async context => 
                    {
                        // This needs to be done here, as the builder needs Scoped services
                        // and the baseline generic .AddUserContextBuilder puts things
                        // in Singleton scope...
                        var builder = context.RequestServices.GetService<GraphQLUserContextBuilder>();
                        return (await builder.BuildUserContext(context)) as GraphQLUserContext;
                    })
                    .AddGraphQLAuthorization((options) =>
                    {
                        options.AddPolicy(Policies.AuthorizedUser, _ => _.RequireAuthenticatedUser());
                        options.AddPolicy(Policies.CharacterAccess, _ => _.RequireScopeClaim(ApiScope.CharacterRead, ApiScope.CharacterWrite));
                        options.AddPolicy(Policies.CharacterWriteAccess, _ => _.RequireScopeClaim(ApiScope.CharacterWrite));
                    })
                    .AddDefaultEndpointSelectorPolicy()
                    .AddSystemTextJson()
                    .AddWebSockets()
                    .AddGraphTypes(ServiceLifetime.Scoped);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseWebSockets();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // GraphQL
                endpoints.MapGraphQLWebSockets<StarWarsSchema>().RequireAuthorization();
                endpoints.MapGraphQL<StarWarsSchema>().RequireAuthorization();

                // GraphiQL [we provide this ourselves now]
                //endpoints.MapGraphQLGraphiQL().RequireAuthorization();

                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
