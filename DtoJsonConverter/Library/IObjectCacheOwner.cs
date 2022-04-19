namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Вспомогательный интерфейс для системного использования, чтобы абстрагироваться от параметрического аргумента <see cref="DtoConverter{T}"/>,
/// не затаскивать его в <see cref="DtoJsonConverterFactory"/>
/// </para>
/// <para xml:lang="en">
/// Helper interface for system use to abstract from parametric argument of <see cref="DtoConverter{T}"/>,
/// not to drag it into <see cref="DtoJsonConverterFactory"/>
/// </para>
/// </summary>
internal interface IObjectCacheOwner
{
    /// <summary>
    /// <para xml:lang="ru">
    /// Обработчик соответствующего события
    /// </para>
    /// <para xml:lang="en">
    /// Handler for the corresponding event
    /// </para>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void OnObjectCachesClear(object? sender, EventArgs args);
}
