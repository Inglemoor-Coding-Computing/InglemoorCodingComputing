using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using InglemoorCodingComputing.Shared;

public static class ComponentRendererExtensions
{
    private static RenderFragment AddContent(Type type, string? route) => builder =>
    {
        builder.OpenComponent(0, typeof(MainLayout));
        builder.AddAttribute(1, "Body", Content(type, route));
        builder.CloseComponent();
    };
    private static RenderFragment Content(Type type, string? route) => builder =>
    {
        builder.OpenComponent(2, type);
        if (route is not null)
            builder.AddAttribute(3, "PageRoute", route);
        builder.CloseComponent();
    };
    public static Task<IHtmlContent> RenderComponentWithAuthAsync<T>(this IHtmlHelper<T> html, Type type, string? route = null)
    {
        return html.RenderComponentAsync<CascadingAuthenticationState>(RenderMode.Static, new { ChildContent = AddContent(type, route) });
    }
}