using System;
namespace MegaMarketLibnk.Models;
public partial class Review
{
    public int Id { get; set; }
    public int Productid { get; set; }
    public int Customerid { get; set; }
    public short Rating { get; set; }
    public string? Textreview { get; set; }
    public string? Imageurls { get; set; }
    public DateTime Createdat { get; set; }
    public virtual Customerprofile Customer { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
