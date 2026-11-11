using System;
namespace MegaMarketLibnk.Models;
public partial class Sellerprofile
{
    public int Id { get; set; }
    public int Userid { get; set; }
    public string Companyname { get; set; } = null!;
    public string Inn { get; set; } = null!;
    public string? Kpp { get; set; }
    public int? Legaladdressid { get; set; }
    public decimal Balance { get; set; }
    public decimal Rating { get; set; }
    public virtual Address? Legaladdress { get; set; }
    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual User User { get; set; } = null!;
}
