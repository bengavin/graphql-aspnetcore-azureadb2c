using GraphQL;
using StarWars.API.Schema;
using StarWars.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Application Services
builder.Services.AddSingleton<IStarWarsDataService, StarWarsDataService>();

// GraphQL
builder.Services.AddGraphQL(configure => 
{
    configure.AddSelfActivatingSchema<StarWarsSchema>(GraphQL.DI.ServiceLifetime.Scoped)
             .AddSystemTextJson()
             .AddScopedSubscriptionExecutionStrategy()
             .AddErrorInfoProvider();
});

var app = builder.Build();

#if DEBUG
app.UseDeveloperExceptionPage();
#endif

app.UseHttpsRedirection();
app.UseWebSockets();
app.UseGraphQL("/graphql");
app.UseGraphQLGraphiQL(
    "/ui/graphiql",
    options: new GraphQL.Server.Ui.GraphiQL.GraphiQLOptions
    {
        GraphQLEndPoint = "/graphql",
        SubscriptionsEndPoint = "/graphql"
    });

await app.RunAsync();
