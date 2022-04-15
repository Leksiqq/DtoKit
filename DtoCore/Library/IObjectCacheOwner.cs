namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Вспомогательный интерфейс для системного использования
/// </para>
/// <para xml:lang="en">
/// Helper interface for system use
/// </para>
/// </summary>
public interface IObjectCacheOwner
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
