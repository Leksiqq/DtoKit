namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Атрибут для отметки не входящих в интерфейс DTO свойств, например, конкретных ключей базы данных, 
/// которые не должны быть известны ни модели, ни клиенту, но при этом участвовать в построении, сериализации
/// и десериализации
/// </para>
/// <para xml:lang="en">
/// An attribute for marking properties that are not part of the DTO interface, for example, specific database keys,
/// which should not be known to either the model or the client, but at the same time participate 
/// in the building, serialization
/// and deserialization
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class KeyAttribute : Attribute
{

}

