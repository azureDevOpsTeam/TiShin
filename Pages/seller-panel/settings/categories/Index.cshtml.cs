using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Categories;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) { _db = db; }

    public IList<CategoryItem> Items { get; set; } = new List<CategoryItem>();

    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ParentId { get; set; }
        public string ParentName { get; set; } = "—";
        public bool IsBaseMenu { get; set; }
    }

    public async Task OnGet()
    {
        var cats = await _db.Categories.AsNoTracking().ToListAsync();
        var dict = cats.ToDictionary(c => c.Id, c => c);
        Items = cats.Select(c => new CategoryItem
        {
            Id = c.Id,
            Name = c.Name,
            ParentId = c.ParentId,
            ParentName = c.ParentId == 0 ? "—" : (dict.TryGetValue(c.ParentId, out var p) ? p.Name : "نامشخص"),
            IsBaseMenu = c.IsBaseMenu
        }).OrderBy(i => i.ParentId).ThenBy(i => i.Name).ToList();
    }
}