namespace Net.Leksi.Dto;
/// <summary>
/// <para xml:lang="ru">
/// Делегат, определяющий сигнатуру методов, которые вызываются перед и после каждого запроса при использовании 
/// <see cref="DtoBuilder"/> с классом-хэлпером. 
/// <see cref="BeforeAttribute"/>, <see cref="AfterAttribute"/>
/// </para>
/// <para xml:lang="en">
/// Delegate that defines the signature of methods that are called before and after each request when used
/// <see cref="DtoBuilder"/> with helper class.
/// <see cref="BeforeAttribute"/>, <see cref="AfterAttribute"/>
/// </para>
/// </summary>
/// <param name="path">
/// <para xml:lang="ru">
/// Абсолютный путь от корня дерева объекта
/// </para>
/// <para xml:lang="en">
/// Absolute path from the root of the object tree
/// </para>
/// </param>
/// <param name="value"></param>
public delegate void BeforeOrAfterProcessor(string path, object value);
