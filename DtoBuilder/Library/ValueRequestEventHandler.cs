namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Делегат обработчика события запроса значения узла или листа дерева объекта
/// </para>
/// <para xml:lang="en">
/// Delegate of the event handler of the request for the value of the node or leaf of the object tree
/// </para>
/// </summary>
/// <param name="args"></param>
public delegate void ValueRequestEventHandler(ValueRequestEventArgs args);
