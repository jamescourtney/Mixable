using System.Xml;
using System.Xml.XPath;

namespace Mixable.Schema;

public enum MatchKind
{
    Subset = 0,
    Strict = 1,
}

/// <summary>
/// A generic element of an XML schema.
/// </summary>
public abstract class SchemaElement
{
    protected SchemaElement(XElement xElement)
    {
        this.XmlElement = xElement;
        this.Modifier = MetadataAttributes.Extract(xElement, null).Modifier;
    }

    /// <summary>
    /// The XML element associated with this schema element. Contents may change as merge operations
    /// are applied.
    /// </summary>
    public XElement XmlElement { get; }

    /// <summary>
    /// Indicates if this element has the Optional attribute.
    /// </summary>
    public NodeModifier Modifier { get; private set; }

    /// <summary>
    /// Merges the given element with the schema, assuming it passes validation.
    /// </summary>
    public bool MergeWith(
        XElement element,
        bool allowAbstract,
        IErrorCollector? collector,
        IAttributeValidator attributeValidator)
    {
        collector = new DeduplicatingErrorCollector(collector);

        if (!this.MatchesSchema(element, MatchKind.Subset, attributeValidator, collector))
        {
            return false;
        }

        this.MergeWithProtected(element, attributeValidator, collector);

        if (!allowAbstract)
        {
            foreach (var child in this.XmlElement.Descendants())
            {
                if (MetadataAttributes.Extract(child, null).Modifier == NodeModifier.Abstract)
                {
                    collector.Error(
                        "Abstract nodes are not permitted to remain after the final merge.",
                        child);
                }
            }
        }

        return !collector.HasErrors;
    }

    /// <summary>
    /// Recursively merges the given element into this one. Assumes that 
    /// <see cref="MatchesSchema(XElement, MatchKind, IAttributeValidator, IErrorCollector)"/>
    /// has been invoked.
    /// </summary>
    protected internal virtual void MergeWithProtected(
        XElement element,
        IAttributeValidator validator,
        IErrorCollector errorCollector)
    {
        MetadataAttributes attributes = validator.Validate(element, errorCollector);
        if (this.Modifier == NodeModifier.Final)
        {
            errorCollector.Error(
                $"Cannot override element with the '{nameof(NodeModifier.Final)}' option",
                element);
        }
        else if (this.Modifier != attributes.Modifier)
        {
            this.Modifier = attributes.Modifier;
            this.XmlElement.SetAttributeValue(Constants.Attributes.Flags, attributes.Modifier.ToString());
        }

        // no special handling for optional.
    }

    /// <summary>
    /// Recursively tests whether the given element matches the current schema.
    /// </summary>
    protected internal abstract bool MatchesSchema(
        XElement element,
        MatchKind matchKind,
        IAttributeValidator validator,
        IErrorCollector errorCollector);

    public abstract T Accept<T>(ISchemaVisitor<T> visitor);
}