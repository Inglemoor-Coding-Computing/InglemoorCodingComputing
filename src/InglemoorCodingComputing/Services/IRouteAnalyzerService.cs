namespace InglemoorCodingComputing.Services;

/// <summary>
/// Finds routes already in use.
/// </summary>
public interface IRouteAnalyzerService
{
    /// <summary>
    /// List of existing routes used by the application.
    /// </summary>
    IReadOnlyList<string> Routes { get; }
}
