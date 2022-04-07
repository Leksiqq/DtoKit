using Microsoft.Extensions.DependencyInjection;

namespace Net.Leksi.Dto;

public class DtoKit
{
    public static void Install(IServiceCollection services, Action<IServiceCollection> configure)
    {
        DtoServiceProvider.Install(services, configure);
        services.AddSingleton(opt => new TypesForest(opt.GetRequiredService<DtoServiceProvider>()));
        services.AddTransient(opt => new DtoBuilder(opt.GetRequiredService<TypesForest>()));
        services.AddTransient(opt => new DtoJsonConverterFactory(opt.GetRequiredService<TypesForest>()));
    }
}
