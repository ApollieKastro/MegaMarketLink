using System;
namespace MegaMarketLibnk.Models;
public partial class Warehouse
{
    public int Id { get; set; }
    public string Warehousename { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int Capacity { get; set; }
    public virtual ICollection<Productstock> Productstocks { get; set; } = new List<Productstock>();
}
