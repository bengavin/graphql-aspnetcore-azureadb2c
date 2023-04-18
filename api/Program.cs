using GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Rewrite;
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

// API level auth (Bearer Token)
// builder.Services.AddMicrosoftIdentityWebApiAuthentication(
//     builder.Configuration,
//     "AzureB2C_Demo_API",
//     JwtBearerDefaults.AuthenticationScheme,
//     true
// );

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(options =>
                {
                    builder.Configuration.GetSection("AzureB2C_Demo_API").Bind(options);
                    options.ForwardDefaultSelector = ctx => ctx.Request.Path.StartsWithSegments("/ui/graphiql")
                                                         ? OpenIdConnectDefaults.AuthenticationScheme
                                                         : null;
                }, options =>
                {
                    builder.Configuration.GetSection("AzureB2C_Demo_API").Bind(options);
                },
                JwtBearerDefaults.AuthenticationScheme,
                true);

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var authFailed = options.Events.OnAuthenticationFailed;
    var tokenValidated = options.Events.OnTokenValidated;
    var msgReceived = options.Events.OnMessageReceived;
    var challenge = options.Events.OnChallenge;

    options.Events.OnAuthenticationFailed = async context =>
    {
        await authFailed(context);
    };
    options.Events.OnTokenValidated = async context =>
    {
        await tokenValidated(context);
    };
    options.Events.OnMessageReceived = async context =>
    {
        await msgReceived(context);
    };
    options.Events.OnChallenge = async context =>
    {
        await challenge(context);
    };
});

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

builder.Services.AddCors(options => 
{
    options.AddDefaultPolicy(policy => 
    {
        policy.AllowCredentials()
              .WithMethods(HttpMethods.Post, HttpMethods.Options)
              .AllowAnyHeader()
              .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[0]);
    });
});

builder.Services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();
builder.Services.AddRazorPages();

builder.Services.AddOptions();
builder.Services.Configure<OpenIdConnectOptions>(builder.Configuration.GetSection("AzureB2C_Demo_UI"));

// Enable both types of authentication/authorization
builder.Services.AddAuthorization(options =>
{
    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
            JwtBearerDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser();
    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
    
    options.AddPolicy(Policies.AuthorizedUser, _ => _.RequireAuthenticatedUser());
    options.AddPolicy(Policies.CharacterAccess, _ => _.RequireScopeClaim(ApiScope.CharacterRead, ApiScope.CharacterWrite));
    options.AddPolicy(Policies.CharacterWriteAccess, _ => _.RequireScopeClaim(ApiScope.CharacterWrite));
});

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
            //.AddAuthorizationRule()
            .AddWebSocketAuthentication<GraphQLWebSocketAuthService>();
});

var app = builder.Build();

#if DEBUG
app.UseDeveloperExceptionPage();
#endif

app.UseHttpsRedirection();
app.UseWebSockets();
app.UseCookiePolicy();

app.UseCors();
/*app.UseRewriter(
    new RewriteOptions()
        .AddRedirect("^$", "ui/graphiql")
);*/
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseGraphQL("/graphql", config => 
{
    config.AuthorizationRequired = true;
    //config.AuthorizedPolicy = Policies.AuthorizedUser;
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
