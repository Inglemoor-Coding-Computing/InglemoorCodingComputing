namespace InglemoorCodingComputing.Services;

using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;

public sealed class URLShortenerEndpointDataSource : EndpointDataSource, IDisposable
{
    private readonly object _lock = new();

    private IReadOnlyList<Endpoint> _endpoints = Array.Empty<Endpoint>();

    private CancellationTokenSource _cancellationTokenSource = new();

    private IChangeToken _changeToken;

    private readonly IURLShortenerService _URLShortenerService;

    public URLShortenerEndpointDataSource(IURLShortenerService URLShortenerService)
    {
        _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);
        _URLShortenerService = URLShortenerService;
        _URLShortenerService.SpecialURLsChanged += OnSpecialURLsChanged;
        Task.Run(async () => OnSpecialURLsChanged(await _URLShortenerService.ReadAllSpecialAsync().ToListAsync()));
    }

    private void OnSpecialURLsChanged(IReadOnlyList<string> endpoints) =>
        SetEndpoints(endpoints.Select(x => new RouteEndpointBuilder(ctx => { ctx.Response.Redirect($"s/{x}"); return Task.CompletedTask; }, RoutePatternFactory.Parse($"/{x}"), 0).Build()).ToList());

    public override IChangeToken GetChangeToken() => _changeToken;

    public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

    private void SetEndpoints(IReadOnlyList<Endpoint> endpoints)
    {
        lock (_lock)
        {
            var oldCancellationTokenSource = _cancellationTokenSource;

            _endpoints = endpoints;

            _cancellationTokenSource = new();
            _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);

            oldCancellationTokenSource?.Cancel();
        }
    }

    public void Dispose() =>
        _URLShortenerService.SpecialURLsChanged -= OnSpecialURLsChanged;
}