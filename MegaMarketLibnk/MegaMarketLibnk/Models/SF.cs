namespace MegaMarketLibnk.Models;
public static class SF
{
    public static MegaMarketDbContext Context { get; } = new();
    public static User? User { get; set; }
}
