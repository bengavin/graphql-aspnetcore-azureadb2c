using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StarWars.API.Schema;
using StarWars.API.Services;

namespace StarWars.API
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Application Services
            services.AddSingleton<IStarWarsDataService, StarWarsDataService>();

            // Schema
            services.AddScoped<IDocumentWriter, GraphQL.SystemTextJson.DocumentWriter>();
            services.AddScoped<StarWarsSchema>(services => new StarWarsSchema(new SelfActivatingServiceProvider(services)));

            // GraphQL
            services.AddGraphQL()
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

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // GraphQL
                endpoints.MapGraphQLWebSockets<StarWarsSchema>();
                endpoints.MapGraphQL<StarWarsSchema>();

                // GraphiQL
                endpoints.MapGraphQLGraphiQL();
            });
        }
    }
}
