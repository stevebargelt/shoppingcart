namespace shoppingcart
{
  using Nancy;

  public class ShoppingCartModule : NancyModule
  {
      public ShoppingCartModule()
      {
          Get("/shoppingcart/{userid:int}",  parameters => "This WILL be the shopping cart for userid " + parameters.userid);
      }
  }
}