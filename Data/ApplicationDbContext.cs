using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Entities;

namespace TiShinShop.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductCategory> ProductCategories { get; set; }

        public DbSet<ProductSize> ProductSizes { get; set; }

        public DbSet<ProductColor> ProductColors { get; set; }

        public DbSet<ProductMaterial> ProductMaterials { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Discount> Discounts { get; set; }

        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<OrderAddress> OrderAddress { get; set; }

        public DbSet<Size> Sizes { get; set; }

        public DbSet<Color> Colors { get; set; }

        public DbSet<Material> Materials { get; set; }

        public DbSet<UserAddress> UserAddresses { get; set; }

        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

        public DbSet<ProductReview> ProductReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // تغییر نام جدول‌ها
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");  // تغییر نام جدول کاربران
            });

            builder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles");  // تغییر نام جدول نقش‌ها
            });

            builder.Entity<IdentityUserRole<int>>(entity =>
            {
                entity.ToTable("UserRoles");  // تغییر نام جدول پیوند کاربران و نقش‌ها
            });

            builder.Entity<IdentityUserClaim<int>>(entity =>
            {
                entity.ToTable("UserClaims");  // تغییر نام جدول اطلاعات کاربری
            });

            builder.Entity<IdentityUserLogin<int>>(entity =>
            {
                entity.ToTable("UserLogins");  // تغییر نام جدول لاگین‌های کاربر
            });

            builder.Entity<IdentityRoleClaim<int>>(entity =>
            {
                entity.ToTable("RoleClaims");  // تغییر نام جدول اطلاعات نقش‌ها
            });

            builder.Entity<IdentityUserToken<int>>(entity =>
            {
                entity.ToTable("UserTokens");  // تغییر نام جدول توکن‌های کاربری
            });

            builder.Entity<CartItem>()
                .HasOne(ci => ci.ProductMaterial)
                .WithMany()
                .HasForeignKey(ci => ci.ProductMaterialId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.ProductSize)
                .WithMany()
                .HasForeignKey(ci => ci.ProductSizeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.ProductColor)
                .WithMany()
                .HasForeignKey(ci => ci.ProductColorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderItem>()
                .HasOne(ci => ci.ProductMaterial)
                .WithMany()
                .HasForeignKey(ci => ci.ProductMaterialId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderItem>()
                .HasOne(ci => ci.ProductSize)
                .WithMany()
                .HasForeignKey(ci => ci.ProductSizeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderItem>()
                .HasOne(ci => ci.ProductColor)
                .WithMany()
                .HasForeignKey(ci => ci.ProductColorId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}