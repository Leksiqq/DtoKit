namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// При использовании <see cref="DtoBuilder"/> с классом-хэлпером помечает метод такого класса, 
/// который вызывается прежде всех запросов
/// </para>
/// <para xml:lang="en">
/// When using <see cref="DtoBuilder"/> with a helper class, marks the method of that class,
/// which is called before requests start
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SetupAttribute: Attribute
{
}
