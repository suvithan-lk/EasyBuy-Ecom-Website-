using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public sealed class EasyBuyDbContext(DbContextOptions<EasyBuyDbContext> options) : DbContext(options)
{
    public DbSet<SiteSettingsEntity> SiteSettings => Set<SiteSettingsEntity>();
    public DbSet<SiteFeatureCalloutEntity> SiteFeatureCallouts => Set<SiteFeatureCalloutEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<ProductImageEntity> ProductImages => Set<ProductImageEntity>();
    public DbSet<ProductTagEntity> ProductTags => Set<ProductTagEntity>();
    public DbSet<ProductSpecEntity> ProductSpecs => Set<ProductSpecEntity>();
    public DbSet<CouponEntity> Coupons => Set<CouponEntity>();
    public DbSet<CartItemEntity> CartItems => Set<CartItemEntity>();
    public DbSet<WishlistItemEntity> WishlistItems => Set<WishlistItemEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    public DbSet<OrderTrackingEventEntity> OrderTrackingEvents => Set<OrderTrackingEventEntity>();
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SiteSettingsEntity>(entity =>
        {
            entity.ToTable("SiteSettings");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).ValueGeneratedNever();
            entity.Property(item => item.Brand).HasMaxLength(80);
            entity.Property(item => item.Headline).HasMaxLength(240);
            entity.Property(item => item.Subheadline).HasMaxLength(320);
            entity.Property(item => item.FreeShippingThreshold).HasPrecision(10, 2);
            entity.Property(item => item.StandardShippingRate).HasPrecision(10, 2);
        });

        modelBuilder.Entity<SiteFeatureCalloutEntity>(entity =>
        {
            entity.ToTable("SiteFeatureCallouts");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Text).HasMaxLength(180);
            entity.HasOne(item => item.SiteSettings)
                .WithMany(item => item.FeatureCallouts)
                .HasForeignKey(item => item.SiteSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasMaxLength(50);
            entity.Property(item => item.Email).HasMaxLength(255);
            entity.Property(item => item.Name).HasMaxLength(120);
            entity.Property(item => item.Phone).HasMaxLength(30);
            entity.Property(item => item.Role).HasMaxLength(20);
            entity.HasIndex(item => item.Email).IsUnique();
        });

        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).ValueGeneratedNever();
            entity.Property(item => item.Name).HasMaxLength(120);
            entity.Property(item => item.Slug).HasMaxLength(150);
            entity.Property(item => item.Description).HasMaxLength(255);
            entity.Property(item => item.ImageUrl).HasMaxLength(500);
            entity.HasIndex(item => item.Slug).IsUnique();
        });

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).ValueGeneratedNever();
            entity.Property(item => item.Name).HasMaxLength(150);
            entity.Property(item => item.Slug).HasMaxLength(180);
            entity.Property(item => item.Summary).HasMaxLength(255);
            entity.Property(item => item.Price).HasPrecision(10, 2);
            entity.Property(item => item.CompareAtPrice).HasPrecision(10, 2);
            entity.Property(item => item.Badge).HasMaxLength(50);
            entity.Property(item => item.Accent).HasMaxLength(200);
            entity.HasIndex(item => item.Slug).IsUnique();
            entity.HasIndex(item => item.CategoryId);
            entity.HasIndex(item => item.Name);
            entity.HasOne(item => item.Category)
                .WithMany(item => item.Products)
                .HasForeignKey(item => item.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImageEntity>(entity =>
        {
            entity.ToTable("ProductImages");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ImageUrl).HasMaxLength(500);
            entity.HasIndex(item => new { item.ProductId, item.SortOrder });
            entity.HasOne(item => item.Product)
                .WithMany(item => item.Images)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductTagEntity>(entity =>
        {
            entity.ToTable("ProductTags");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Value).HasMaxLength(80);
            entity.HasIndex(item => new { item.ProductId, item.SortOrder }).IsUnique();
            entity.HasOne(item => item.Product)
                .WithMany(item => item.Tags)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductSpecEntity>(entity =>
        {
            entity.ToTable("ProductSpecs");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Label).HasMaxLength(80);
            entity.Property(item => item.Value).HasMaxLength(120);
            entity.HasIndex(item => new { item.ProductId, item.SortOrder }).IsUnique();
            entity.HasOne(item => item.Product)
                .WithMany(item => item.Specs)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CouponEntity>(entity =>
        {
            entity.ToTable("Coupons");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).ValueGeneratedNever();
            entity.Property(item => item.Code).HasMaxLength(50);
            entity.Property(item => item.Description).HasMaxLength(180);
            entity.Property(item => item.DiscountType).HasMaxLength(20);
            entity.Property(item => item.DiscountValue).HasPrecision(10, 2);
            entity.Property(item => item.MinimumSpend).HasPrecision(10, 2);
            entity.HasIndex(item => item.Code).IsUnique();
        });

        modelBuilder.Entity<CartItemEntity>(entity =>
        {
            entity.ToTable("Cart");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.UserId).HasMaxLength(50);
            entity.HasIndex(item => new { item.UserId, item.ProductId }).IsUnique();
            entity.HasOne(item => item.User)
                .WithMany(item => item.CartItems)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Product)
                .WithMany(item => item.CartItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WishlistItemEntity>(entity =>
        {
            entity.ToTable("Wishlist");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.UserId).HasMaxLength(50);
            entity.HasIndex(item => new { item.UserId, item.ProductId }).IsUnique();
            entity.HasOne(item => item.User)
                .WithMany(item => item.WishlistItems)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Product)
                .WithMany(item => item.WishlistItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).ValueGeneratedNever();
            entity.Property(item => item.OrderNumber).HasMaxLength(50);
            entity.Property(item => item.UserId).HasMaxLength(50);
            entity.Property(item => item.CustomerName).HasMaxLength(120);
            entity.Property(item => item.CustomerEmail).HasMaxLength(255);
            entity.Property(item => item.SubtotalAmount).HasPrecision(10, 2);
            entity.Property(item => item.TotalAmount).HasPrecision(10, 2);
            entity.Property(item => item.DiscountAmount).HasPrecision(10, 2);
            entity.Property(item => item.Status).HasMaxLength(20);
            entity.Property(item => item.PaymentMethod).HasMaxLength(50);
            entity.Property(item => item.TrackingNumber).HasMaxLength(80);
            entity.Property(item => item.CourierName).HasMaxLength(100);
            entity.Property(item => item.ShippingAddress).HasMaxLength(255);
            entity.Property(item => item.ShippingCost).HasPrecision(10, 2);
            entity.Property(item => item.Notes).HasMaxLength(300);
            entity.HasIndex(item => item.OrderNumber).IsUnique();
            entity.HasIndex(item => item.UserId);
            entity.HasOne(item => item.User)
                .WithMany(item => item.Orders)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Coupon)
                .WithMany(item => item.Orders)
                .HasForeignKey(item => item.CouponId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItemEntity>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ProductName).HasMaxLength(150);
            entity.Property(item => item.UnitPrice).HasPrecision(10, 2);
            entity.HasOne(item => item.Order)
                .WithMany(item => item.Items)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Product)
                .WithMany(item => item.OrderItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderTrackingEventEntity>(entity =>
        {
            entity.ToTable("OrderTrackingEvents");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.EventCode).HasMaxLength(50);
            entity.Property(item => item.Title).HasMaxLength(120);
            entity.Property(item => item.Description).HasMaxLength(255);
            entity.Property(item => item.Location).HasMaxLength(150);
            entity.HasOne(item => item.Order)
                .WithMany(item => item.TrackingEvents)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.GatewayPaymentId).HasMaxLength(100);
            entity.Property(item => item.Amount).HasPrecision(10, 2);
            entity.Property(item => item.Status).HasMaxLength(20);
            entity.HasIndex(item => item.OrderId).IsUnique();
            entity.HasOne(item => item.Order)
                .WithOne(item => item.Payment)
                .HasForeignKey<PaymentEntity>(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewEntity>(entity =>
        {
            entity.ToTable("Reviews");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.UserId).HasMaxLength(50);
            entity.Property(item => item.Comment).HasMaxLength(1000);
            entity.Property(item => item.Headline).HasMaxLength(150);
            entity.HasIndex(item => item.ProductId);
            entity.HasOne(item => item.User)
                .WithMany(item => item.Reviews)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Product)
                .WithMany(item => item.Reviews)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
