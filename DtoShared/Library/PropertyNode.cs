using System.Reflection;

namespace Net.Leksi.Dto
{
    public class PropertyNode
    {
        //public string PropertyName { get; set; } = null!;
        public PropertyInfo? SourcePropertyInfo { get; set; } = null;
        public PropertyInfo? PropertyInfo { get; set; } = null;

        public TypeNode TypeNode { get; set; } = null!;
        public bool IsNullable { get; set; } = false;
    }
}
