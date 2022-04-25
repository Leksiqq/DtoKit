using DtoKit.Demo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dto = Net.Leksi.Dto;

Connector connector = new Connector();

connector.GetShipCalls(null, null);