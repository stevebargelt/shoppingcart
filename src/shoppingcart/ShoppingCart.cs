namespace shoppingcart
{
  using System;
  using System.Collections.Generic;

  public class ShoppingCart
  {
    private HashSet<ShoppingCartItem> items = new HashSet<ShoppingCartItem>();

    public int UserId { get; }
    public IEnumerable<ShoppingCartItem> Items { get { return items; } }

    public ShoppingCart(int userId)
    {
      this.UserId = userId;
    }

    public void AddItem(ShoppingCartItem item)
    {
        this.items.Add(item);
    }
  }

  public class ShoppingCartItem
  {
    public int ProductCatalogueId { get; }
    public string ProductName { get; }
    public string Description { get; }
    public double Price { get; }

    public ShoppingCartItem(
      int productCatalogueId,
      string productName,
      string description,
      double price)
    {
      this.ProductCatalogueId = productCatalogueId;
      this.ProductName = productName;
      this.Description = description;
      this.Price = price;
    }

  }

}