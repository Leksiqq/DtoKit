using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Конвертер для десериализации списка верхнего уровня, то есть корневого элемента JSON.
/// Требуется кастомный, так как мы хотим использовать существующий список (например, <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> на клиенте, а не 
/// создавать каждый раз новый или не создавать новый промежуточный для заполнения. Также настраивается способ заполнения: перезапись
/// или добавление.
/// Данный конвертер не предназначен для сериализации
/// </para>
/// <para xml:lang="en">
/// Converter to deserialize the top-level list, i.e. the root JSON element.
/// Requires custom since we want to use an existing list (e.g. <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> on the client, not
/// create a new one every time or don't create a new intermediate one to fill. The filling method is also configured: overwriting
/// or adding.
/// This converter is not intended for serialization
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
internal class ListDeserializer<T> : JsonConverter<ListStub<T>>
{
    private readonly DtoJsonConverterFactory _factory;
    private readonly bool _appendable;

    public ListDeserializer(DtoJsonConverterFactory factory, bool appendable)
    {
        _factory = factory;
        _appendable = appendable;
    }

    /// <inheritdoc cref="JsonConverter{T}.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)"/>
    /// <returns>
    /// <para xml:lang="ru">
    /// Возвращает заглушку, заполненный лист передаётся через <see cref="TransferJsonConverterFactory.Target"/>
    /// </para>
    /// <para xml:lang="en">
    /// Returns a stub, the filled sheet is passed via <see cref="TransferJsonConverterFactory.Target"/>
    /// </para>
    /// </returns>
    /// <exception cref="JsonException">
    /// <para xml:lang="ru">
    /// 1) Если элемент JSON не является массивом
    /// 2) Если <see cref="TransferJsonConverterFactory.UseEndOfDataNull"/><c> == true</c>, при этом встретился 
    /// элемент после <c>null</c>
    /// </para>
    /// <para xml:lang="en">
    /// 1) If the JSON element is not an array
    /// 2) If <see cref="TransferJsonConverterFactory.UseEndOfDataNull"/><c> == true</c>, and there is an element after <c>null</c>
    /// </para>    
    /// /// </exception>
    public override ListStub<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IList? result = null;
        if (_factory.Target is null || !typeof(IList).IsAssignableFrom(_factory.Target.GetType()) || reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }
        _factory.IsEndOfData = false;
        result = (IList)_factory.Target;
        _factory.Target = null;
        int i = _appendable ? result.Count : 0;
        for (; reader.Read() && reader.TokenType != JsonTokenType.EndArray; i++)
        {
            if (_factory.IsEndOfData)
            {
                throw new JsonException();
            }
            bool hasElement = i < result.Count;
            if (_factory.TypesForest.ServiceProvider.Select(sd => sd.ServiceType).Any(type => type.IsAssignableFrom(typeof(T))))
            {
                if (hasElement)
                {
                    _factory.Target = result[i];
                }
                else if (_factory.ObjectsPool.TryGetValue(typeof(T), out List<object> pool) && pool.Count > 0)
                {
                    _factory.Target = pool[0];
                    pool.RemoveAt(0);
                }
            }
            T? value = JsonSerializer.Deserialize<T>(ref reader, options);
            if (value is null && _factory.ShouldUseEndOfDataNull)
            {
                _factory.IsEndOfData = true;
            }
            else
            {
                if (!hasElement || !object.ReferenceEquals(value, result[i]))
                {
                    if (i < result.Count)
                    {
                        result[i] = value;
                    }
                    else
                    {
                        result.Add(value);
                    }
                }
            }
        }
        if (i < result.Count && !_factory.ObjectsPool.ContainsKey(typeof(T)))
        {
            _factory.ObjectsPool[typeof(T)] = new List<object>();
        }
        while (i < result.Count)
        {
            _factory.ObjectsPool[typeof(T)].Add(result[i]);
            result.RemoveAt(i);
        }
        return _appendable ? AppendableListStub<T>.Instance : RewritableListStub<T>.Instance;
    }
    /// <summary>
    /// <para xml:lang="ru">
    /// Данный конвертер не предназначен для сериализации
    /// </para>
    /// <para xml:lang="en">
    /// This converter is not intended for serialization
    /// </para>
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public override void Write(Utf8JsonWriter writer, ListStub<T> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
