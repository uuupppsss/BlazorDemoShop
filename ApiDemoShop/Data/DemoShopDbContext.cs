using ApiDemoShop.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ApiDemoShop.Data
{

    public partial class DemoShopDbContext : DbContext
    {
        public DemoShopDbContext()
        {
        }

        public DemoShopDbContext(DbContextOptions<DemoShopDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BasketItem> BasketItems { get; set; }

        public virtual DbSet<Order> Orders { get; set; }

        public virtual DbSet<OrderItem> OrderItems { get; set; }

        public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<ProductImage> ProductImages { get; set; }

        public virtual DbSet<ProductTag> ProductTags { get; set; }

        public virtual DbSet<ProductType> ProductTypes { get; set; }

        public virtual DbSet<SavedProduct> SavedProducts { get; set; }

        public virtual DbSet<Tag> Tags { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<UserRole> UserRoles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                optionsBuilder.UseSqlServer("Server=192.168.200.35;Database=user26;user=user26;password=50371;TrustServerCertificate=true;MultipleActiveResultSets=true");

                //optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DemoShopDb;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Cyrillic_General_CI_AS");

            modelBuilder.Entity<BasketItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("BasketItem");

                entity.HasIndex(e => e.ProductId, "IX_BasketItem_ProductId");

                entity.HasIndex(e => e.UserId, "IX_BasketItem_UserId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Count)
                    .HasColumnName("count");

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.BasketItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BasketItem_Product");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.BasketItems)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BasketItem_User");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("Order");

                entity.HasIndex(e => e.StatusId, "IX_Order_StatusId");

                entity.HasIndex(e => e.UserId, "IX_Order_UserId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime2");

                entity.Property(e => e.FullCost)
                    .HasColumnType("decimal(19,2)");

                entity.Property(e => e.RecieveDate)
                    .HasColumnType("datetime2");

                entity.Property(e => e.StatusId)
                    .HasColumnName("status_id");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_OrderStatus");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_User");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("OrderItem");

                entity.HasIndex(e => e.OrdeId, "IX_OrderItem_OrderId");

                entity.HasIndex(e => e.ProductId, "IX_OrderItem_ProductId");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Count);
                entity.Property(e => e.OrdeId);
                entity.Property(e => e.ProductId);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrdeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItem_Order");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItem_Product");
            });

            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("OrderStatus");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("Product");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Count);
                entity.Property(e => e.Description)
                    .HasMaxLength(255);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasDefaultValue(" ");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(19,2)");

                entity.Property(e => e.TimeBought);
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("ProductImage");

                entity.HasIndex(e => e.ProductId, "IX_ProductImage_ProductId");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Image)
                    .HasDefaultValue("");

                entity.Property(e => e.ProductId);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductImage_Product");
            });

            modelBuilder.Entity<ProductTag>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("ProductTag");

                entity.HasIndex(e => e.ProductId, "IX_ProductTag_ProductId");

                entity.HasIndex(e => e.TagId, "IX_ProductTag_TagId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ProductId);
                entity.Property(e => e.TagId);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductTags)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductTag_Product");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.ProductTags)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductTag_Tag");
            });

            modelBuilder.Entity<ProductType>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("ProductType");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasDefaultValue(" ");
            });

            modelBuilder.Entity<SavedProduct>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("SavedProduct");

                entity.HasIndex(e => e.ProductId, "IX_SavedProduct_ProductId");

                entity.HasIndex(e => e.UserId, "IX_SavedProduct_UserId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.SavedProducts)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_SavedProduct_Product");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SavedProducts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_SavedProduct_User");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("Tag");

                entity.HasIndex(e => e.TypeId, "IX_Tag_TypeId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasDefaultValue("");

                entity.Property(e => e.TypeId);

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Tags)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tag_ProductType");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("User");

                entity.HasIndex(e => e.RoleId, "IX_User_RoleId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ContactPhone)
                    .HasMaxLength(255)
                    .HasDefaultValue("");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasDefaultValue("");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .HasDefaultValue(" ");

                entity.Property(e => e.RoleId)
                    .HasColumnName("role_Id");

                entity.Property(e => e.Username)
                    .HasMaxLength(255)
                    .HasDefaultValue(" ");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_UserRole");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("UserRole");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasDefaultValue(" ");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
