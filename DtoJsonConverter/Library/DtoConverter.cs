using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Конвертер для сериализации/десериализации DTO (предполагается, что свойства реализуемого интерфейса пишутся и читаются, 
/// возможны свойства, отмеченные атрибутом KeyAttribute, отсутствующие в интерфейсе с целью сокрытия конкреной реализации
/// хранения). Порождается фабрикой <see cref="TransferJsonConverterFactory"/> если соответствующий интерфейс в ней зарегистрирован
/// </para>
/// <para xml:lang="en">
/// Converter for DTO serialization/deserialization (it is assumed that the properties of the implemented interface are written and read,
/// possible properties marked with the KeyAttribute attribute that are not present in the interface in order to hide the concrete implementation
/// storage). Raised by the factory <see cref="TransferJsonConverterFactory"/> if the corresponding interface is registered in it
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
internal class DtoConverter<T> : JsonConverter<T>
{
    private const string MagicPropertyName = "$magic";
    private const string MagicPropertyValue = "applied";
    private const string DollarSign = "$";

    private readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _readerPropertiesCache = new();

    private readonly Dictionary<Type, List<PropertyInfo>> _writerPropertiesCache = new();

    private readonly DtoJsonConverterFactory _factory;

    public DtoConverter(DtoJsonConverterFactory factory)
    {
        _factory = factory;
    }

    #region Реализация JsonConverter<T>

    /// <inheritdoc/>
    /// <exception cref="JsonException">
    /// <para xml:lang="ru">
    /// Если элемент JSON не является объектом
    /// </para>
    /// <para xml:lang="en">
    /// If the JSON element is not an object
    /// </para>
    /// </exception>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {

            T item = default(T)!;
            TypeNode typeNode = _factory.TypesForest.GetTypeNode(typeof(T));

            bool itemAssigned = false;

            if (_factory.Target is { } target)
            {
                if (
                    target.GetType().IsClass
                        && typeToConvert.IsAssignableFrom(target.GetType())
                )
                {
                    item = (T)target;
                    itemAssigned = true;
                }
                _factory.Target = null;
            }
            if (!itemAssigned)
            {
                if (_factory.ObjectsPool.TryGetValue(typeof(T), out List<object> pool) && pool.Count > 0)
                {
                    item = (T)pool[0];
                    pool.RemoveAt(0);
                }
                else
                {
                    item = (T)_factory.TypesForest.ServiceProvider.GetRequiredService(typeof(T));
                }
            }

            Type itemType = item.GetType();
            _factory.TypesForest.ConfirmTypeNode(typeNode, itemType);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return item;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }


                string? propertyName = reader.GetString();
                if(propertyName is null)
                {
                    throw new JsonException();
                }

                if(propertyName.StartsWith(DollarSign))
                {
                    var obj = JsonSerializer.Deserialize<object>(ref reader);
                }
                else 
                {
                    PropertyNode propertyNode = _factory.TypesForest.GetPropertyNode(typeNode, propertyName);

                    if (propertyNode is null)
                    {
                        throw new JsonException();
                    }

                    bool propertyValueAssigned = false;

                    if (propertyNode.PropertyInfo.PropertyType.IsEnum)
                    {
                        reader.Read();
                        propertyNode.PropertyInfo.SetValue(item, Enum.Parse(propertyNode.PropertyInfo.PropertyType,
                            reader.GetString()!));
                        propertyValueAssigned = true;
                    }
                    if (
                        !propertyValueAssigned
                        && typeof(IList).IsAssignableFrom(propertyNode.TypeNode.Type)
                    )
                    {
                        Type listType = typeof(IList<>).MakeGenericType(
                                propertyNode.TypeNode.Type.GenericTypeArguments);
                        Type elementType = propertyNode.TypeNode.Type.GenericTypeArguments[0];
                        if (listType.IsAssignableFrom(propertyNode.TypeNode.Type))
                        {
                            IList? list = (IList)propertyNode.PropertyInfo.GetValue(item)!;
                            if (list is null)
                            {
                                list = (IList)JsonSerializer.Deserialize(ref reader,
                                    propertyNode.TypeNode.Type, options)!;

                                if (propertyNode.PropertyInfo.GetValue(item) != list)
                                {
                                    propertyNode.PropertyInfo.SetValue(item, list);
                                }
                                propertyValueAssigned = true;
                            }
                            else
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.Null)
                                {
                                    propertyNode.PropertyInfo.SetValue(item, null);
                                    propertyValueAssigned = true;
                                }
                                else
                                {
                                    if (reader.TokenType == JsonTokenType.StartArray)
                                    {
                                        int i = 0;
                                        for (; reader.Read() && reader.TokenType != JsonTokenType.EndArray; i++)
                                        {
                                            bool hasElement = i < list.Count;
                                            if (_factory.TypesForest.ServiceProvider.Select(sd => sd.ServiceType).Any(type => type.IsAssignableFrom(elementType)))
                                            {
                                                if (hasElement)
                                                {
                                                    _factory.Target = list[i];
                                                }
                                                else if (_factory.ObjectsPool.TryGetValue(elementType, out List<object> pool) && pool.Count > 0)
                                                {
                                                    _factory.Target = pool[0];
                                                    pool.RemoveAt(0);
                                                }
                                            }
                                            object? value = JsonSerializer.Deserialize(ref reader, elementType, options);
                                            if (!hasElement || value != list[i])
                                            {
                                                if (i < list.Count)
                                                {
                                                    list[i] = value;
                                                }
                                                else
                                                {
                                                    list.Add(value);
                                                }
                                            }
                                        }
                                        if (i < list.Count && !_factory.ObjectsPool.ContainsKey(elementType))
                                        {
                                            _factory.ObjectsPool[elementType] = new List<object>();
                                        }
                                        while (i < list.Count)
                                        {
                                            _factory.ObjectsPool[elementType].Add(list[i]);
                                            list.RemoveAt(i);
                                        }
                                        propertyValueAssigned = true;
                                    }
                                    else
                                    {
                                        throw new JsonException();
                                    }
                                }
                            }

                        }
                    }
                    if (
                        !propertyValueAssigned
                        && _factory.TypesForest.ServiceProvider.Select(sd => sd.ServiceType)
                            .Any(type => type.IsAssignableFrom(propertyNode.PropertyInfo.PropertyType)))
                    {
                        _factory.Target = propertyNode.PropertyInfo.GetValue(item);
                    }
                    if (!propertyValueAssigned)
                    {
                        object? value = JsonSerializer.Deserialize(ref reader, propertyNode.TypeNode.Type, options);
                        object? prevValue = propertyNode.PropertyInfo.GetValue(item);
                        if (prevValue != value)
                        {
                            if (value is null)
                            {
                                if (!_factory.ObjectsPool.ContainsKey(propertyNode.PropertyInfo.PropertyType))
                                {
                                    _factory.ObjectsPool[prevValue!.GetType()] = new List<object>() { prevValue };
                                }
                                else
                                {
                                    _factory.ObjectsPool[prevValue!.GetType()].Add(prevValue);
                                }
                            }
                            propertyNode.PropertyInfo.SetValue(item, value);
                        }
                        propertyValueAssigned = true;
                    }
                }

            }
        }
        throw new JsonException();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            Type? actualType = value!.GetType();

            TypeNode typeNode = _factory.TypesForest.GetTypeNode(typeof(T));

            _factory.TypesForest.ConfirmTypeNode(typeNode, actualType);

            writer.WriteStartObject();
            if (_factory.ShouldApplyMagic)
            {
                writer.WritePropertyName(MagicPropertyName);
                writer.WriteStringValue(MagicPropertyValue);
            }
            foreach (PropertyNode propertyNode in typeNode.ChildNodes)
            {
                writer.WritePropertyName(propertyNode.PropertyName);
                if (propertyNode.PropertyInfo.PropertyType.IsEnum)
                {
                    writer.WriteStringValue(propertyNode.PropertyInfo.GetValue(value)?.ToString());
                }
                else
                {
                    JsonSerializer.Serialize(writer, propertyNode.PropertyInfo.GetValue(value), propertyNode.TypeNode.Type, options);
                }
            }
            writer.WriteEndObject();
        }
    }

    #endregion
}
