namespace InglemoorCodingComputing.Services;

using System.Reflection;

/// <summary>
/// Finds url routes for controllers and blazor.
/// </summary>
public class RouteAnalyzerService : IRouteAnalyzerService
{
    public RouteAnalyzerService(IEnumerable<EndpointDataSource> endpointSources)
    {
        // Find regular routes
        List<string> routes = new(endpointSources.SelectMany(x => x.Endpoints)
                                                 .OfType<RouteEndpoint>()
                                                 .Select(x => x.RoutePattern.RawText ?? string.Empty)
                                                 .Where(x => !string.IsNullOrEmpty(x) && x is not "{*path:nonfile}")
                                                 .Select(x => x[0] != '/' ? $"/{x}" : x)
                                                 .Select(x => x.ToLower()));

        // Find blazor routes via reflection
        routes.AddRange(Assembly.GetExecutingAssembly()
                                .GetTypes()
                                .Where(x => x.IsSubclassOf(typeof(Microsoft.AspNetCore.Components.ComponentBase)))
                                .SelectMany(x => x.GetCustomAttributes(inherit: true)
                                                  .OfType<Microsoft.AspNetCore.Components.RouteAttribute>())
                                .Select(x => x.Template)
                                .OfType<string>());
        routes.Sort();
        Routes = routes;
    }

    public IReadOnlyList<string> Routes { get; }
}
