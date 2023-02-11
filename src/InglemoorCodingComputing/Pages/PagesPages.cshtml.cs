namespace InglemoorCodingComputing.Pages;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class PagesPagesModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? PageRoute { get; set; }
}
