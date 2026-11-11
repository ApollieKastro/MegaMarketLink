using System;
namespace MegaMarketLibnk.Models;
public partial class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string Passwordhash { get; set; } = null!;
    public string Salt { get; set; } = null!;
    public int Roleid { get; set; }
    public bool Isbanned { get; set; }
    public DateTime Createdat { get; set; }
    public virtual Customerprofile? Customerprofile { get; set; }
    public virtual Role Role { get; set; } = null!;
    public virtual Sellerprofile? Sellerprofile { get; set; }
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
