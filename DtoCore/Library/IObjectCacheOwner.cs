namespace Net.Leksi.Dto;

public interface IObjectCacheOwner
{
    void OnObjectCachesClear(object? sender, EventArgs args);
}
