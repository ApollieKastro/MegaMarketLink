using System;
namespace MegaMarketLibnk.Models;
public partial class Transaction
{
    public int Id { get; set; }
    public int Orderid { get; set; }
    public int Userid { get; set; }
    public decimal Amount { get; set; }
    public DateTime Transactiondate { get; set; }
    public virtual Order Order { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
