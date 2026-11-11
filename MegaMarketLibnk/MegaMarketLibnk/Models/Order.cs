using System;
namespace MegaMarketLibnk.Models;
public partial class Order
{
    public int Id { get; set; }
    public int Customerid { get; set; }
    public DateTime Orderdate { get; set; }
    public int? Deliveryaddressid { get; set; }
    public decimal Totalprice { get; set; }
    public decimal Deliverycost { get; set; }
    public int Statusid { get; set; }
    public virtual Customerprofile Customer { get; set; } = null!;
    public virtual Address? Deliveryaddress { get; set; }
    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();
    public virtual ICollection<Orderstatus> Orderstatuses { get; set; } = new List<Orderstatus>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
