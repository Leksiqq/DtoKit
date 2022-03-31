namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Заглушка для <see cref="ListDeserializer{T}"/> декларирующая, что целевой <see cref="IList{T}"/> должен быть 
/// перезаписан или дополнен, в зависимости от наследующего класса. <seealso cref="AppendableListStub{T}"/>, <seealso cref="RewritableListStub{T}"/>
/// </para>
/// <para xml:lang="en">
/// Stub for <see cref="ListDeserializer{T}"/> declaring that the target <see cref="IList{T}"/> should be
/// overwritten or added, depending on the inheriting class. <seealso cref="AppendableListStub{T}"/>, <seealso cref="RewritableListStub{T}"/>
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
public class ListStub<T>
{
    /// <summary>
    /// <para xml:lang="ru">
    /// Объект-заглушка для формального параметра  
    /// <see cref="AsyncEnumerableJsonSerializer{T}.Write(System.Text.Json.Utf8JsonWriter, ListStub{T}, System.Text.Json.JsonSerializerOptions)"/>
    /// </para>
    /// <para xml:lang="en">
    /// Stub object for formal parameter of 
    /// <see cref="AsyncEnumerableJsonSerializer{T}.Write(System.Text.Json.Utf8JsonWriter, ListStub{T}, System.Text.Json.JsonSerializerOptions)"/>
    /// </para>
    /// </summary>
    public static ListStub<T> Instance { get; private set; } = new();

    protected ListStub() { }
}
