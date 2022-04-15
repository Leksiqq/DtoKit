namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// При использовании <see cref="DtoBuilder"/> с классом-хэлпером помечает метод такого класса, 
/// который вызывается <b>после</b> каждого запроса
/// </para>
/// <para xml:lang="en">
/// When using <see cref="DtoBuilder"/> with a helper class, marks the method of that class,
/// which is called <b>after</b> each request
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AfterAttribute: Attribute
{
}
