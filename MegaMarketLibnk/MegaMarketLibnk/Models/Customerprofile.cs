using System;
namespace MegaMarketLibnk.Models;
public partial class Customerprofile
{
    public int Id { get; set; }
    public int Userid { get; set; }
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string? Middlename { get; set; }
    public int? Mainaddressid { get; set; }
    public int Bonuspointsbalance { get; set; }
    public virtual Address? Mainaddress { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual User User { get; set; } = null!;
}
