using GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using StarWars.API.Schema;
using StarWars.API.Security;
using StarWars.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Application Insights
builder.Services.AddApplicationInsightsTelemetry(); // Add this line of code to enable Application Insights.
builder.Services.AddServiceProfiler(); // Add this line of code to Enable Profiler

// Application Services
builder.Services.AddSingleton<IStarWarsDataService, StarWarsDataService>();

// API level auth
builder.Services.AddMicrosoftIdentityWebApiAuthentication(
    builder.Configuration,
    "AzureB2C_Demo_API",
    JwtBearerDefaults.AuthenticationScheme
);

// UI level auth (Cookie)
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-7.0
    options.HandleSameSiteCookieCompatibility();
});

builder.Services
       .AddMicrosoftIdentityWebAppAuthentication(
            builder.Configuration,
            "AzureB2C_Demo_UI",
            OpenIdConnectDefaults.AuthenticationScheme
        )
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();
builder.Services.AddRazorPages();

builder.Services.AddOptions();
builder.Services.Configure<OpenIdConnectOptions>(builder.Configuration.GetSection("AzureB2C_Demo_UI"));

// GraphQL
builder.Services.AddScoped<GraphQLUserContextBuilder>();
builder.Services.AddGraphQL(config => 
{
    config.AddSelfActivatingSchema<StarWarsSchema>(GraphQL.DI.ServiceLifetime.Scoped)
             .AddSystemTextJson()
             .AddScopedSubscriptionExecutionStrategy()
             .AddErrorInfoProvider()
             .AddUserContextBuilder<GraphQLUserContext>(async (context, payload) => 
                {
                    // This needs to be done here, as the builder needs Scoped services
                    // and the baseline generic .AddUserContextBuilder puts things
                    // in Singleton scope...
                    var builder = context.RequestServices.GetService<GraphQLUserContextBuilder>();
                    return (await builder.BuildUserContextAsync(context, payload)) as GraphQLUserContext;
                })
            .AddAuthorizationRule()
            .AddWebSocketAuthentication<GraphQLWebSocketAuthService>();
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AuthorizedUser, _ => _.RequireAuthenticatedUser());
    options.AddPolicy(Policies.CharacterAccess, _ => _.RequireScopeClaim(ApiScope.CharacterRead, ApiScope.CharacterWrite));
    options.AddPolicy(Policies.CharacterWriteAccess, _ => _.RequireScopeClaim(ApiScope.CharacterWrite));
});

var app = builder.Build();

#if DEBUG
app.UseDeveloperExceptionPage();
#endif

app.UseHttpsRedirection();

app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();

app.UseGraphQL("/graphql", config => 
{
    config.AuthorizationRequired = true;
    config.AuthorizedPolicy = Policies.AuthorizedUser;
});

app.MapControllers();
app.MapRazorPages();

// app.UseGraphQLGraphiQL(
//     "/ui/graphiql",
//     options: new GraphQL.Server.Ui.GraphiQL.GraphiQLOptions
//     {
//         GraphQLEndPoint = "/graphql",
//         SubscriptionsEndPoint = "/graphql"
//     });

await app.RunAsync();
