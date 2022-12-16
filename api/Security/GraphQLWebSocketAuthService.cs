using GraphQL;
using GraphQL.Server.Transports.AspNetCore.WebSockets;
using GraphQL.Transport;

namespace StarWars.API.Security;

public class GraphQLWebSocketAuthService : IWebSocketAuthenticationService
{
    private readonly IGraphQLSerializer _serializer;

    public GraphQLWebSocketAuthService(IGraphQLSerializer serializer)
    {
        _serializer = serializer;
    }

    public Task AuthenticateAsync(IWebSocketConnection connection, string subProtocol, OperationMessage operationMessage)
    {
        // read payload of ConnectionInit message and look for an "Authorization" entry that starts with "Bearer "
        var payload = _serializer.ReadNode<Inputs>(operationMessage.Payload);
        if ((payload?.TryGetValue("Authorization", out var value) ?? false) && value is string valueString)
        {
            var user = valueString.ParseToken();
            if (user != null)
            {
                // set user and indicate authentication was successful
                connection.HttpContext.User = user;
            }
        }

        return Task.CompletedTask;
    } 
}