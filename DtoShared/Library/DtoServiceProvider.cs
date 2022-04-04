using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace Net.Leksi.Dto;

public class DtoServiceProvider : IServiceProvider, IServiceCollection
{
    private List<ServiceDescriptor> _serviceDescriptors = new();
    private bool _commited = false;

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

    public void Commit()
    {
        CheckNotCommited();
        _commited = true;
    }

    public int Count => _serviceDescriptors.Count;

    public bool IsReadOnly => ((ICollection<ServiceDescriptor>)_serviceDescriptors).IsReadOnly;

    public static void Install(IServiceCollection services, Action<IServiceCollection> configure)
    {
        DtoServiceProvider instance = new(null);
        services.AddSingleton<DtoServiceProvider>(serviceProvider =>
        {
            instance.ServiceProvider = serviceProvider;
            return instance;
        });
        configure?.Invoke(instance);
        instance.Commit();
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
        CheckCommited();
        return _serviceDescriptors.Any(item => item.ServiceType == serviceType);
    }

    public object? GetService(Type serviceType)
    {
        CheckCommited();
        ServiceDescriptor item = _serviceDescriptors.Where(item => item.ServiceType == serviceType).FirstOrDefault();
        if (item is { })
        {
            return InstantiateService(item);
        }
        return null;
    }

    private object InstantiateService(ServiceDescriptor item)
    {
        if (item.ImplementationFactory is Func<IServiceProvider, object> implementation)
        {
            return implementation.Invoke(ServiceProvider);
        }
        try
        {
            return Activator.CreateInstance(item.ImplementationType);
        }
        catch (MissingMethodException ex)
        {
            return ServiceProvider.GetService(item.ServiceType);
        }
    }

    public void Add(ServiceDescriptor item)
    {
        CheckNotCommited();
        if (item.Lifetime is not ServiceLifetime.Transient)
        {
            throw new InvalidOperationException($"{item.Lifetime} {item.ServiceType}");
        }
        _serviceDescriptors.Add(item);
    }

    public void Clear()
    {
        CheckNotCommited();
        _serviceDescriptors.Clear();
    }

    public bool Contains(ServiceDescriptor item)
    {
        return _serviceDescriptors.Contains(item);
    }

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        _serviceDescriptors.CopyTo(array, arrayIndex);
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
        return _serviceDescriptors.IndexOf(item);
    }

    public void Insert(int index, ServiceDescriptor item)
    {
        CheckNotCommited();
        _serviceDescriptors.Insert(index, item);
    }

    public bool Remove(ServiceDescriptor item)
    {
        CheckNotCommited();
        return _serviceDescriptors.Remove(item);
    }

    public void RemoveAt(int index)
    {
        CheckNotCommited();
        _serviceDescriptors.RemoveAt(index);
    }

    private void CheckNotCommited()
    {
        if (_commited)
        {
            throw new InvalidOperationException("Already commited");
        }
    }
    private void CheckCommited()
    {
        if (!_commited)
        {
            throw new InvalidOperationException("Must be commited");
        }
    }
}
