using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace StarWars.API.Controllers;

[Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
[AuthorizeForScopes(ScopeKeySection = "GraphiQL:ApiScopes")]
public class GraphiqlController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ITokenAcquisition _tokens;

    public class GraphiqlViewModel
    {
        public string? GraphQLEndPoint { get; set; }
        public HtmlString? DefaultQuery { get; set; }
        public HtmlString? Headers { get; set; }
    }

    public GraphiqlController(IConfiguration configuration, ITokenAcquisition tokens)
    {
        _configuration = configuration;
        _tokens = tokens;
    }

    [Route("ui/graphiql")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var accessToken = await _tokens.GetAccessTokenForUserAsync(
                            (_configuration["GraphiQL:ApiScopes"] ?? string.Empty)
                            .Split(new [] { ' ', ',' }));

        return View(new GraphiqlViewModel
        {
            GraphQLEndPoint = $"{_configuration["GraphiQL:BaseUri"]}/graphql",
            DefaultQuery = new HtmlString("query getHeroes {\n  hero {\n    id\n    name\n  }\n}"),
            Headers = new HtmlString(JsonSerializer.Serialize<object>(new Dictionary<string, string>()
            {
                { "Accept", "application/json" },
                { "Authorization", $"Bearer {accessToken}" }
            }))
        });
    }
}
