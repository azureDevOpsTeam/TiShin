using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Pages.SellerPanel.Orders
{
    [Authorize(Roles = "Seller")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public EditModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)] public int Id { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> StatusOptions { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }
            public OrderStatus Status { get; set; }
            public string TrackingCode { get; set; }

            // Shipping address
            public string FullName { get; set; }
            public string Phone { get; set; }
            public string City { get; set; }
            public string Street { get; set; }
            public string FullAddress { get; set; }
            public string PostalCode { get; set; }

            public string? Note { get; set; }
        }

        private void FillStatusOptions()
        {
            StatusOptions = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString()
                }).ToList();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            FillStatusOptions();
            var order = await _db.Orders.Include(o => o.ShippingAddress).FirstOrDefaultAsync(o => o.Id == Id);
            if (order == null) return NotFound();

            Input = new InputModel
            {
                Id = order.Id,
                Status = order.Status,
                TrackingCode = order.TrackingCode,
                FullName = order.ShippingAddress?.FullName,
                Phone = order.ShippingAddress?.Phone,
                City = order.ShippingAddress?.City,
                Street = order.ShippingAddress?.Street,
                FullAddress = order.ShippingAddress?.FullAddress,
                PostalCode = order.ShippingAddress?.PostalCode
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            FillStatusOptions();
            if (!ModelState.IsValid) return Page();

            var order = await _db.Orders.Include(o => o.ShippingAddress).FirstOrDefaultAsync(o => o.Id == Input.Id);
            if (order == null) return NotFound();

            var oldStatus = order.Status;

            // Update order core fields
            order.Status = Input.Status;
            order.TrackingCode = Input.TrackingCode;

            // Update or create shipping address
            if (order.ShippingAddress == null)
            {
                order.ShippingAddress = new OrderAddress();
            }
            order.ShippingAddress.FullName = Input.FullName;
            order.ShippingAddress.Phone = Input.Phone;
            order.ShippingAddress.City = Input.City;
            order.ShippingAddress.Street = Input.Street;
            order.ShippingAddress.FullAddress = Input.FullAddress;
            order.ShippingAddress.PostalCode = Input.PostalCode;

            // If status changed, log history
            if (oldStatus != order.Status)
            {
                _db.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = order.Status,
                    ChangedAt = DateTime.UtcNow,
                    Note = string.IsNullOrWhiteSpace(Input.Note) ? null : Input.Note
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToPage("/seller-panel/orders/Index");
        }
    }
}