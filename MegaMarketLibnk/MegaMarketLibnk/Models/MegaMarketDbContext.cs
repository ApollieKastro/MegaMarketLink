using System;
using Microsoft.EntityFrameworkCore;
namespace MegaMarketLibnk.Models;
public partial class MegaMarketDbContext : DbContext
{
    public MegaMarketDbContext()
    {
    }
    public MegaMarketDbContext(DbContextOptions<MegaMarketDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Customerprofile> Customerprofiles { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Orderitem> Orderitems { get; set; }
    public virtual DbSet<Orderstatus> Orderstatuses { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Productcharacteristic> Productcharacteristics { get; set; }
    public virtual DbSet<Productstock> Productstocks { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Sellerprofile> Sellerprofiles { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Warehouse> Warehouses { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;UserName=postgres;Password=root;DataBase=MegaMarketDB");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum("transaction_type", new[] { "Payment", "Refund", "Payout" });
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("addresses_pkey");
            entity.ToTable("addresses");
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Apartment)
                .HasMaxLength(20)
                .HasColumnName("apartment");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.House)
                .HasMaxLength(20)
                .HasColumnName("house");
            entity.Property(e => e.Postcode)
                .HasMaxLength(10)
                .HasColumnName("postcode");
            entity.Property(e => e.Region)
                .HasMaxLength(100)
                .HasColumnName("region");
            entity.Property(e => e.Street)
                .HasMaxLength(100)
                .HasColumnName("street");
        });
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");
            entity.ToTable("categories");
            entity.HasIndex(e => e.Parentcategoryid, "ix_categories_parent");
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Parentcategoryid).HasColumnName("parentcategoryid");
            entity.HasOne(d => d.Parentcategory).WithMany(p => p.InverseParentcategory)
                .HasForeignKey(d => d.Parentcategoryid)
                .HasConstraintName("categories_parentcategoryid_fkey");
        });
        modelBuilder.Entity<Customerprofile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customerprofiles_pkey");
            entity.ToTable("customerprofiles");
            entity.HasIndex(e => e.Userid, "customerprofiles_userid_key").IsUnique();
            entity.HasIndex(e => e.Mainaddressid, "ix_customerprofiles_addr");
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Bonuspointsbalance)
                .HasDefaultValue(0)
                .HasColumnName("bonuspointsbalance");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .HasColumnName("lastname");
            entity.Property(e => e.Mainaddressid).HasColumnName("mainaddressid");
            entity.Property(e => e.Middlename)
                .HasMaxLength(100)
                .HasColumnName("middlename");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.HasOne(d => d.Mainaddress).WithMany(p => p.Customerprofiles)
                .HasForeignKey(d => d.Mainaddressid)
                .HasConstraintName("customerprofiles_mainaddressid_fkey");
            entity.HasOne(d => d.User).WithOne(p => p.Customerprofile)
                .HasForeignKey<Customerprofile>(d => d.Userid)
                .HasConstraintName("customerprofiles_userid_fkey");
        });
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");
            entity.ToTable("orders");
            entity.HasIndex(e => e.Deliveryaddressid, "ix_orders_address");
            entity.HasIndex(e => e.Customerid, "ix_orders_customer");
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Customerid).HasColumnName("customerid");
            entity.Property(e => e.Deliveryaddressid).HasColumnName("deliveryaddressid");
            entity.Property(e => e.Deliverycost)
                .HasPrecision(12, 2)
                .HasColumnName("deliverycost");
            entity.Property(e => e.Orderdate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("orderdate");
            entity.Property(e => e.Statusid)
                .HasDefaultValue(0)
                .HasColumnName("statusid");
            entity.Property(e => e.Totalprice)
                .HasPrecision(12, 2)
                .HasColumnName("totalprice");
            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Customerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_customerid_fkey");
            entity.HasOne(d => d.Deliveryaddress).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Deliveryaddressid)
                .HasConstraintName("orders_deliveryaddressid_fkey");
        });
        modelBuilder.Entity<Orderitem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orderitems_pkey");
            entity.ToTable("orderitems");
            entity.HasIndex(e => e.Orderid, "ix_orderitems_order");
            entity.HasIndex(e => e.Productid, "ix_orderitems_product");
            entity.HasIndex(e => e.Sellerid, "ix_orderitems_seller");
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Priceatpurchase)
                .HasPrecision(12, 2)
                .HasColumnName("priceatpurchase");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Sellerid).HasColumnName("sellerid");
            entity.HasOne(d => d.Order).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("orderitems_orderid_fkey");
            entity.HasOne(d => d.Product).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.Productid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orderitems_productid_fkey");
            entity.HasOne(d => d.Seller).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.Sellerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orderitems_sellerid_fkey");
        });
        modelBuilder.Entity<Orderstatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orderstatuses_pkey");
            entity.ToTable("orderstatuses");
            entity.HasIndex(e => e.Orderid, "ix_orderstatuses_order");
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Changedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("changedat");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.HasOne(d => d.Order).WithMany(p => p.Orderstatuses)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("orderstatuses_orderid_fkey");
        });
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");
            entity.ToTable("products");
            entity.HasIndex(e => e.Categoryid, "ix_products_category");
            entity.HasIndex(e => e.Sellerid, "ix_products_seller");
            entity.HasIndex(e => e.Sku, "products_sku_key").IsUnique();
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Barcode)
                .HasMaxLength(100)
                .HasColumnName("barcode");
            entity.Property(e => e.Baseprice)
                .HasPrecision(12, 2)
                .HasColumnName("baseprice");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Currentdiscount)
                .HasPrecision(5, 2)
                .HasColumnName("currentdiscount");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Sellerid).HasColumnName("sellerid");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("sku");
            entity.Property(e => e.Statusid)
                .HasDefaultValue(0)
                .HasColumnName("statusid");
            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_categoryid_fkey");
            entity.HasOne(d => d.Seller).WithMany(p => p.Products)
                .HasForeignKey(d => d.Sellerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_sellerid_fkey");
        });
        modelBuilder.Entity<Productcharacteristic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productcharacteristics_pkey");
            entity.ToTable("productcharacteristics");
            entity.HasIndex(e => e.Productid, "ix_productchars_product");
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Keyname)
                .HasMaxLength(100)
                .HasColumnName("keyname");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.HasOne(d => d.Product).WithMany(p => p.Productcharacteristics)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("productcharacteristics_productid_fkey");
        });
        modelBuilder.Entity<Productstock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productstocks_pkey");
            entity.ToTable("productstocks");
            entity.HasIndex(e => e.Productid, "ix_productstocks_product");
            entity.HasIndex(e => e.Warehouseid, "ix_productstocks_wh");
            entity.HasIndex(e => new { e.Productid, e.Warehouseid }, "uq_product_warehouse").IsUnique();
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Quantityinstock)
                .HasDefaultValue(0)
                .HasColumnName("quantityinstock");
            entity.Property(e => e.Reservedquantity)
                .HasDefaultValue(0)
                .HasColumnName("reservedquantity");
            entity.Property(e => e.Warehouseid).HasColumnName("warehouseid");
            entity.HasOne(d => d.Product).WithMany(p => p.Productstocks)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("productstocks_productid_fkey");
            entity.HasOne(d => d.Warehouse).WithMany(p => p.Productstocks)
                .HasForeignKey(d => d.Warehouseid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("productstocks_warehouseid_fkey");
        });
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reviews_pkey");
            entity.ToTable("reviews");
            entity.HasIndex(e => e.Customerid, "ix_reviews_customer");
            entity.HasIndex(e => e.Productid, "ix_reviews_product");
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Customerid).HasColumnName("customerid");
            entity.Property(e => e.Imageurls)
                .HasColumnType("jsonb")
                .HasColumnName("imageurls");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Textreview).HasColumnName("textreview");
            entity.HasOne(d => d.Customer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.Customerid)
                .HasConstraintName("reviews_customerid_fkey");
            entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("reviews_productid_fkey");
        });
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");
            entity.ToTable("roles");
            entity.HasIndex(e => e.Rolename, "roles_rolename_key").IsUnique();
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Permissionsmask)
                .HasDefaultValue(0L)
                .HasColumnName("permissionsmask");
            entity.Property(e => e.Rolename)
                .HasMaxLength(50)
                .HasColumnName("rolename");
        });
        modelBuilder.Entity<Sellerprofile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sellerprofiles_pkey");
            entity.ToTable("sellerprofiles");
            entity.HasIndex(e => e.Legaladdressid, "ix_sellerprofiles_addr");
            entity.HasIndex(e => e.Userid, "sellerprofiles_userid_key").IsUnique();
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasPrecision(15, 2)
                .HasColumnName("balance");
            entity.Property(e => e.Companyname)
                .HasMaxLength(255)
                .HasColumnName("companyname");
            entity.Property(e => e.Inn)
                .HasMaxLength(12)
                .HasColumnName("inn");
            entity.Property(e => e.Kpp)
                .HasMaxLength(9)
                .HasColumnName("kpp");
            entity.Property(e => e.Legaladdressid).HasColumnName("legaladdressid");
            entity.Property(e => e.Rating)
                .HasPrecision(3, 2)
                .HasColumnName("rating");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.HasOne(d => d.Legaladdress).WithMany(p => p.Sellerprofiles)
                .HasForeignKey(d => d.Legaladdressid)
                .HasConstraintName("sellerprofiles_legaladdressid_fkey");
            entity.HasOne(d => d.User).WithOne(p => p.Sellerprofile)
                .HasForeignKey<Sellerprofile>(d => d.Userid)
                .HasConstraintName("sellerprofiles_userid_fkey");
        });
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transactions_pkey");
            entity.ToTable("transactions");
            entity.HasIndex(e => e.Orderid, "ix_transactions_order");
            entity.HasIndex(e => e.Userid, "ix_transactions_user");
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(15, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Transactiondate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("transactiondate");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.HasOne(d => d.Order).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_orderid_fkey");
            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_userid_fkey");
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");
            entity.ToTable("users");
            entity.HasIndex(e => e.Roleid, "ix_users_roleid");
            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();
            entity.HasIndex(e => e.Phone, "users_phone_key").IsUnique();
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Isbanned)
                .HasDefaultValue(false)
                .HasColumnName("isbanned");
            entity.Property(e => e.Passwordhash).HasColumnName("passwordhash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Salt).HasColumnName("salt");
            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_roleid_fkey");
        });
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("warehouses_pkey");
            entity.ToTable("warehouses");
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Warehousename)
                .HasMaxLength(255)
                .HasColumnName("warehousename");
        });
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
