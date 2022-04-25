//------------------------------
// MVC Controller proxy class DtoKit.Demo.DemoControllerProxy (Generated automatically)
//------------------------------
using Microsoft.AspNetCore.Mvc;
using Net.Leksi.Server.Contract;

namespace DtoKit.Demo;

[Connector(typeof(IConnector))]
public class DemoControllerProxy : BaseControllerProxy, IDemoController
{
    public DemoControllerProxy(IServiceProvider? services) : base(services) { }
    [Route("/shipCalls")]
    [HttpGet]
    public async Task GetShipCalls(ShipCallsFilter filter)
    {
        Controller controller = (Controller)HttpContext.RequestServices.GetRequiredService<IDemoController>();
        controller.ControllerContext = ControllerContext;
        await ((IDemoController)controller).GetShipCalls(filter);
    }
}
