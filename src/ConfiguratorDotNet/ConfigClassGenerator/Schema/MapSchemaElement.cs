using ConfiguratorDotNet.Runtime;
using System.Linq;

namespace ConfiguratorDotNet.Generator;

internal class MapSchemaElement : SchemaElement
{
    private readonly Dictionary<XName, SchemaElement> children = new();

    public MapSchemaElement(SchemaElement? parent, XElement node) : base(parent, node)
    {
    }

    public override IEnumerable<SchemaElement> Children => this.children.Values;

    public void AddChild(string tagName, SchemaElement child)
    {
        this.children.Add(tagName, child);
    }

    public override void Validate()
    {
        foreach (var kvp in this.children)
        {
            kvp.Value.Validate();
        }
    }

    public override bool Equals(SchemaElement? other)
    {
        if (other is not MapSchemaElement map)
        {
            return false;
        }

        if (map.children.Count != this.children.Count)
        {
            return false;
        }

        foreach (var kvp in this.children)
        {
            XName key = kvp.Key;
            SchemaElement value = kvp.Value;

            if (!map.children.TryGetValue(key, out SchemaElement otherValue))
            {
                return false;
            }

            if (otherValue != value)
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        int hash = this.children.Count;
        foreach (var key in this.children)
        {
            hash <<= 6;
            hash ^= key.GetHashCode();
        }

        return hash;
    }

    public override void MergeWith(XElement element, IAttributeValidator validator)
    {
        Dictionary<XName, XElement> map = element.GetFilteredChildren().ToDictionary(x => x.Name, x => x);

        foreach (var kvp in map)
        {
            if (!this.children.TryGetValue(kvp.Key, out SchemaElement? value))
            {
                throw new ConfiguratorDotNetException("Merged file contains key not present in base file. Merging may not add new keys.");
            }

            value.MergeWith(kvp.Value, validator);
        }
    }
}
