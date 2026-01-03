using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.ViewComponents
{
    public class MenuViewComponent(ApplicationDbContext context) : ViewComponent
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IViewComponentResult> InvokeAsync(bool mobile = false)
        {
            var query = await Task.Run(() => _context.Categories.AsQueryable());
            var categories = await query
                .Where(c => c.ParentId == 0)
                .ToListAsync();

            var menuItems = await query
                .Where(c => c.IsBaseMenu)
                .Select(parent => new MenuItemViewModel
                {
                    BaseMenu = categories,
                    Parent = parent,
                    Children = query.Where(c => c.ParentId == parent.Id).ToList()
                }).ToListAsync();

            if (mobile)
            {
                return View("Mobile", menuItems);
            }
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
