using System;
using System.Collections.ObjectModel;
using System.Linq;
using MegaMarketLibnk.Models;
using ReactiveUI;

namespace MegaMarketLibnk.ViewModels;

public class SellerStockRow
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = "";
    public int Quantity { get; set; }
    public int EditableQty { get; set; }
    public int Reserved { get; set; }
}

public class SellerProductRow
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Sku { get; set; } = "";
    public decimal Price { get; set; }
    public int TotalStock { get; set; }
    public bool LowStock => TotalStock <= 5;
    public string DbStatus { get; set; } = "В БД";
    public ObservableCollection<SellerStockRow> Stocks { get; } = new();
}

public class SellerCabinetViewModel : ViewModelBase
{
    public string UserName => SF.User?.Email ?? "Гость";
    public ObservableCollection<SellerProductRow> Products { get; } = new();

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> GoHomeCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> LogoutCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RefreshCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddProductCommand { get; }
    public ReactiveCommand<SellerStockRow, System.Reactive.Unit> SaveStockCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> BatchUpdateCommand { get; }
    private string newName = ""; public string NewName { get => newName; set => this.RaiseAndSetIfChanged(ref newName, value); }
    private string newSku = ""; public string NewSku { get => newSku; set => this.RaiseAndSetIfChanged(ref newSku, value); }
    private decimal newPrice; public decimal NewPrice { get => newPrice; set => this.RaiseAndSetIfChanged(ref newPrice, value); }
    private int newQty; public int NewQty { get => newQty; set => this.RaiseAndSetIfChanged(ref newQty, value); }
    private int batchDelta; public int BatchDelta { get => batchDelta; set => this.RaiseAndSetIfChanged(ref batchDelta, value); }
    private bool isAdding; public bool IsAdding { get => isAdding; set => this.RaiseAndSetIfChanged(ref isAdding, value); }
    private string addStatus = ""; public string AddStatus { get => addStatus; set => this.RaiseAndSetIfChanged(ref addStatus, value); }

    public SellerCabinetViewModel()
    {
        GoHomeCommand = ReactiveUI.ReactiveCommand.Create(GoHome);
        LogoutCommand = ReactiveUI.ReactiveCommand.Create(Logout);
        RefreshCommand = ReactiveUI.ReactiveCommand.Create(Load);
        AddProductCommand = ReactiveUI.ReactiveCommand.Create(AddProduct);
        SaveStockCommand = ReactiveUI.ReactiveCommand.Create<SellerStockRow>(SaveStock);
        BatchUpdateCommand = ReactiveUI.ReactiveCommand.Create(BatchUpdate);
        Load();
    }

    private void Load()
    {
        Products.Clear();
        if (SF.User?.Sellerprofile == null && SF.User != null)
            SF.Context.Entry(SF.User).Reference(x => x.Sellerprofile).Load();
        if (SF.User?.Sellerprofile == null && SF.User != null)
            SF.Context.Entry(SF.User).Reference(x => x.Sellerprofile).Load();
        var seller = SF.User?.Sellerprofile;
        if (seller == null) return;
        foreach (var p in SF.Context.Products.Where(p => p.Sellerid == seller.Id).ToList())
        {
            SF.Context.Entry(p).Collection(x => x.Productstocks).Load();
            var row = new SellerProductRow { Id = p.Id, Name = p.Name, Sku = p.Sku, Price = p.Baseprice, DbStatus = "В БД" };
            foreach (var st in p.Productstocks)
            {
                SF.Context.Entry(st).Reference(x => x.Warehouse).Load();
                row.Stocks.Add(new SellerStockRow
                {
                    ProductId = p.Id,
                    WarehouseId = st.Warehouseid,
                    WarehouseName = st.Warehouse.Warehousename,
                    Quantity = st.Quantityinstock,
                    EditableQty = st.Quantityinstock,
                    Reserved = st.Reservedquantity
                });
                row.TotalStock += st.Quantityinstock;
            }
            Products.Add(row);
        }
    }

    private void AddProduct()
    {
        IsAdding = true;
        AddStatus = "Добавление...";
        try
        {
            if (SF.User?.Sellerprofile == null && SF.User != null)
                SF.Context.Entry(SF.User).Reference(x => x.Sellerprofile).Load();
            var seller = SF.User?.Sellerprofile;
            if (seller == null) { AddStatus = "Ошибка: продавец не найден"; return; }
            var wh = SF.Context.Warehouses.FirstOrDefault();
            if (wh == null)
            {
                wh = new Warehouse { Warehousename = "Основной склад", City = "Москва", Address = "ул. Складская, 1", Capacity = 10000 };
                SF.Context.Warehouses.Add(wh);
                SF.Context.SaveChanges();
            }
            var categoryId = SF.Context.Categories.Select(c => c.Id).FirstOrDefault();
            if (categoryId == 0) categoryId = 1;
            var p = new Product { Name = NewName, Sku = NewSku, Baseprice = NewPrice, Sellerid = seller.Id, Categoryid = categoryId, Statusid = 0 };
            p.Productstocks.Add(new Productstock { Warehouseid = wh.Id, Quantityinstock = NewQty, Reservedquantity = 0 });
            SF.Context.Products.Add(p);
            SF.Context.SaveChanges();
            NewName = ""; NewSku = ""; NewPrice = 0; NewQty = 0;
            Load();
            AddStatus = "Добавлено в БД";
        }
        catch (Exception ex)
        {
            AddStatus = "Не добавлено в БД: " + ex.Message;
        }
        finally
        {
            IsAdding = false;
        }
    }
    private void SaveStock(SellerStockRow row)
    {
        var st = SF.Context.Productstocks.FirstOrDefault(x => x.Productid == row.ProductId && x.Warehouseid == row.WarehouseId);
        if (st == null) return;
        st.Quantityinstock = row.EditableQty;
        SF.Context.SaveChanges();
        Load();
    }
    private void BatchUpdate()
    {
        if (SF.User?.Sellerprofile == null && SF.User != null)
            SF.Context.Entry(SF.User).Reference(x => x.Sellerprofile).Load();
        if (SF.User?.Sellerprofile == null && SF.User != null)
            SF.Context.Entry(SF.User).Reference(x => x.Sellerprofile).Load();
        var seller = SF.User?.Sellerprofile;
        if (seller == null) return;
        var ids = SF.Context.Products.Where(p => p.Sellerid == seller.Id).Select(p => p.Id).ToList();
        foreach (var st in SF.Context.Productstocks.Where(s => ids.Contains(s.Productid)))
            st.Quantityinstock += BatchDelta;
        SF.Context.SaveChanges();
        BatchDelta = 0;
        Load();
    }
    public void GoHome() => MainWindowViewModel.Instance.CurrentModel = new HomeViewModel();
    public void Logout() { SF.User = null; MainWindowViewModel.Instance.CurrentModel = new AuthViewModel(); }
}
