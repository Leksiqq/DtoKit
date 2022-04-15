namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Делегат, определяющий сигнатуру метода, который запрашивает значение свойства, не являющегося 
/// зарегистрированным типом при использовании <see cref="DtoBuilder"/> с классом-хэлпером. 
/// Должен быть также помечен <see cref="PathAttribute"/>.
/// </para>
/// <para xml:lang="en">
/// A delegate that defines the signature of a method that requests the value of a property that is not
/// a registered type when using <see cref="DtoBuilder"/> with a helper class.
/// Should also be marked with <see cref="PathAttribute"/>.
/// </para>
/// </summary>
/// <param name="value">
/// <para xml:lang="ru">
/// Текущее значение свойства
/// </para>
/// <para xml:lang="en">
/// The current value of the property
/// </para>
/// </param>
/// <returns>
/// <para xml:lang="ru">
/// Новое или старое значение свойства.
/// </para>
/// <para xml:lang="en">
/// New or old property value.
/// </para>
/// </returns>
public delegate object TerminalSetter(object value);
