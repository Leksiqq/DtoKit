using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace Net.Leksi.Dto;

public class DtoServiceProvider : IServiceProvider, IServiceCollection
{
    private List<ServiceDescriptor> _serviceDescriptors = new();
    private IServiceProvider _serviceProvider;
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

    public int Count => _serviceDescriptors.Count;

    public bool IsReadOnly => ((ICollection<ServiceDescriptor>)_serviceDescriptors).IsReadOnly;

    public DtoServiceProvider(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool IsRegistered(Type serviceType)
    {
        return _serviceDescriptors.Any(item => item.ServiceType == serviceType);
    }

    public object? GetService(Type serviceType)
    {
        ServiceDescriptor item = _serviceDescriptors.Where(item => item.ServiceType == serviceType).FirstOrDefault();
        if(item is { })
        {
            return InstantiateService(item);
        }
        return null;
    }

    private object InstantiateService(ServiceDescriptor item)
    {
        if(item.ImplementationFactory is Func<IServiceProvider, object> implementation)
        {
            return implementation.Invoke(_serviceProvider);
        }
        try
        {
            return Activator.CreateInstance(item.ImplementationType);
        }
        catch(MissingMethodException ex)
        {
            return _serviceProvider.GetService(item.ServiceType);
        }
    }

    public void Add(ServiceDescriptor item)
    {
        if (item.Lifetime is not ServiceLifetime.Transient)
        {
            throw new InvalidOperationException($"{item.Lifetime} {item.ServiceType}");
        }
        _serviceDescriptors.Add(item);
    }

    public void Clear()
    {
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
        _serviceDescriptors.Insert(index, item);
    }

    public bool Remove(ServiceDescriptor item)
    {
        return _serviceDescriptors.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _serviceDescriptors.RemoveAt(index);
    }
}
