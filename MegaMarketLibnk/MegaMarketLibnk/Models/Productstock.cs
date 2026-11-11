using System;
namespace MegaMarketLibnk.Models;
public partial class Productstock
{
    public int Id { get; set; }
    public int Productid { get; set; }
    public int Warehouseid { get; set; }
    public int Quantityinstock { get; set; }
    public int Reservedquantity { get; set; }
    public virtual Product Product { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
}
