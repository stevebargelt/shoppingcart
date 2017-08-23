namespace ShoppingCartUnitTests
{
  using System;
  using System.Threading.Tasks;
  using shoppingcart;
  using Nancy;
  using Nancy.Testing;
  using Xunit;

  public class ShoppingCartModule_should
  {
   
    [Fact]
    public async Task Should_return_status_ok_when_route_exists()
    {
        // Given
        var bootstrapper = new DefaultNancyBootstrapper();
        var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/json"));

        // When
        var result = await browser.Get("/shoppingcart/123", with => {
            with.HttpRequest();
        });

        // Then
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
  }
}