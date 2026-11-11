using System;
namespace MegaMarketLibnk.Models;
public partial class Orderitem
{
    public int Id { get; set; }
    public int Orderid { get; set; }
    public int Productid { get; set; }
    public int Quantity { get; set; }
    public decimal Priceatpurchase { get; set; }
    public int Sellerid { get; set; }
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual Sellerprofile Seller { get; set; } = null!;
}
