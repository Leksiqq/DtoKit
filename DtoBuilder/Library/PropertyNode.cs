using System.Reflection;

namespace Net.Leksi.Dto
{
    internal class PropertyNode
    {
        internal PropertyInfo PropertyInfo { get; set; } = null!;
        internal PropertyInfo AliasPropertyInfo { get; set; } = null!;
        internal List<PropertyNode>? ChildNodes { get; set; } = null;

        internal bool IsConfigured { get; set; } = false;

        internal int Id { get; set; } = 0;

    }
}
