using System;
namespace MegaMarketLibnk.Models;
public partial class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = null!;
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Categoryid { get; set; }
    public int Sellerid { get; set; }
    public decimal Baseprice { get; set; }
    public decimal Currentdiscount { get; set; }
    public int Statusid { get; set; }
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();
    public virtual ICollection<Productcharacteristic> Productcharacteristics { get; set; } = new List<Productcharacteristic>();
    public virtual ICollection<Productstock> Productstocks { get; set; } = new List<Productstock>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual Sellerprofile Seller { get; set; } = null!;
}
