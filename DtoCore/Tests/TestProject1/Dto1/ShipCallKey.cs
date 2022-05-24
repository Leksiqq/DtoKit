using Net.Leksi.Dto;
using System;

namespace DtoTestProject.Dto1;

public class ShipCallKey: IShipCallKey
{
    [Key]
    public string ID_LINE { get; set; }
    [Key]
    public int ID_ROUTE { get; set; }
}
