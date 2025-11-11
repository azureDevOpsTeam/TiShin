using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Reports
{
    [Authorize(Roles = "Seller")]
    public class CustomersModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CustomersModel(ApplicationDbContext db) { _db = db; }

        [BindProperty(SupportsGet = true)] public string CustomerType { get; set; } = "all"; // all, new, loyal, inactive
        [BindProperty(SupportsGet = true)] public string City { get; set; }
        [BindProperty(SupportsGet = true)] public int? MinPurchases { get; set; }
        [BindProperty(SupportsGet = true)] public string Search { get; set; }
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

        public List<string> AvailableCities { get; set; } = new();

        public List<CustomerRow> Customers { get; set; } = new();

        public StatsModel Stats { get; set; } = new();

        public int TotalFilteredCustomers { get; set; }
        public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalFilteredCustomers / PageSize));
        public int PageStart => TotalFilteredCustomers == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
        public int PageEnd => Math.Min(PageNumber * PageSize, TotalFilteredCustomers);

        public List<string> GeoLabels { get; set; } = new();
        public List<int> GeoData { get; set; } = new();
        public List<string> RegistrationLabels { get; set; } = new();
        public List<int> RegistrationData { get; set; } = new();

        public async Task OnGet()
        {
            // Load orders with related user and address for computation in memory.
            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .AsNoTracking()
                .ToListAsync();

            // Available cities for filter dropdown from ShippingAddress
            AvailableCities = orders
                .Select(o => o.ShippingAddress?.City)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Group orders per customer
            var grouped = orders
                .GroupBy(o => o.UserId)
                .Select(g => new CustomerRow
                {
                    UserId = g.Key,
                    FullName = BuildName(g.First().User),
                    Email = g.First().User?.Email ?? string.Empty,
                    Phone = g.Select(x => x.ShippingAddress?.Phone).FirstOrDefault() ?? g.First().User?.PhoneNumber ?? string.Empty,
                    City = g.Select(x => x.ShippingAddress?.City).FirstOrDefault() ?? string.Empty,
                    PurchaseCount = g.Count(),
                    TotalSpent = g.Sum(x => x.TotalPrice),
                    LastPurchase = g.Max(x => x.CreatedAt),
                    CreatedAt = g.First().User?.CreatedAt ?? DateTime.UtcNow
                })
                .ToList();

            // Apply filters
            grouped = ApplyFilters(grouped);

            // Search
            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                grouped = grouped.Where(c =>
                    (!string.IsNullOrEmpty(c.FullName) && c.FullName.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.Email) && c.Email.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.Phone) && c.Phone.Contains(s, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // Stats (based on current filtered list)
            Stats.TotalCustomers = grouped.Count;
            Stats.AveragePurchases = grouped.Count == 0 ? 0 : grouped.Average(c => c.PurchaseCount);
            var activeThreshold = DateTime.UtcNow.AddDays(-60);
            Stats.ActiveCustomers = grouped.Count(c => c.LastPurchase >= activeThreshold);
            var repeaters = grouped.Count(c => c.PurchaseCount > 1);
            Stats.RetentionRate = grouped.Count == 0 ? 0 : (double)repeaters / grouped.Count;

            // Chart data: geo (top cities) and registration trend (last 6 months)
            var cityCounts = grouped
                .Where(c => !string.IsNullOrWhiteSpace(c.City))
                .GroupBy(c => c.City)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();
            GeoLabels = cityCounts.Select(x => x.City).ToList();
            GeoData = cityCounts.Select(x => x.Count).ToList();

            var now = DateTime.UtcNow;
            var months = Enumerable.Range(0, 6).Select(i => now.AddMonths(-i)).OrderBy(d => d).ToList();
            RegistrationLabels = months.Select(m => PersianMonthName(m.Month)).ToList();
            RegistrationData = months.Select(m => grouped.Count(c => c.CreatedAt.Month == m.Month && c.CreatedAt.Year == m.Year)).ToList();

            // Pagination
            TotalFilteredCustomers = grouped.Count;
            PageNumber = Math.Max(1, PageNumber);
            PageSize = new[] { 10, 25, 50 }.Contains(PageSize) ? PageSize : 10;
            Customers = grouped
                .OrderByDescending(c => c.LastPurchase)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public IActionResult OnGetExport()
        {
            // Export current filter as CSV
            var orders = _db.Orders.Include(o => o.User).Include(o => o.ShippingAddress).AsNoTracking().ToList();
            var grouped = orders.GroupBy(o => o.UserId).Select(g => new CustomerRow
            {
                UserId = g.Key,
                FullName = BuildName(g.First().User),
                Email = g.First().User?.Email ?? string.Empty,
                Phone = g.Select(x => x.ShippingAddress?.Phone).FirstOrDefault() ?? g.First().User?.PhoneNumber ?? string.Empty,
                City = g.Select(x => x.ShippingAddress?.City).FirstOrDefault() ?? string.Empty,
                PurchaseCount = g.Count(),
                TotalSpent = g.Sum(x => x.TotalPrice),
                LastPurchase = g.Max(x => x.CreatedAt),
                CreatedAt = g.First().User?.CreatedAt ?? DateTime.UtcNow
            }).ToList();

            grouped = ApplyFilters(grouped);
            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                grouped = grouped.Where(c =>
                    (!string.IsNullOrEmpty(c.FullName) && c.FullName.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.Email) && c.Email.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.Phone) && c.Phone.Contains(s, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            var sb = new StringBuilder();
            sb.AppendLine("FullName,Email,Phone,City,PurchaseCount,TotalSpent,LastPurchase");
            foreach (var c in grouped.OrderByDescending(c => c.LastPurchase))
            {
                sb.AppendLine($"{EscapeCsv(c.FullName)},{EscapeCsv(c.Email)},{EscapeCsv(c.Phone)},{EscapeCsv(c.City)},{c.PurchaseCount},{c.TotalSpent},{c.LastPurchase:yyyy-MM-dd}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "customer-report.csv");
        }

        private List<CustomerRow> ApplyFilters(List<CustomerRow> list)
        {
            if (!string.IsNullOrWhiteSpace(City))
            {
                list = list.Where(c => string.Equals(c.City, City, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (MinPurchases.HasValue)
            {
                list = list.Where(c => c.PurchaseCount >= MinPurchases.Value).ToList();
            }
            var now = DateTime.UtcNow;
            switch (CustomerType)
            {
                case "new":
                    var newThreshold = now.AddDays(-30);
                    list = list.Where(c => c.CreatedAt >= newThreshold).ToList();
                    break;
                case "loyal":
                    list = list.Where(c => c.PurchaseCount >= 3).ToList();
                    break;
                case "inactive":
                    var inactiveThreshold = now.AddDays(-90);
                    list = list.Where(c => c.LastPurchase < inactiveThreshold).ToList();
                    break;
            }
            return list;
        }

        private static string BuildName(ApplicationUser u)
        {
            if (u == null) return string.Empty;
            var parts = new[] { u.FirstName, u.LastName }.Where(p => !string.IsNullOrWhiteSpace(p));
            var name = string.Join(" ", parts);
            return string.IsNullOrWhiteSpace(name) ? u.UserName ?? u.Email ?? "" : name;
        }

        private static string PersianMonthName(int month)
        {
            // Simple Persian month names mapping
            var names = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
            return names[(month - 1) % 12];
        }

        private static string EscapeCsv(string input)
        {
            if (input == null) return "";
            var needsQuotes = input.Contains(',') || input.Contains('"') || input.Contains('\n');
            input = input.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{input}\"" : input;
        }

        public class CustomerRow
        {
            public int UserId { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string City { get; set; }
            public int PurchaseCount { get; set; }
            public decimal TotalSpent { get; set; }
            public DateTime LastPurchase { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class StatsModel
        {
            public int TotalCustomers { get; set; }
            public double AveragePurchases { get; set; }
            public int ActiveCustomers { get; set; }
            public double RetentionRate { get; set; }
        }
    }
}