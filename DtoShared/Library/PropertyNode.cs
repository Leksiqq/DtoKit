using System.Reflection;

namespace Net.Leksi.Dto
{
    public class PropertyNode
    {
        public string? Name { get; set; } = null;
        public PropertyInfo? PropertyInfo { get; set; } = null;

        public TypeNode TypeNode { get; set; } = null!;
        public bool IsNullable { get; set; } = false;

        public bool IsLeaf => TypeNode.ChildNodes is null;
    }
}
