using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Net.Leksi.Dto;

/// <summary>
/// <para xml:lang="ru">
/// Фабрика по производству <see cref="JsonConverter"/> для специальной обработки 
/// зарегистрированных интерфейсов и <see cref="IList"/>'ов таких элементов. 
/// Под специальной обработкой понимается:
/// 1) повторное использование объектов и списков объектов
/// в противоположность их созданию при каждой десериализации.
/// 2) повторное использование списка верхнего уровня с указанием, добавить в него новые элементы или
/// перезаписать.
/// </para>
/// /// <para xml:lang="en">
/// Manufacturing factory <see cref="JsonConverter"/> for special processing
/// registered interfaces and <see cref="IList"/>'s of such elements.
/// Special processing means:
/// 1) reuse of objects and lists of objects
/// as opposed to creating them on every deserialization.
/// 2) reuse the top-level list, specifying whether to add new elements to it or
/// overwrite.
/// </para>
/// </summary>
public class DtoJsonConverterFactory : JsonConverterFactory
{

    private object? _target = null;
    private bool _used = false;
    private bool _useEndOfDataNull = true;
    private bool _withMagic = false;
    private bool _withOnlyKeysForRepeated = true;

    internal TypesForest TypesForest { get; init; }

    /// <summary>
    /// <para xml:lang="en">
    /// Пул объектов удалённых из списков или бывших значением свойства, которому при новой 
    /// десериализации был присвоен null;
    /// </para>
    /// <para xml:lang="en">
    /// The pool of objects removed from the lists or the former value of the property, which, with the new
    /// deserialization was set to null;
    /// </para>
    /// </summary>
    public Dictionary<Type, List<object>> ObjectsPool { get; init; } = new();

    /// <summary>
    /// <para xml:lang="ru">
    /// "Крепление" для объекта, запланированного для повторного использования
    /// </para>
    /// <para xml:lang="en">
    /// "Mount" for an object scheduled for reuse
    /// </para>
    /// </summary>
    public object? Target
    {
        get
        {
            return _target;
        }
        set
        {
            if (
                value is null
                || TypesForest.ServiceProvider.Select(sd => sd.ServiceType).Any(t => t.IsAssignableFrom(value!.GetType()))
                || value is IList
                    && value.GetType().IsGenericType
                    && TypesForest.ServiceProvider.Select(sd => sd.ServiceType).Any(t => t.IsAssignableFrom(value.GetType().GenericTypeArguments[0]))
            )
            {
                _target = value;
            }
            else
            {
                throw new ArgumentException(value!.ToString());
            }
        }
    }

    /// <summary>
    /// <para xml:lang="ru">
    /// Значение свойства <c>true</c> указывает, что элемент null в списке верхнего уровня означает
    /// окончание передачи серии данных (смотрите также <seealso cref="Net.Leksi.PartialLoader.PartialLoader{T}"/>)
    /// </para>
    /// <para xml:lang="en">
    /// The <c>true</c> property value indicates that the null element in the top-level list means
    /// end of data series transfer (see also <seealso cref="Net.Leksi.PartialLoader.PartialLoader{T}"/>)
    /// </para>
    /// </summary>
    public bool UseEndOfDataNull
    {
        get => _useEndOfDataNull;
        set
        {
            CheckUsed();
            _useEndOfDataNull = value;
        }
    }

    /// <summary>
    /// <para xml:lang="ru">
    /// Значение свойства <c>true</c> указывает, что при <see cref="UseEndOfDataNull"/><c> == true</c> встретился элемент null в списке верхнего уровня
    /// </para>
    /// <para xml:lang="en">
    /// The <c>true</c> property value indicates that when <see cref="UseEndOfDataNull"/><c> == true</c>, a null element was encountered in the top-level list
    /// </para>
    /// </summary>
    public bool IsEndOfData { get; internal set; }

    public bool WithMagic
    {
        get => _withMagic;
        set
        {
            CheckUsed();
            _withMagic = value;
        }
    }

    public bool WithKeyOnlyForRepeated
    {
        get => _withOnlyKeysForRepeated;
        set
        {
            CheckUsed();
            _withOnlyKeysForRepeated = value;
        }
    }


    public DtoJsonConverterFactory(TypesForest typesForest)
    {
        TypesForest = typesForest ?? throw new ArgumentNullException(nameof(typesForest)) ;
    }


    #region Реализация JsonConverterFactory

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        _used = true;
        // Если вызвана десериализация для одного из типов-заглушек: AppendableListStub<> или RewritableListStub<>,
        // используемых для 2) варианта применения (см. описание класса)
        if (TypesForest.ServiceProvider.Select(sd => sd.ServiceType).Any(t => typeof(ListStub<>).MakeGenericType(new Type[] { t }).IsAssignableFrom(typeToConvert)))
        {
            return true;
        }
        bool result = TypesForest.ServiceProvider.IsRegistered(typeToConvert);
        if (result)
        {
            TypesForest.PlantTypeTree(typeToConvert);
        }
        return result;
    }

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        JsonConverter converter;
        Type? type = TypesForest.ServiceProvider.Select(sd => sd.ServiceType).Where(
                t => typeof(ListStub<>).MakeGenericType(new Type[] { t }).IsAssignableFrom(typeToConvert)
            ).FirstOrDefault((Type?)null);
        if (type is { })
        {
            // Если вызвана десериализация для одного из типов-заглушек: AppendableList<> или RewritableList<>,
            // используемых для 2) варианта применения (см. описание класса)
            converter = (JsonConverter)Activator.CreateInstance(
                    typeof(ListDeserializer<>).MakeGenericType(new Type[] { type }),
                    args: new object[] { this, typeToConvert == typeof(AppendableListStub<>).MakeGenericType(new Type[] { type }) }
                )!;
        }
        else
        {
            // Если вызвана сериализация/десериализация для одного из зарегистрированных интерфейсов,
            // используемых для 1) варианта применения (см. описание класса)
            converter = (JsonConverter)Activator.CreateInstance(
                    typeof(DtoConverter<>).MakeGenericType(new Type[] { typeToConvert }), 
                    args: new object[] { this }
                )!;
        }

        return converter;
    }
    #endregion

    private void CheckUsed()
    {
        if (_used)
        {
            throw new InvalidOperationException("Cannot configure after using.");
        }
    }

}

