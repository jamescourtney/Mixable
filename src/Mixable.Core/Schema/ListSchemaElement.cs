namespace Mixable.Schema;

/// <summary>
/// Represents a list of elements with a shared schema.
/// </summary>
public class ListSchemaElement : SchemaElement
{
    public ListSchemaElement(
        XElement node,
        SchemaElement templateChild) : base(node)
    {
        this.Template = templateChild;
    }

    public SchemaElement Template { get; }

    public override T Accept<T>(ISchemaVisitor<T> visitor, IErrorCollector errorCollector)
    {
        return visitor.Accept(this, errorCollector);
    }

    protected internal override bool MatchesSchema(
        XElement element,
        MatchKind matchKind,
        IAttributeValidator validator,
        IErrorCollector errorCollector)
    {
        bool returnValue = true;

        if (validator.Validate(element, errorCollector).Modifier != NodeModifier.None)
        {
            errorCollector.Error($"List elements in override schemas may not specify the '{Constants.Attributes.Flags.LocalName}' attribute.", element);
        }

        foreach (XElement child in element.GetFilteredChildren())
        {
            if (child.Name != this.Template.XmlElement.Name)
            {
                errorCollector.Error($"Expected tag name: '{this.Template.XmlElement.Name}'. Got: '{child.Name}'.", child.GetDocumentPath());
                returnValue = false;
            }

            // Always use strict matching for lists. A subset of an item isn't good enough
            // here.
            returnValue &= this.Template.MatchesSchema(
                child,
                MatchKind.Strict,
                validator,
                errorCollector);
        }

        return returnValue;
    }

    protected override void MergeWithProtected(
        XElement element,
        bool allowAbstract,
        IAttributeValidator validator,
        IErrorCollector collector)
    {
        MetadataAttributes attrs = validator.Validate(element, collector);

        if (attrs.ListMergePolicy == ListMergePolicy.Replace)
        {
            this.XmlElement.RemoveNodes();
        }

        foreach (var child in element.GetFilteredChildren())
        {
            this.XmlElement.Add(child);
        }
    }
}
