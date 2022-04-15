namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// При использовании <see cref="DtoBuilder"/> с классом-хэлпером связывает абсолютный 
/// путь от корня дерева объекта с соответствующим сеттером.
/// </para>
/// <para xml:lang="en">
/// When using <see cref="DtoBuilder"/> with a helper class, binds an absolute
/// path from the root of the object tree with the corresponding setter.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PathAttribute: Attribute
{
    /// <summary>
    /// <para xml:lang="ru">
    /// Абсолютный путь от корня дерева объекта.
    /// </para>
    /// <para xml:lang="en">
    /// Absolute path from the root of the object tree.
    /// </para>
    /// </summary>
    public string Path { get; init; }
    /// <summary>
    /// <para xml:lang="ru">
    /// Тип соответствующего делегата <see cref="NodeSetter"/> или <see cref="TerminalSetter"/> 
    /// для контроля, что разработчик понимает, какой тип вершины он обрабатывает.
    /// </para>
    /// <para xml:lang="en">
    /// Absolute path from the root of the object tree.
    /// </para>
    /// <para xml:lang="en">
    /// The type of the corresponding delegate <see cref="NodeSetter"/> or <see cref="TerminalSetter"/>
    /// to control that the developer understands what type of vertex he is processing.
    /// </para>
    /// </summary>
    public Type DelegateType { get; init; }
    /// <summary>
    /// <para xml:lang="ru">
    /// Инициализирует атрибут путём и типом делегата
    /// </para>
    /// <para xml:lang="en">
    /// Initializes the attribute with the path and delegate's type
    /// </para>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="delegateType"></param>
    public PathAttribute(string path, Type delegateType)
    {
        Path = path;
        DelegateType = delegateType;
    }
}
