using ServerApp.Models;

namespace ServerApp.Services;

/// <summary>
/// Provides seed data for the InventoryHub application
/// </summary>
public static class SeedingService
{
    /// <summary>
    /// Gets sample product data with nested category information
    /// </summary>
    /// <returns>Array of sample products with categories</returns>
    public static Product[] GetSampleProducts()
    {
        return new Product[]
        {
            // Original Products
            new() { Id = 1, Name = "Laptop", Description = "High-performance laptop with 16GB RAM, 512GB SSD, and Intel Core i7 processor", Price = 1200.50M, Stock = 25, CategoryId = 101, Category = new Category { Id = 101, Name = "Electronics" } },
            new() { Id = 2, Name = "Headphones", Description = "Wireless over-ear headphones with active noise cancellation and 30-hour battery life", Price = 50.00M, Stock = 100, CategoryId = 102, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 3, Name = "Mouse", Description = "Ergonomic wireless mouse with adjustable DPI and rechargeable battery", Price = 25.99M, Stock = 150, CategoryId = 102, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 4, Name = "Keyboard", Description = "Mechanical keyboard with RGB backlighting and programmable keys", Price = 75.50M, Stock = 80, CategoryId = 102, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 5, Name = "Monitor", Description = "27-inch 4K UHD monitor with HDR support and 144Hz refresh rate", Price = 299.99M, Stock = 45, CategoryId = 101, Category = new Category { Id = 101, Name = "Electronics" } },
            new() { Id = 6, Name = "Webcam", Description = "1080p Full HD webcam with built-in microphone and auto-focus technology", Price = 89.99M, Stock = 75, CategoryId = 101, Category = new Category { Id = 101, Name = "Electronics" } },

            // Additional Electronics (Category 101)
            new() { Id = 7, Name = "Smartphone", Description = "Latest flagship smartphone with 5G connectivity, triple camera system, and 128GB storage", Price = 899.99M, Stock = 50, CategoryId = 101, Category = new Category { Id = 101, Name = "Electronics" } },
            new() { Id = 8, Name = "Tablet", Description = "10.5-inch tablet with stylus support, 256GB storage, and all-day battery life", Price = 599.99M, Stock = 35, CategoryId = 101, Category = new Category { Id = 101, Name = "Electronics" } },
            new() { Id = 9, Name = "Smart Watch", Description = "Fitness tracking smart watch with heart rate monitor, GPS, and water resistance", Price = 299.99M, Stock = 60, CategoryId = 101, Category = new Category { Id = 101, Name = "Electronics" } },
            new() { Id = 10, Name = "Wireless Speaker", Description = "Portable Bluetooth speaker with 360-degree sound and 12-hour battery life", Price = 129.99M, Stock = 85, CategoryId = 101, Category = new Category { Id = 101, Name = "Electronics" } },

            // Additional Accessories (Category 102)
            new() { Id = 11, Name = "Mouse Pad", Description = "Extra-large gaming mouse pad with non-slip rubber base and smooth surface", Price = 15.99M, Stock = 200, CategoryId = 102, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 12, Name = "USB Hub", Description = "7-port USB 3.0 hub with individual power switches and LED indicators", Price = 29.99M, Stock = 120, CategoryId = 102, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 13, Name = "Laptop Stand", Description = "Adjustable aluminum laptop stand with ergonomic design and cooling ventilation", Price = 45.99M, Stock = 90, CategoryId = 102, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 14, Name = "Cable Organizer", Description = "Cable management system with clips and ties to keep your workspace tidy", Price = 19.99M, Stock = 150, CategoryId = 102, Category = new Category { Id = 102, Name = "Accessories" } },

            // Gaming (Category 103)
            new() { Id = 15, Name = "Gaming Console", Description = "Next-generation gaming console with 4K gaming, ray tracing, and 1TB SSD", Price = 499.99M, Stock = 30, CategoryId = 103, Category = new Category { Id = 103, Name = "Gaming" } },
            new() { Id = 16, Name = "Gaming Controller", Description = "Wireless gaming controller with haptic feedback, adaptive triggers, and 15-hour battery", Price = 69.99M, Stock = 100, CategoryId = 103, Category = new Category { Id = 103, Name = "Gaming" } },
            new() { Id = 17, Name = "Gaming Headset", Description = "7.1 surround sound gaming headset with noise-canceling microphone", Price = 129.99M, Stock = 75, CategoryId = 103, Category = new Category { Id = 103, Name = "Gaming" } },
            new() { Id = 18, Name = "Gaming Chair", Description = "Ergonomic gaming chair with lumbar support, adjustable armrests, and reclining feature", Price = 299.99M, Stock = 40, CategoryId = 103, Category = new Category { Id = 103, Name = "Gaming" } },
            new() { Id = 19, Name = "Gaming Mouse", Description = "Professional gaming mouse with 16000 DPI sensor and customizable RGB lighting", Price = 89.99M, Stock = 95, CategoryId = 103, Category = new Category { Id = 103, Name = "Gaming" } },

            // Networking (Category 104)
            new() { Id = 20, Name = "Wireless Router", Description = "Dual-band Wi-Fi 6 router with gigabit ethernet ports and advanced security features", Price = 159.99M, Stock = 55, CategoryId = 104, Category = new Category { Id = 104, Name = "Networking" } },
            new() { Id = 21, Name = "Network Switch", Description = "8-port gigabit network switch with plug-and-play installation", Price = 89.99M, Stock = 45, CategoryId = 104, Category = new Category { Id = 104, Name = "Networking" } },
            new() { Id = 22, Name = "Wi-Fi Extender", Description = "Dual-band Wi-Fi range extender covering up to 1500 sq ft", Price = 49.99M, Stock = 70, CategoryId = 104, Category = new Category { Id = 104, Name = "Networking" } },
            new() { Id = 23, Name = "Network Cable", Description = "Cat6 ethernet cable, 25 feet, with gold-plated connectors for reliable connectivity", Price = 12.99M, Stock = 200, CategoryId = 104, Category = new Category { Id = 104, Name = "Networking" } },

            // Storage (Category 105)
            new() { Id = 24, Name = "External SSD", Description = "1TB portable external SSD with USB-C connection and up to 1050MB/s read speed", Price = 179.99M, Stock = 65, CategoryId = 105, Category = new Category { Id = 105, Name = "Storage" } },
            new() { Id = 25, Name = "USB Flash Drive", Description = "64GB USB 3.0 flash drive with keychain design and retractable connector", Price = 29.99M, Stock = 180, CategoryId = 105, Category = new Category { Id = 105, Name = "Storage" } },
            new() { Id = 26, Name = "Memory Card", Description = "128GB microSD card with adapter, UHS-I speed class for 4K video recording", Price = 39.99M, Stock = 120, CategoryId = 105, Category = new Category { Id = 105, Name = "Storage" } },
            new() { Id = 27, Name = "Hard Drive Enclosure", Description = "USB 3.0 external hard drive enclosure for 2.5-inch SATA drives with tool-free installation", Price = 25.99M, Stock = 90, CategoryId = 105, Category = new Category { Id = 105, Name = "Storage" } },

            // Software (Category 106)
            new() { Id = 28, Name = "Antivirus Software", Description = "Comprehensive antivirus protection for up to 5 devices with real-time threat detection", Price = 49.99M, Stock = 100, CategoryId = 106, Category = new Category { Id = 106, Name = "Software" } },
            new() { Id = 29, Name = "Office Suite", Description = "Complete productivity suite including word processor, spreadsheet, and presentation software", Price = 199.99M, Stock = 85, CategoryId = 106, Category = new Category { Id = 106, Name = "Software" } },
            new() { Id = 30, Name = "Design Software", Description = "Professional graphic design and photo editing software with advanced creative tools", Price = 599.99M, Stock = 40, CategoryId = 106, Category = new Category { Id = 106, Name = "Software" } },
            new() { Id = 31, Name = "Development IDE", Description = "Integrated development environment with intelligent code completion and debugging tools", Price = 299.99M, Stock = 55, CategoryId = 106, Category = new Category { Id = 106, Name = "Software" } },

            // Photography (Category 107)
            new() { Id = 32, Name = "Digital Camera", Description = "24MP mirrorless camera with 4K video recording and interchangeable lens system", Price = 799.99M, Stock = 30, CategoryId = 107, Category = new Category { Id = 107, Name = "Photography" } },
            new() { Id = 33, Name = "Camera Lens", Description = "50mm f/1.8 prime lens with fast autofocus and beautiful bokeh effect", Price = 599.99M, Stock = 25, CategoryId = 107, Category = new Category { Id = 107, Name = "Photography" } },
            new() { Id = 34, Name = "Camera Tripod", Description = "Professional aluminum tripod with ball head and quick-release plate, supports up to 15 lbs", Price = 79.99M, Stock = 60, CategoryId = 107, Category = new Category { Id = 107, Name = "Photography" } },
            new() { Id = 35, Name = "Camera Bag", Description = "Padded camera backpack with customizable dividers and weather-resistant exterior", Price = 89.99M, Stock = 70, CategoryId = 107, Category = new Category { Id = 107, Name = "Photography" } },
            new() { Id = 36, Name = "Memory Card Reader", Description = "USB 3.0 multi-card reader supporting SD, microSD, CF, and MS cards", Price = 29.99M, Stock = 85, CategoryId = 107, Category = new Category { Id = 107, Name = "Photography" } }
        };
    }

    /// <summary>
    /// Gets all available categories from the sample data
    /// </summary>
    /// <returns>Array of unique categories</returns>
    public static Category[] GetCategories()
    {
        return GetSampleProducts()
            .Select(p => p.Category)
            .DistinctBy(c => c.Id)
            .ToArray();
    }
}