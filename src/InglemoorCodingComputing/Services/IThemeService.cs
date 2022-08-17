namespace InglemoorCodingComputing.Services;

public interface IThemeService
{
    Theme Theme { get; set; }

    event EventHandler<Theme> OnThemeChanged;
}
