using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace Net.Leksi.Dto;

public class DtoServiceProvider : IServiceProvider, IServiceCollection
{
    private List<ServiceDescriptor> _serviceDescriptors = new();
    private IServiceCollection? _services = null;

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

    public IServiceProvider? ServiceProvider { get; set; }

    public int Count => throw new InvalidOperationException();

    public bool IsReadOnly => throw new InvalidOperationException();

    public static void Install(IServiceCollection services, Action<IServiceCollection> configure)
    {
        DtoServiceProvider instance = new(null);
        services.AddSingleton<DtoServiceProvider>(serviceProvider =>
        {
            instance.ServiceProvider = serviceProvider;
            return instance;
        });
        instance._services = services;
        configure?.Invoke(instance);
        instance._services = null;
    }

    public DtoServiceProvider(IServiceProvider? serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public bool IsRegistered<T>() where T : class
    {
        return IsRegistered(typeof(T));
    }

    public bool IsRegistered(Type serviceType)
    {
        return _serviceDescriptors.Any(item => item.ServiceType == serviceType);
    }

    public object? GetService(Type serviceType)
    {
        ServiceDescriptor item = _serviceDescriptors.Where(item => item.ServiceType == serviceType).FirstOrDefault();
        if (item is { } && InstantiateService(item) is { } result)
        {
            return result;
        }
        return ServiceProvider.GetService(serviceType);
    }

    private object? InstantiateService(ServiceDescriptor item)
    {
        if(item.ImplementationInstance is { })
        {
            return item.ImplementationInstance;
        }
        if (item.ImplementationFactory is Func<IServiceProvider, object> implementation)
        {
            return implementation.Invoke(ServiceProvider);
        }
        return null;
    }

    public void Add(ServiceDescriptor item)
    {
        if (!item.ServiceType.IsInterface)
        {
            throw new ArgumentException($"{item.ServiceType} is not an interface");
        }
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

    public void Clear()
    {
        throw new InvalidOperationException();
    }

    public bool Contains(ServiceDescriptor item)
    {
        throw new InvalidOperationException();
    }

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        throw new InvalidOperationException();
    }

    public IEnumerator<ServiceDescriptor> GetEnumerator()
    {
        return _serviceDescriptors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _serviceDescriptors.GetEnumerator();
    }

    public int IndexOf(ServiceDescriptor item)
    {
        throw new InvalidOperationException();
    }

    public void Insert(int index, ServiceDescriptor item)
    {
        throw new InvalidOperationException();
    }

    public bool Remove(ServiceDescriptor item)
    {
        throw new InvalidOperationException();
    }

    public void RemoveAt(int index)
    {
        throw new InvalidOperationException();
    }

}
