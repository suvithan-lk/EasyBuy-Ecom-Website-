using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class SeedData
{
    public static async Task InitializeAsync(EasyBuyDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.SiteSettings.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        dbContext.SiteSettings.Add(new SiteSettingsEntity
        {
            Id = 1,
            Brand = "EasyBuy",
            Headline = "Curated tech for focused work, clean desks, and better downtime.",
            Subheadline = "Browse a fully wired storefront with smart filtering, wishlist tracking, checkout, and order updates backed by SQL Server.",
            FreeShippingThreshold = 250m,
            StandardShippingRate = 18m,
            FeatureCallouts =
            [
                new SiteFeatureCalloutEntity { Text = "Same-day dispatch on compact gear", SortOrder = 1 },
                new SiteFeatureCalloutEntity { Text = "Live stock signals on every product", SortOrder = 2 },
                new SiteFeatureCalloutEntity { Text = "Admin metrics generated from actual orders", SortOrder = 3 }
            ]
        });

        dbContext.Users.AddRange(
        [
            new UserEntity
            {
                Id = "admin-001",
                Email = "admin@easybuy.demo",
                Name = "EasyBuy Admin",
                Phone = "+94 77 000 0000",
                Role = "Admin",
                CreatedAt = now.AddMonths(-6)
            },
            new UserEntity
            {
                Id = "customer-001",
                Email = "ayesha@easybuy.demo",
                Name = "Ayesha Fernando",
                Phone = "+94 71 555 0101",
                Role = "Customer",
                CreatedAt = now.AddMonths(-4)
            },
            CreateReviewer("nimali-001", "Nimali", "nimali@easybuy.demo", now.AddMonths(-5)),
            CreateReviewer("hasith-001", "Hasith", "hasith@easybuy.demo", now.AddMonths(-5)),
            CreateReviewer("kavindu-001", "Kavindu", "kavindu@easybuy.demo", now.AddMonths(-4)),
            CreateReviewer("roshani-001", "Roshani", "roshani@easybuy.demo", now.AddMonths(-4)),
            CreateReviewer("tharushi-001", "Tharushi", "tharushi@easybuy.demo", now.AddMonths(-4)),
            CreateReviewer("janidu-001", "Janidu", "janidu@easybuy.demo", now.AddMonths(-4)),
            CreateReviewer("pasindu-001", "Pasindu", "pasindu@easybuy.demo", now.AddMonths(-3)),
            CreateReviewer("lakshika-001", "Lakshika", "lakshika@easybuy.demo", now.AddMonths(-3)),
            CreateReviewer("dilan-001", "Dilan", "dilan@easybuy.demo", now.AddMonths(-3)),
            CreateReviewer("udari-001", "Udari", "udari@easybuy.demo", now.AddMonths(-3)),
            CreateReviewer("mihiri-001", "Mihiri", "mihiri@easybuy.demo", now.AddMonths(-2)),
            CreateReviewer("ravin-001", "Ravin", "ravin@easybuy.demo", now.AddMonths(-2)),
            CreateReviewer("amali-001", "Amali", "amali@easybuy.demo", now.AddMonths(-2))
        ]);

        dbContext.Categories.AddRange(
        [
            new CategoryEntity
            {
                Id = 1,
                Name = "Audio",
                Slug = "audio",
                Description = "Headphones, speakers, and compact listening gear.",
                ImageUrl = "https://unsplash.com/photos/gNnJ3e7yISE/download?force=true&w=900"
            },
            new CategoryEntity
            {
                Id = 2,
                Name = "Workstations",
                Slug = "workstations",
                Description = "Keyboards, docks, and productivity hardware.",
                ImageUrl = "https://unsplash.com/photos/CjFZM1IiGw4/download?force=true&w=900"
            },
            new CategoryEntity
            {
                Id = 3,
                Name = "Home Office",
                Slug = "home-office",
                Description = "Desk essentials built for long sessions.",
                ImageUrl = "https://unsplash.com/photos/JiQA3t5Q5bo/download?force=true&w=900"
            },
            new CategoryEntity
            {
                Id = 4,
                Name = "Wearables",
                Slug = "wearables",
                Description = "Smart devices for motion, sleep, and focus.",
                ImageUrl = "https://unsplash.com/photos/TChQAfBsLAc/download?force=true&w=900"
            }
        ]);

        dbContext.Products.AddRange(
        [
            CreateProduct(
                id: 101,
                name: "Nebula ANC Headphones",
                slug: "nebula-anc-headphones",
                summary: "Studio-grade wireless headphones with adaptive noise control.",
                description: "Nebula blends wide soundstage tuning, low-latency Bluetooth, and all-day comfort for focused work or travel.",
                categoryId: 1,
                price: 189m,
                compareAtPrice: 239m,
                stock: 14,
                rating: 4.8,
                reviewCount: 186,
                warrantyMonths: 24,
                isFeatured: true,
                isNewArrival: false,
                accent: "linear-gradient(135deg, #13293d 0%, #1b4965 50%, #5fa8d3 100%)",
                badge: "Bestseller",
                shippingDays: 2,
                createdAt: now.AddDays(-46),
                images:
                [
                    ("https://unsplash.com/photos/gNnJ3e7yISE/download?force=true&w=1200", true),
                    ("https://unsplash.com/photos/7Sm1wHWKjcE/download?force=true&w=1200", false)
                ],
                tags: ["wireless", "noise cancelling", "travel"],
                specs:
                [
                    ("Battery", "42 hours"),
                    ("Drivers", "40 mm titanium"),
                    ("Charging", "USB-C fast charge")
                ],
                reviews:
                [
                    CreateReview("nimali-001", 5, "Quiet on demand", "Blocks bus noise without flattening vocals.", now.AddDays(-25)),
                    CreateReview("hasith-001", 4, "Great for deep work", "Strong battery life and very comfortable clamp.", now.AddDays(-43))
                ]),
            CreateProduct(
                id: 102,
                name: "Atlas Mechanical Keyboard",
                slug: "atlas-mechanical-keyboard",
                summary: "Low-profile aluminum keyboard with tactile silent switches.",
                description: "Atlas is tuned for writers and coders who want precision, damping, and clean desk aesthetics without gaming excess.",
                categoryId: 2,
                price: 129m,
                compareAtPrice: 159m,
                stock: 21,
                rating: 4.7,
                reviewCount: 142,
                warrantyMonths: 18,
                isFeatured: true,
                isNewArrival: true,
                accent: "linear-gradient(135deg, #302b63 0%, #0f0c29 100%)",
                badge: "New",
                shippingDays: 3,
                createdAt: now.AddDays(-21),
                images:
                [
                    ("https://unsplash.com/photos/CjFZM1IiGw4/download?force=true&w=1200", true),
                    ("https://unsplash.com/photos/JiQA3t5Q5bo/download?force=true&w=1200", false)
                ],
                tags: ["mechanical", "bluetooth", "mac/win"],
                specs:
                [
                    ("Layout", "75 percent"),
                    ("Connection", "Tri-mode wireless"),
                    ("Frame", "CNC aluminum")
                ],
                reviews:
                [
                    CreateReview("kavindu-001", 5, "Very clean typing feel", "Feels premium and the acoustics are tighter than expected.", now.AddDays(-18)),
                    CreateReview("roshani-001", 4, "Fast to switch devices", "Perfect if you move between laptop and tablet.", now.AddDays(-31))
                ]),
            CreateProduct(
                id: 103,
                name: "Pulse 4K Webcam",
                slug: "pulse-4k-webcam",
                summary: "Ultra-clear webcam with auto-framing and low-light correction.",
                description: "Pulse keeps remote meetings sharp with a wide sensor, privacy shutter, and software-free framing presets.",
                categoryId: 2,
                price: 99m,
                compareAtPrice: 129m,
                stock: 32,
                rating: 4.6,
                reviewCount: 94,
                warrantyMonths: 12,
                isFeatured: true,
                isNewArrival: false,
                accent: "linear-gradient(135deg, #1f4037 0%, #99f2c8 100%)",
                badge: "Remote Ready",
                shippingDays: 2,
                createdAt: now.AddDays(-54),
                images:
                [
                    ("https://unsplash.com/photos/HENZpJ-KWg0/download?force=true&w=1200", true),
                    ("https://unsplash.com/photos/Z-rhpsLItec/download?force=true&w=1200", false)
                ],
                tags: ["streaming", "meetings", "4k"],
                specs:
                [
                    ("Resolution", "4K30 / 1080p60"),
                    ("Lens", "95 degree field of view"),
                    ("Privacy", "Integrated shutter")
                ],
                reviews:
                [
                    CreateReview("tharushi-001", 5, "Looks better than my laptop cam by a mile", "Auto framing is subtle and useful.", now.AddDays(-59)),
                    CreateReview("janidu-001", 4, "Plug and call", "Worked immediately on Windows and Teams.", now.AddDays(-71))
                ]),
            CreateProduct(
                id: 104,
                name: "Dock One 12-in-1 Hub",
                slug: "dock-one-12-in-1-hub",
                summary: "Single-cable desktop dock with dual-display output and Ethernet.",
                description: "Dock One anchors a hybrid desk setup with HDMI, DisplayPort, SD, USB-A, USB-C, and gigabit Ethernet in a slim wedge chassis.",
                categoryId: 2,
                price: 149m,
                compareAtPrice: 179m,
                stock: 8,
                rating: 4.5,
                reviewCount: 63,
                warrantyMonths: 24,
                isFeatured: false,
                isNewArrival: true,
                accent: "linear-gradient(135deg, #16222a 0%, #3a6073 100%)",
                badge: "Hybrid Desk",
                shippingDays: 4,
                createdAt: now.AddDays(-14),
                images:
                [
                    ("https://unsplash.com/photos/MDHLwA3Awb8/download?force=true&w=1200", true),
                    ("https://unsplash.com/photos/WzIBr55iRwM/download?force=true&w=1200", false)
                ],
                tags: ["usb-c", "dual monitor", "ethernet"],
                specs:
                [
                    ("Ports", "12 total"),
                    ("Displays", "Dual 4K"),
                    ("Power", "100W passthrough")
                ],
                reviews:
                [
                    CreateReview("pasindu-001", 4, "One cable, clean setup", "Saved a lot of desk clutter instantly.", now.AddDays(-23))
                ]),
            CreateProduct(
                id: 105,
                name: "GlowDesk Task Lamp",
                slug: "glowdesk-task-lamp",
                summary: "Matte steel lamp with ambient edge light and USB-C charging base.",
                description: "GlowDesk balances warm desk lighting with a thin silhouette, touch dimmer, and integrated charging pad for phones or earbuds.",
                categoryId: 3,
                price: 79m,
                compareAtPrice: 99m,
                stock: 26,
                rating: 4.4,
                reviewCount: 57,
                warrantyMonths: 12,
                isFeatured: false,
                isNewArrival: false,
                accent: "linear-gradient(135deg, #614385 0%, #516395 100%)",
                badge: "Desk Upgrade",
                shippingDays: 2,
                createdAt: now.AddDays(-37),
                images:
                [
                    ("https://unsplash.com/photos/CwvWsr8s7Bk/download?force=true&w=1200", true),
                    ("https://unsplash.com/photos/DqHgAfilZwI/download?force=true&w=1200", false)
                ],
                tags: ["lighting", "usb-c", "workspace"],
                specs:
                [
                    ("Brightness", "5 dim levels"),
                    ("Color modes", "Warm / neutral / cool"),
                    ("Charging", "15W wireless pad")
                ],
                reviews:
                [
                    CreateReview("lakshika-001", 4, "Warm light, no glare", "Good for late-night desk sessions.", now.AddDays(-55))
                ]),
            CreateProduct(
                id: 106,
                name: "Motion Ergonomic Chair",
                slug: "motion-ergonomic-chair",
                summary: "Breathable mesh chair with dynamic lumbar support and lockable recline.",
                description: "Motion is tuned for long workdays with flexible lumbar support, waterfall seat geometry, and quiet multi-surface casters.",
                categoryId: 3,
                price: 329m,
                compareAtPrice: 399m,
                stock: 5,
                rating: 4.9,
                reviewCount: 74,
                warrantyMonths: 36,
                isFeatured: true,
                isNewArrival: false,
                accent: "linear-gradient(135deg, #000428 0%, #004e92 100%)",
                badge: "Editor Pick",
                shippingDays: 5,
                createdAt: now.AddDays(-63),
                images:
                [
                    ("https://unsplash.com/photos/ouU8_n9aIlA/download?force=true&w=1200", true),
                    ("https://unsplash.com/photos/JiQA3t5Q5bo/download?force=true&w=1200", false)
                ],
                tags: ["ergonomic", "mesh", "premium"],
                specs:
                [
                    ("Support", "Dynamic lumbar"),
                    ("Armrests", "4D adjustable"),
                    ("Frame", "Reinforced nylon + steel")
                ],
                reviews:
                [
                    CreateReview("dilan-001", 5, "Worth the price", "The back support is noticeably better than budget chairs.", now.AddDays(-39)),
                    CreateReview("udari-001", 5, "Long-day comfortable", "Solved lower-back fatigue after a week.", now.AddDays(-62))
                ]),
            CreateProduct(
                id: 107,
                name: "Verse Smartwatch",
                slug: "verse-smartwatch",
                summary: "Fitness-forward smartwatch with sleep coaching and dual-band GPS.",
                description: "Verse focuses on recovery, movement, and clean notifications with a bright AMOLED display and five-day battery life.",
                categoryId: 4,
                price: 219m,
                compareAtPrice: 269m,
                stock: 17,
                rating: 4.7,
                reviewCount: 121,
                warrantyMonths: 24,
                isFeatured: true,
                isNewArrival: true,
                accent: "linear-gradient(135deg, #1d4350 0%, #a43931 100%)",
                badge: "Fitness Lab",
                shippingDays: 2,
                createdAt: now.AddDays(-11),
                images:
                [
                    ("https://unsplash.com/photos/TChQAfBsLAc/download?force=true&w=1200", true),
                    ("https://unsplash.com/photos/jT5mSPEteVc/download?force=true&w=1200", false)
                ],
                tags: ["fitness", "gps", "sleep tracking"],
                specs:
                [
                    ("Display", "1.43 inch AMOLED"),
                    ("Battery", "Up to 5 days"),
                    ("Sensors", "ECG, SpO2, temperature")
                ],
                reviews:
                [
                    CreateReview("mihiri-001", 5, "Better recovery insights than expected", "Sleep trend view is actually useful.", now.AddDays(-22)),
                    CreateReview("ravin-001", 4, "Screen is excellent outdoors", "GPS locks quickly during runs.", now.AddDays(-48))
                ]),
            CreateProduct(
                id: 108,
                name: "Arc Mini Speaker",
                slug: "arc-mini-speaker",
                summary: "Portable room speaker with rich mids and carry-anywhere footprint.",
                description: "Arc Mini fills a bedroom or studio desk with balanced sound and simple stereo pairing without needing a bulky cabinet.",
                categoryId: 1,
                price: 69m,
                compareAtPrice: 89m,
                stock: 40,
                rating: 4.3,
                reviewCount: 52,
                warrantyMonths: 12,
                isFeatured: false,
                isNewArrival: false,
                accent: "linear-gradient(135deg, #0b486b 0%, #f56217 100%)",
                badge: "Compact",
                shippingDays: 2,
                createdAt: now.AddDays(-80),
                images:
                [
                    ("https://unsplash.com/photos/B_KcElhSprY/download?force=true&w=1200", true),
                    ("https://unsplash.com/photos/pM4fy8F_FHo/download?force=true&w=1200", false)
                ],
                tags: ["portable", "bluetooth", "stereo pair"],
                specs:
                [
                    ("Battery", "16 hours"),
                    ("Water resistance", "IPX5"),
                    ("Pairing", "Stereo wireless")
                ],
                reviews:
                [
                    CreateReview("amali-001", 4, "Looks tiny, sounds bigger", "Great for desk listening and podcasts.", now.AddDays(-72))
                ])
        ]);

        dbContext.Coupons.AddRange(
        [
            new CouponEntity
            {
                Id = 1,
                Code = "WELCOME10",
                Description = "10 percent off your first order",
                DiscountType = "Percent",
                DiscountValue = 10m,
                MinimumSpend = 100m,
                ExpiryDate = now.AddMonths(4),
                UsageLimit = 500,
                UsedCount = 37
            },
            new CouponEntity
            {
                Id = 2,
                Code = "FREESHIP",
                Description = "Flat shipping discount for compact orders",
                DiscountType = "Fixed",
                DiscountValue = 18m,
                MinimumSpend = 80m,
                ExpiryDate = now.AddMonths(2),
                UsageLimit = 250,
                UsedCount = 91
            }
        ]);

        dbContext.WishlistItems.AddRange(
        [
            new WishlistItemEntity
            {
                UserId = "customer-001",
                ProductId = 102,
                AddedAt = now.AddDays(-8)
            },
            new WishlistItemEntity
            {
                UserId = "customer-001",
                ProductId = 107,
                AddedAt = now.AddDays(-3)
            }
        ]);

        dbContext.Orders.Add(new OrderEntity
        {
            Id = 1001,
            OrderNumber = "EB-1001",
            UserId = "customer-001",
            CouponId = 1,
            CustomerName = "Ayesha Fernando",
            CustomerEmail = "ayesha@easybuy.demo",
            SubtotalAmount = 228m,
            TotalAmount = 205.20m,
            DiscountAmount = 22.80m,
            Status = "Delivered",
            PaymentMethod = "Card",
            TrackingNumber = "TRK-1001",
            CourierName = "EasyBuy Express",
            OrderDate = now.AddDays(-12),
            EstimatedDeliveryDate = now.AddDays(-8),
            ShippingDate = now.AddDays(-10),
            ShippingAddress = "14 Lake View Avenue, Colombo 05, 00500",
            ShippingCost = 0m,
            Notes = "Leave at reception if unavailable.",
            Payment = new PaymentEntity
            {
                GatewayPaymentId = "PAY-1001",
                Amount = 205.20m,
                Status = "Success",
                PaymentDate = now.AddDays(-12).AddMinutes(7)
            },
            Items =
            [
                new OrderItemEntity
                {
                    ProductId = 103,
                    ProductName = "Pulse 4K Webcam",
                    Quantity = 1,
                    UnitPrice = 99m
                },
                new OrderItemEntity
                {
                    ProductId = 102,
                    ProductName = "Atlas Mechanical Keyboard",
                    Quantity = 1,
                    UnitPrice = 129m
                }
            ],
            TrackingEvents =
            [
                new OrderTrackingEventEntity
                {
                    EventCode = "order_placed",
                    Title = "Order placed",
                    Description = "We received your order and created the shipment.",
                    Location = "EasyBuy online store",
                    OccurredAt = now.AddDays(-12),
                    IsCompleted = true
                },
                new OrderTrackingEventEntity
                {
                    EventCode = "payment_confirmed",
                    Title = "Payment confirmed",
                    Description = "Card payment was approved and captured.",
                    Location = "Payment gateway",
                    OccurredAt = now.AddDays(-12).AddMinutes(7),
                    IsCompleted = true
                },
                new OrderTrackingEventEntity
                {
                    EventCode = "packed",
                    Title = "Packed for dispatch",
                    Description = "Your items were packed and sealed at the fulfilment hub.",
                    Location = "Fulfilment hub - Colombo",
                    OccurredAt = now.AddDays(-11).AddHours(3),
                    IsCompleted = true
                },
                new OrderTrackingEventEntity
                {
                    EventCode = "shipped",
                    Title = "Shipped",
                    Description = "The package left the hub and is moving through the courier network.",
                    Location = "EasyBuy Express linehaul",
                    OccurredAt = now.AddDays(-10).AddHours(8),
                    IsCompleted = true
                },
                new OrderTrackingEventEntity
                {
                    EventCode = "out_for_delivery",
                    Title = "Out for delivery",
                    Description = "The courier is making the final delivery attempt today.",
                    Location = "Colombo 05 delivery zone",
                    OccurredAt = now.AddDays(-8).AddHours(8),
                    IsCompleted = true
                },
                new OrderTrackingEventEntity
                {
                    EventCode = "delivered",
                    Title = "Delivered",
                    Description = "Package delivered successfully to the provided address.",
                    Location = "14 Lake View Avenue, Colombo 05",
                    OccurredAt = now.AddDays(-8).AddHours(13),
                    IsCompleted = true
                }
            ]
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static UserEntity CreateReviewer(string id, string name, string email, DateTime createdAt)
    {
        return new UserEntity
        {
            Id = id,
            Email = email,
            Name = name,
            Role = "Customer",
            CreatedAt = createdAt
        };
    }

    private static ReviewEntity CreateReview(string userId, int rating, string headline, string comment, DateTime createdAt)
    {
        return new ReviewEntity
        {
            UserId = userId,
            Rating = rating,
            Headline = headline,
            Comment = comment,
            CreatedAt = createdAt,
            VerifiedPurchase = true
        };
    }

    private static ProductEntity CreateProduct(
        int id,
        string name,
        string slug,
        string summary,
        string description,
        int categoryId,
        decimal price,
        decimal compareAtPrice,
        int stock,
        double rating,
        int reviewCount,
        int warrantyMonths,
        bool isFeatured,
        bool isNewArrival,
        string accent,
        string badge,
        int shippingDays,
        DateTime createdAt,
        IReadOnlyList<(string ImageUrl, bool IsMain)> images,
        IReadOnlyList<string> tags,
        IReadOnlyList<(string Label, string Value)> specs,
        IReadOnlyList<ReviewEntity> reviews)
    {
        return new ProductEntity
        {
            Id = id,
            Name = name,
            Slug = slug,
            Summary = summary,
            Description = description,
            CategoryId = categoryId,
            Price = price,
            CompareAtPrice = compareAtPrice,
            Stock = stock,
            Rating = rating,
            ReviewCount = reviewCount,
            WarrantyMonths = warrantyMonths,
            IsFeatured = isFeatured,
            IsNewArrival = isNewArrival,
            Accent = accent,
            Badge = badge,
            ShippingDays = shippingDays,
            CreatedAt = createdAt,
            Images = images.Select((item, index) => new ProductImageEntity
            {
                ImageUrl = item.ImageUrl,
                IsMain = item.IsMain,
                SortOrder = index + 1
            }).ToList(),
            Tags = tags.Select((item, index) => new ProductTagEntity
            {
                Value = item,
                SortOrder = index + 1
            }).ToList(),
            Specs = specs.Select((item, index) => new ProductSpecEntity
            {
                Label = item.Label,
                Value = item.Value,
                SortOrder = index + 1
            }).ToList(),
            Reviews = reviews.ToList()
        };
    }
}
