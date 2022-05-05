namespace ConfiguratorDotNet.Schema;

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

        root = this.Parse(null, document.Root);
        return true;
    }

    private SchemaElement Parse(
        SchemaElement? parent,
        XElement xElement)
    {
        ISchemaElementParser? firstParser = null;
        MetadataAttributes attributes = this.AttributeValidator.Validate(xElement, this.ErrorCollector);

        foreach (var parser in this.elementParsers)
        {
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
            this.ErrorCollector.Error("No ISchemaElementParser was able to parse the given node.", xElement.GetDocumentPath());
            return new MapSchemaElement(parent, xElement); // no children though.
        }
        else
        {
            return firstParser.Parse(parent, xElement, this.AttributeValidator, this.ErrorCollector, this.Parse);
        }
    }
}
