using System;
namespace MegaMarketLibnk.Models;
public partial class Category
{
    public int Id { get; set; }
    public int? Parentcategoryid { get; set; }
    public string Name { get; set; } = null!;
    public bool Isactive { get; set; }
    public virtual ICollection<Category> InverseParentcategory { get; set; } = new List<Category>();
    public virtual Category? Parentcategory { get; set; }
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
