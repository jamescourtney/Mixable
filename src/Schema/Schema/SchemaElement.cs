using System.Threading;

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
    private static readonly ThreadLocal<int> MergeWithDepth = new ThreadLocal<int>();

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
    public NodeModifier Modifier { get; protected set; }

    /// <summary>
    /// Merges the given element with the schema, assuming it passes validation.
    /// </summary>
    public bool MergeWith(
        XElement element,
        bool allowAbstract,
        IErrorCollector? collector,
        IAttributeValidator attributeValidator)
    {
        MergeWithDepth.Value++;

        try
        {
            collector = new DeduplicatingErrorCollector(collector);

            if (!this.MatchesSchema(element, MatchKind.Subset, attributeValidator, collector))
            {
                return false;
            }

            this.MergeWithProtected(element, allowAbstract, attributeValidator, collector);
        }
        finally
        {
            MergeWithDepth.Value--;
        }

        if (!allowAbstract && MergeWithDepth.Value == 0)
        {
            MixableInternal.Assert(this.XmlElement.Document is not null, "Document was null");

            foreach (var node in this.XmlElement.Document.Descendants())
            {
                if (MetadataAttributes.Extract(node, null).Modifier == NodeModifier.Abstract)
                {
                    collector.Error(
                        "Abstract nodes are not permitted to remain after the final merge.",
                        node);
                }
            }
        }

        return !collector.HasErrors;
    }

    /// <summary>
    /// Recursively merges the given element into this one. Assumes that 
    /// <see cref="MatchesSchema(XElement, MatchKind, IAttributeValidator, IErrorCollector)"/>
    /// succeeded.
    /// </summary>
    protected abstract void MergeWithProtected(
        XElement element,
        bool allowAbstract,
        IAttributeValidator validator,
        IErrorCollector errorCollector);

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