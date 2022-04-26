using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Класс объекта для регистрации множества интерфейсов, реализуемых DTO-типами, объекты которых будут загружаться, 
/// сериализоваться и десериализоваться в зависимости от применяемого интерфейса.
/// Также служит прокси для <see cref="IServiceProvider"/>, получаемого через механизм DI
/// </para>
/// <para xml:lang="en">
/// Object class for registering a set of interfaces implemented by DTO types whose objects will be loaded,
/// serialize and deserialize depending on the applied interface.
/// Also serves as a proxy for <see cref="IServiceProvider"/> obtained through the DI mechanism
/// </para>
/// </summary>
public class DtoServiceProvider : IServiceProvider, IServiceCollection
{
    private List<ServiceDescriptor> _serviceDescriptors = new();
    private IServiceCollection? _services = null;
    private IServiceProvider? _serviceProvider { get; set; }

    /// <inheritdoc/>
    public ServiceDescriptor this[int index]
    {
        get
        {
            return _serviceDescriptors[index];
        }
        set
        {
            _serviceDescriptors[index] = value;
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    public int Count => throw new NotImplementedException();

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    public bool IsReadOnly => throw new NotImplementedException();

    /// <summary>
    /// <para xml:lang="ru">
    /// Даёт возможность совместить регистрацию итерфейсов с их регистрацией в DI
    /// </para>
    /// <para xml:lang="en">
    /// Makes it possible to combine the registration of interfaces with their registration in DI
    /// </para>
    /// </summary>
    /// <param name="services">
    /// <para xml:lang="ru">
    /// Коллекция сервисов, предоставляемая хостом для конфигурации DI
    /// </para>
    /// <para xml:lang="en">
    /// Collection of services provided by host for DI configuration
    /// </para>
    /// </param>
    /// <param name="configure">
    /// <para xml:lang="ru">
    /// <see cref="Action{IServiceCollection}"/> для непосредственного выполненя регистрации
    /// </para>
    /// <para xml:lang="en">
    /// <see cref="Action{IServiceCollection}"/> to perform registration itself
    /// </para>
    /// </param>
    /// <example>
    /// <code>
    /// IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
    ///.ConfigureServices(serviceCollection =>
    ///{
    ///    DtoKit.Install(serviceCollection, services =>
    ///    {
    ///        services.AddTransient&lt;IShipCall, ShipCall&gt;();
    ///        services.AddTransient&lt;ILocation, Location&gt;();
    ///        services.AddTransient&lt;IRoute, Route&gt;();
    ///        services.AddTransient&lt;ILine, Line&gt;();
    ///        services.AddTransient&lt;IVessel, Vessel&gt;();
    ///        services.AddTransient&lt;IShipCallForListing, ShipCall&gt;();
    ///        services.AddTransient&lt;IShipCallAdditionalInfo, ShipCall&gt;();
    ///        services.AddTransient&lt;IArrivalShipCall, ShipCall&gt;();
    ///        services.AddTransient&lt;IDepartureShipCall, ShipCall&gt;();
    ///        services.AddTransient&lt;IRouteShort, Route&gt;();
    ///        services.AddTransient&lt;IVesselShort, Vessel&gt;();
    ///        services.AddTransient&lt;ITravelForListing, Travel&gt;();
    ///    });
    ///});
    ///host = hostBuilder.Build();
    /// </code>
    /// </example>
    public static void Install(IServiceCollection services, Action<IServiceCollection> configure)
    {
        DtoServiceProvider instance = new(null);
        services.AddSingleton<DtoServiceProvider>(serviceProvider =>
        {
            instance._serviceProvider = serviceProvider;
            return instance;
        });
        instance._services = services;
        configure?.Invoke(instance);
        instance._services = null;
    }

    /// <summary>
    /// <para xml:lang="ru">
    /// Конструктор с возможной инициализацией свойства <see cref="IServiceProvider"/>
    /// </para>
    /// <para xml:lang="en">
    /// Constructor with possible property initialization <see cref="IServiceProvider"/>
    /// </para>    
    /// </summary>
    /// <param name="serviceProvider">
    /// <para xml:lang="ru">
    /// <see cref="IServiceProvider"/>, получаемый через механизм DI
    /// </para>
    /// <para xml:lang="en">
    /// <see cref="IServiceProvider"/> obtained through the DI mechanism
    /// </para>
    /// </param>
    public DtoServiceProvider(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// <para xml:lang="ru">
    /// Проверяет зарегистрирован ли интерфейс
    /// </para>
    /// <para xml:lang="en">
    /// Checks if the interface is registered
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// <para xml:lang="ru">
    /// Проверяемый интерфейс
    /// </para>
    /// <para xml:lang="en">
    /// Tested interface
    /// </para>
    /// </typeparam>
    /// <returns></returns>
    public bool IsRegistered<T>() where T : class
    {
        return IsRegistered(typeof(T));
    }

    /// <summary>
    /// <para xml:lang="ru">
    /// Проверяет зарегистрирован ли интерфейс
    /// </para>
    /// <para xml:lang="en">
    /// Checks if the interface is registered
    /// </para>
    /// </summary>
    /// <param name="serviceType">
    /// <para xml:lang="ru">
    /// Проверяемый интерфейс
    /// </para>
    /// <para xml:lang="en">
    /// Tested interface
    /// </para>
    /// </param>
    /// <returns></returns>
    public bool IsRegistered(Type serviceType)
    {
        return _serviceDescriptors.Any(item => item.ServiceType == serviceType);
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType)
    {
        ServiceDescriptor? item = _serviceDescriptors.Where(item => item.ServiceType == serviceType).FirstOrDefault();
        if (item is { } && InstantiateService(item) is { } result)
        {
            return result;
        }
        return _serviceProvider?.GetService(serviceType);
    }

    /// <inheritdoc/>
    public void Add(ServiceDescriptor item)
    {
        //if (!item.ServiceType.IsInterface)
        //{
        //    throw new ArgumentException($"{item.ServiceType} is not an interface");
        //}
        if (item.Lifetime is not ServiceLifetime.Transient)
        {
            throw new InvalidOperationException($"{item.Lifetime} must be {ServiceLifetime.Transient} for {item.ServiceType}");
        }
        if(_services is { })
        {
            _serviceDescriptors.Add(new ServiceDescriptor(item.ServiceType, item.ServiceType, item.Lifetime));
            _services.Add(item);
        }
        else
        {
            _serviceDescriptors.Add(item);
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    public void Clear()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    public bool Contains(ServiceDescriptor item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    public IEnumerator<ServiceDescriptor> GetEnumerator()
    {
        return _serviceDescriptors.GetEnumerator();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _serviceDescriptors.GetEnumerator();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    public int IndexOf(ServiceDescriptor item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    public void Insert(int index, ServiceDescriptor item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    public bool Remove(ServiceDescriptor item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para xml:lang="ru">
    /// Не реализовано в этом классе
    /// </para>
    /// <para xml:lang="en">
    /// Not implemented in this class
    /// </para>
    /// </summary>
    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    private object? InstantiateService(ServiceDescriptor item)
    {
        if (item.ImplementationInstance is { })
        {
            return item.ImplementationInstance;
        }
        if (item.ImplementationFactory is Func<IServiceProvider, object> implementation && _serviceProvider is { })
        {
            return implementation.Invoke(_serviceProvider);
        }
        return null;
    }

}
