﻿using Net.Leksi.Dto;

namespace DtoKitDemoEntities;

public class Vessel
{
    [Key]
    public string ID_VESSEL { get; set; }

    public double Length { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public double Brutto { get; set; }

    public double Netto { get; set; }

    public double LineMeters { get; set; }

    public string Description { get; set; }

    public int RiffCount { get; set; }

    public bool IsOcean { get; set; }

    public string CallSign { get; set; }

    public string Name { get; set; }

}
