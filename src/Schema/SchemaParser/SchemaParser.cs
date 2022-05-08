namespace Mixable.Schema;

public class SchemaParser
{
    private readonly ISchemaElementParser[] elementParsers;

    public SchemaParser() : this(new NoOpErrorCollector())
    {
    }

    public SchemaParser(IErrorCollector errorCollector)
    {
        this.elementParsers = new ISchemaElementParser[]
        {
            new ScalarSchemaElementParser(),
            new ListSchemaElementParser(),
            new MapSchemaElementParser(),
        };

        this.ErrorCollector = errorCollector;
    }

    public SchemaParser(IEnumerable<ISchemaElementParser> elementParsers, IErrorCollector errorCollector)
    {
        this.elementParsers = elementParsers.ToArray();
        this.ErrorCollector = errorCollector;
    }

    public IErrorCollector ErrorCollector { get; init; } = new NoOpErrorCollector();

    public IAttributeValidator AttributeValidator { get; init; } = new BaseSchemaAttributeValidator();

    public bool TryParse(
        XDocument document,
        [NotNullWhen(true)] out SchemaElement? root)
    {
        root = null;

        if (document.Root is null)
        {
            this.ErrorCollector.Error("XML Document did not have a root element.");
            return false;
        }

        if (!DocumentMetadata.TryCreateFromXDocument(document, this.ErrorCollector, out _))
        {
            return false;
        }

        root = this.Parse(document.Root);

        if (this.ErrorCollector.HasErrors)
        {
            root = null;
            return false;
        }

        return true;
    }

    internal SchemaElement Parse(XElement xElement)
    {
        ISchemaElementParser? firstParser = null;
        MetadataAttributes attributes = this.AttributeValidator.Validate(xElement, this.ErrorCollector);

        foreach (var parser in this.elementParsers)
        {
            if (attributes.WellKnownType is not null)
            {
                if (!parser.SupportsType(attributes.WellKnownType.Value))
                {
                    // Can't handle the known type.
                    continue;
                }
            }
            else if (attributes.RawTypeName is not null)
            {
                // Type specified, but didn't parse out to a handler.
                if (!parser.SupportsUnparsableWellKnownTypes)
                {
                    continue;
                }
            }

            if (parser.CanParse(xElement, attributes))
            {
                if (firstParser is null)
                {
                    firstParser = parser;
                }
                else
                {
                    this.ErrorCollector.Warning("Two ISchemaElementParsers reported cability to parse a node.", xElement.GetDocumentPath());
                }
            }
        }

        if (firstParser is null)
        {
            if (attributes.RawTypeName is not null)
            {
                this.ErrorCollector.Error(
                    $"Mixable was unable to build a schema for the XML. This may be because the Mixable 'Type' attribute is set to '{attributes.RawTypeName}' and there is no handler registered for this type.",
                    xElement.GetDocumentPath());
            }
            else
            {
                this.ErrorCollector.Error(
                    $"Mixable was unable to build a schema for the XML. Consider adding the Mixable 'Type' attribute to tell Mixable how to interpet the schema. Suggestions are: {string.Join(", ", Enum.GetNames(typeof(WellKnownType)))}",
                    xElement.GetDocumentPath());
            }
            
            return new MapSchemaElement(xElement); // no children though.
        }
        else
        {
            return firstParser.Parse(xElement, this.AttributeValidator, this.ErrorCollector, this.Parse);
        }
    }
}
