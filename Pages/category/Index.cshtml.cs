using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace TiShin.Pages.Category;

public class IndexModel : PageModel
{
    [FromRoute]
    public int? Id { get; set; }

    public void OnGet(int? id)
    {
        Id = id;
        // Static page for now; when dynamic, filter products by category id
    }
}