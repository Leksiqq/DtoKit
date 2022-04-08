namespace Net.Leksi.Dto;

public class KeyStub<T>
{
    public T Source { get; init; }

    public KeyStub(T source)
    {
        Source = source;
    }
}
