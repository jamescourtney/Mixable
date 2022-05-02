using ConfiguratorDotNet.Runtime;

namespace ConfiguratorDotNet.Generator;

internal class ListSchemaElement : SchemaElement
{
    private readonly List<SchemaElement> children = new();

    public ListSchemaElement(SchemaElement? parent, XElement node) : base(parent, node)
    {
    }

    public override IEnumerable<SchemaElement> Children => this.children;

    public void AddChild(SchemaElement element)
    {
        this.TypeName = $"List<{element.TypeName}>";
        this.children.Add(element);
    }

    public override void Validate()
    {
        if (this.children.Count > 1)
        {
            SchemaElement firstChild = this.children[0];
            for (int i = 1; i < this.children.Count; ++i)
            {
                if (firstChild != this.children[i])
                {
                    throw new ConfiguratorDotNetException($"Children of List '{this.XPath}' have differing schemas.");
                }
            }
        }
        
        foreach (var item in this.children)
        {
            item.Validate();
        }
    }

    public override bool Equals(SchemaElement? other)
    {
        if (other is not ListSchemaElement list)
        {
            return false;
        }

        bool thisHasChildren = this.children.Count > 0;
        bool otherHasChildren = list.children.Count > 0;

        if (thisHasChildren != otherHasChildren)
        {
            return false;
        }

        if (!thisHasChildren)
        {
            return true;
        }

        return this.children[0] == list.children[0];
    }

    public override int GetHashCode()
    {
        if (this.children.Count > 0)
        {
            return this.children[0].GetHashCode() ^ "List".GetHashCode();
        }

        return -1;
    }
}
