
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json");
DtoKit.Demo.Setup.Configure(builder.Services);

var app = builder.Build();

app.Run();
