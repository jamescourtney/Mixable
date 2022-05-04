﻿namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Represents a map, where each member has a different name.
/// </summary>
public class MapSchemaElement : SchemaElement
{
    private readonly Dictionary<XName, SchemaElement> children = new();

    public MapSchemaElement(SchemaElement? parent, XElement node) 
        : base(parent, node)
    {
    }

    public IEnumerable<KeyValuePair<XName, SchemaElement>> Children => this.children;

    public override T Accept<T>(ISchemaVisitor<T> visitor)
    {
        return visitor.Accept(this);
    }

    public void AddChild(string tagName, SchemaElement child)
    {
        this.children.Add(tagName, child);
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

            if (!map.children.TryGetValue(key, out SchemaElement? otherValue))
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

    protected internal override bool MatchesSchema(
        XElement element,
        IAttributeValidator validator,
        IErrorCollector errorCollector)
    {
        bool returnValue = true;
        validator.Validate(element, errorCollector);

        Dictionary<XName, XElement> map = element.GetFilteredChildren().ToDictionary(x => x.Name, x => x);

        foreach (var kvp in map)
        {
            if (this.children.TryGetValue(kvp.Key, out SchemaElement? value))
            {
                returnValue &= value.MatchesSchema(kvp.Value, validator, errorCollector);
            }
            else
            {
                errorCollector.Error(
                    "Merged file contains key not present in base file. Merging may not add new keys.",
                    kvp.Value.GetDocumentPath());

                returnValue = false;
            }
        }

        return returnValue;
    }

    protected internal override void MergeWithProtected(
        XElement element,
        IAttributeValidator validator,
        IErrorCollector collector)
    {
        Dictionary<XName, XElement> map = element.GetFilteredChildren().ToDictionary(x => x.Name, x => x);

        foreach (var kvp in map)
        {
            SchemaElement value = this.children[kvp.Key];
            value.MergeWithProtected(kvp.Value, validator, collector);
        }
    }
}
