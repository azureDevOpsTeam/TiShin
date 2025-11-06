using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.ViewComponents
{
    public class MenuViewComponent(ApplicationDbContext context) : ViewComponent
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var query = await Task.Run(() => _context.Categories.AsQueryable());
            // Base categories used for left list and panel grouping (landing.html pattern)
            var baseCategories = await query
                .Where(c => c.ParentId == 0)
                .ToListAsync();

            // Build menu items keyed by each base category id
            var menuItems = baseCategories
                .Select(parent => new MenuItemViewModel
                {
                    BaseMenu = baseCategories,
                    Parent = parent,
                    Children = query.Where(c => c.ParentId == parent.Id).ToList()
                })
                .ToList();

            return View(menuItems);
        }
    }

    public class MenuItemViewModel
    {
        public List<Category> BaseMenu { get; set; }

        public Category Parent { get; set; }

        public List<Category> Children { get; set; }
    }
}