using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;

namespace TiShinShop.ViewComponents
{
    public class CategoriesViewComponent(ApplicationDbContext context) : ViewComponent
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuItems = await _context.Categories
                .Where(c => !c.IsBaseMenu && c.ParentId != 0)
                .Select(parent => new CategoriesItemViewModel
                {
                    Id = parent.Id,
                    Title = parent.Name
                }).ToListAsync();

            return View(menuItems);
        }
    }

    public class CategoriesItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }
}