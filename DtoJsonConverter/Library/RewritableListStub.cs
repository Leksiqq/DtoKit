namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Тип-заглушка для <see cref="ListDeserializer{T}"/> декларирующая, что целевой <see cref="IList{T}"/> должен быть 
/// перезаписан
/// </para>
/// <para xml:lang="en">
/// Stub type for <see cref="ListDeserializer{T}"/> declaring that the target <see cref="IList{T}"/> should be
/// overwritten
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class RewritableListStub<T>: ListStub<T>
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
    public new static RewritableListStub<T> Instance { get; private set; } = new();
}
