namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Тип-заглушка для <see cref="ListDeserializer{T}"/> декларирующая, что в целевом <see cref="IList{T}"/> должены быть 
/// обновлены свойства существующих элементов
/// </para>
/// <para xml:lang="en">
/// Stub type for <see cref="ListDeserializer{T}"/> declaring that the target <see cref="IList{T}"/> should have
/// updated properties of existing elements
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class UpdateableListStub<T>: ListStub<T>
{
    /// <summary>
    /// <para xml:lang="ru">
    /// Объект-заглушка для формального возврата из 
    /// <see cref="ListDeserializer{T}.Read(ref System.Text.Json.Utf8JsonReader, Type, System.Text.Json.JsonSerializerOptions)"/>
    /// </para>
    /// <para xml:lang="en">
    /// Stub object for formal return from
    /// <see cref="ListDeserializer{T}.Read(ref System.Text.Json.Utf8JsonReader, Type, System.Text.Json.JsonSerializerOptions)"/>
    /// </para>
    /// </summary>
    public new static UpdateableListStub<T> Instance { get; private set; } = new();
}
