using System;
namespace MegaMarketLibnk.Models;
public partial class Address
{
    public int Id { get; set; }
    public string? Postcode { get; set; }
    public string? Region { get; set; }
    public string City { get; set; } = null!;
    public string? Street { get; set; }
    public string? House { get; set; }
    public string? Apartment { get; set; }
    public virtual ICollection<Customerprofile> Customerprofiles { get; set; } = new List<Customerprofile>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Sellerprofile> Sellerprofiles { get; set; } = new List<Sellerprofile>();
}
