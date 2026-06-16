using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Models;

namespace МаршрутСборки.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Models.Assembly> Assemblies { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<AssemblyComponent> AssemblyComponents { get; set; }
        public DbSet<WarehouseOperation> WarehouseOperations { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<WarrantyCase> WarrantyCases { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<AssemblyReworkItem> AssemblyReworkItems { get; set; }
        public DbSet<WarrantyCaseNote> WarrantyCaseNotes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=assembly_tracker;Username=postgres;Password=12344321");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Assembly — два внешних ключа на User
            modelBuilder.Entity<Models.Assembly>()
                .HasOne(a => a.Dispatcher)
                .WithMany(u => u.CreatedAssemblies)
                .HasForeignKey(a => a.DispatcherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.Assembly>()
                .HasOne(a => a.Assembler)
                .WithMany(u => u.AssignedAssemblies)
                .HasForeignKey(a => a.AssemblerId)
                .OnDelete(DeleteBehavior.Restrict);

            //AssemblyComponent — составной уникальный индекс
            modelBuilder.Entity<AssemblyComponent>()
                .HasIndex(ac => new { ac.AssemblyId, ac.ComponentId })
                .IsUnique();

            // AssemblyReworkItem — два FK на Component
            modelBuilder.Entity<AssemblyReworkItem>()
                .HasOne(r => r.Assembly)
                .WithMany()
                .HasForeignKey(r => r.AssemblyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssemblyReworkItem>()
                .HasOne(r => r.OldComponent)
                .WithMany()
                .HasForeignKey(r => r.OldComponentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AssemblyReworkItem>()
                .HasOne(r => r.NewComponent)
                .WithMany()
                .HasForeignKey(r => r.NewComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WarrantyCaseNote>()
                .HasOne(n => n.Case)
                .WithMany()
                .HasForeignKey(n => n.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WarrantyCaseNote>()
                .HasOne(n => n.Author)
                .WithMany()
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            //Точность для decimal
            modelBuilder.Entity<Component>()
                .Property(c => c.Price)
                .HasPrecision(10, 2);

            //Seed — роли
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Технический директор" },
                new Role { RoleId = 2, RoleName = "Диспетчер" },
                new Role { RoleId = 3, RoleName = "Сборщик" },
                new Role { RoleId = 4, RoleName = "Кладовщик" },
                new Role { RoleId = 5, RoleName = "Тестировщик" },
                new Role { RoleId = 6, RoleName = "Инженер сервисного центра" }
            );

            //Seed — администратор по умолчанию
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    LastName = "Администратор",
                    FirstName = "Системный",
                    Login = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    RoleId = 1,
                    IsActive = true
                },
                new User
                {
                    UserId = 2,
                    LastName = "Иванов",
                    FirstName = "Иван",
                    Login = "ivanov",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    RoleId = 3, // Сборщик
                    IsActive = true
                },
                new User
                {
                    UserId = 3,
                    LastName = "Петров",
                    FirstName = "Пётр",
                    Login = "petrov",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    RoleId = 5, // Тестировщик
                    IsActive = true
                },
                new User
                {
                    UserId = 4,
                    LastName = "Сидоров",
                    FirstName = "Алексей",
                    Login = "sidorov",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    RoleId = 4, // Кладовщик
                    IsActive = true
                },
                new User
                {
                    UserId = 5,
                    LastName = "Фёдоров",
                    FirstName = "Михаил",
                    Login = "fedorov",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    RoleId = 2, // Диспетчер
                    IsActive = true
                },
                new User
                {
                    UserId = 6,
                    LastName = "Алексеев",
                    FirstName = "Сергей",
                    Login = "alekseev",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    RoleId = 6, // Гарантийный инженер
                    IsActive = true
                }
            );

            //Seed — тестовые комплектующие (реальные позиции из NITRINOnet)
            modelBuilder.Entity<Component>().HasData(
                new Component { ComponentId = 1, SKU = "CPU-I3-12100", Name = "Процессор Intel Core i3-12100", Category = "Процессор", Price = 12500, StockBalance = 15, MinStock = 3 },
                new Component { ComponentId = 2, SKU = "CPU-I5-12400", Name = "Процессор Intel Core i5-12400", Category = "Процессор", Price = 18900, StockBalance = 10, MinStock = 3 },
                new Component { ComponentId = 3, SKU = "CPU-I5-14400", Name = "Процессор Intel Core i5-14400", Category = "Процессор", Price = 22500, StockBalance = 8, MinStock = 2 },
                new Component { ComponentId = 4, SKU = "CPU-I7-13700", Name = "Процессор Intel Core i7-13700", Category = "Процессор", Price = 31000, StockBalance = 5, MinStock = 2 },
                new Component { ComponentId = 5, SKU = "RAM-8GB-DDR5", Name = "ОЗУ 8 GB DDR5", Category = "Оперативная память", Price = 4200, StockBalance = 30, MinStock = 5 },
                new Component { ComponentId = 6, SKU = "RAM-16GB-DDR5", Name = "ОЗУ 16 GB DDR5", Category = "Оперативная память", Price = 7800, StockBalance = 25, MinStock = 5 },
                new Component { ComponentId = 7, SKU = "RAM-32GB-DDR5", Name = "ОЗУ 32 GB DDR5", Category = "Оперативная память", Price = 14500, StockBalance = 12, MinStock = 3 },
                new Component { ComponentId = 8, SKU = "SSD-512GB", Name = "SSD накопитель 512 GB", Category = "Накопитель", Price = 5500, StockBalance = 40, MinStock = 10 },
                new Component { ComponentId = 9, SKU = "SSD-1TB", Name = "SSD накопитель 1 TB", Category = "Накопитель", Price = 9200, StockBalance = 20, MinStock = 5 },
                new Component { ComponentId = 10, SKU = "GPU-RTX3050", Name = "Видеокарта RTX 3050 8GB", Category = "Видеокарта", Price = 24000, StockBalance = 6, MinStock = 2 },
                new Component { ComponentId = 11, SKU = "GPU-RTX5060", Name = "Видеокарта RTX 5060 8GB", Category = "Видеокарта", Price = 38000, StockBalance = 4, MinStock = 2 },
                new Component { ComponentId = 12, SKU = "PSU-600W", Name = "Блок питания 600W APFC", Category = "Блок питания", Price = 6800, StockBalance = 18, MinStock = 4 },
                new Component { ComponentId = 13, SKU = "PSU-700W", Name = "Блок питания 700W APFC", Category = "Блок питания", Price = 8500, StockBalance = 10, MinStock = 3 },
                new Component { ComponentId = 14, SKU = "CASE-NITRINO", Name = "Корпус NITRINOnet", Category = "Корпус", Price = 4500, StockBalance = 22, MinStock = 5 },
                new Component { ComponentId = 15, SKU = "MB-Z690", Name = "Материнская плата ASUS Z690", Category = "Материнская плата", Price = 18200, StockBalance = 8, MinStock = 2 },
                new Component { ComponentId = 16, SKU = "CASE-S600", Name = "Корпус NITRINOnet S600 (системный блок)", Category = "Корпус", Price = 8900, StockBalance = 15, MinStock = 3 },
                new Component { ComponentId = 17, SKU = "CASE-S600M", Name = "Корпус NITRINOnet S600M (моноблок)", Category = "Корпус", Price = 14500, StockBalance = 8, MinStock = 2 },
                new Component { ComponentId = 18, SKU = "MON-22", Name = "Монитор 22\" Full HD", Category = "Монитор", Price = 18850, StockBalance = 10, MinStock = 2 },
                new Component { ComponentId = 19, SKU = "KB-NEWCLICK", Name = "Клавиатура НЬЮКЛИК (PREMIUM)", Category = "Периферия", Price = 2800, StockBalance = 20, MinStock = 5 },
                new Component { ComponentId = 20, SKU = "MS-NEWCLICK", Name = "Мышь НЬЮКЛИК (PREMIUM)", Category = "Периферия", Price = 1950, StockBalance = 0, MinStock = 5 }
            );
        }
    }
}