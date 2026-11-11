namespace MegaMarketLibnk.Models;
public class FeedTile
{
    public int Productid { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Color { get; set; } = "#2E2E36";
    public bool FromDb { get; set; }
    public string PriceText => Money(Price);
    public static string Money(decimal value) => string.Format("{0:0,0}", value).Replace(',', ' ');
}
public class ProductTile : FeedTile
{
}
public class SaleTile : FeedTile
{
    public decimal OldPrice { get; set; }
    public string OldPriceText => Money(OldPrice);
    public string Badge =>
        OldPrice > Price
            ? "−" + (int)Math.Round((OldPrice - Price) / OldPrice * 100) + "%"
            : string.Empty;
}
