//------------------------------
// MVC Controller interface DtoKit.Demo.IDemoController (Generated automatically)
//------------------------------
using System;
using System.Threading.Tasks;
using DtoKit.Demo;

namespace DtoKit.Demo;

public interface IDemoController
{
    Task GetShipCalls(ShipCallsFilter filter);
}
