namespace shoppingcart
{
  using Nancy;

  public class ShoppingCartModule : NancyModule
  {
      public ShoppingCartModule()
      {
          Get("/shoppingcart/{userid:int}",  parameters => 
          {
              var userId = (int)parameters.userid;
              if (userId == 123) 
              {
                var thisCart = new ShoppingCart(userId);
                ShoppingCartItem item1 = new ShoppingCartItem(1, "Coffee", "Caffeine Delivery Sysytem", 12.99);  
                ShoppingCartItem item2 = new ShoppingCartItem(2, "Heavy Cream", "Fat Delivery System", 4.99); 
                thisCart.AddItem(item1);
                thisCart.AddItem(item2);
                return thisCart;
              }
              else
              {
                //if no cart found for that user we return a new, empty cart
                return new ShoppingCart(userId);
              }
          });
      }
  }
}