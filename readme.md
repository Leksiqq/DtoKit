## Net.Leksi.DtoKit

A library for processing of an object as DTO in accordance with the applied interface.
Supports partial building and JSON serialization/deserialization.

## Prerequisites
1. Target platform: .NET 6.0
2. Target CPU: Any

## Usage

	var builder = WebApplication.CreateBuilder(args);

	...

    // For interfaces being used as Dto
    builder.Services.AddDtoKit(
        provider =>
        {
            provider.AddTransient<ILine, Line>();
            provider.AddTransient<IPort, Port>();
            provider.AddTransient<IVessel, Vessel>();
            provider.AddTransient<IRoute, Route>();
            provider.AddTransient<IShipCall, ShipCall>();
            provider.AddTransient<IVesselForShipCallList, Vessel>();
            provider.AddTransient<IRouteForShipCallList, Route>();
            provider.AddTransient<IShipCallForList, ShipCall>();
            provider.AddTransient<IShipCallAdditionInfo, ShipCall>();
        }
    );	
    
    ...

	var app = builder.Build();


## API
See [Documentation](https://dtokit.sourceforge.io/index_en.html)
