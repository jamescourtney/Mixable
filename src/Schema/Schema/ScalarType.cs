namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Enumerates and defines known scalar types.
/// </summary>
public class ScalarType
{
    public static ScalarType Int { get; } = new()
    {
        Parser = new IntScalarParser(),
        TypeName = "int",
    };

    public static ScalarType Double { get; } = new()
    {
        Parser = new DoubleScalarParser(),
        TypeName = "double",
    };

    public static ScalarType Bool { get; } = new()
    {
        Parser = new BoolScalarParser(),
        TypeName = "bool",
    };

    public static ScalarType String { get; } = new()
    {
        Parser = new StringScalarParser(),
        TypeName = "string",
    };

    private static ScalarType[] PriorityOrder = new[] { Bool, Int, Double, String };

    private ScalarType()
    {
        this.Parser = null!;
        this.TypeName = null!;
    }

    public IScalarParser Parser { get; private init; }

    public string TypeName { get; private init; }

    public static bool TryGetExplicitScalarType(string explicitType, [NotNullWhen(true)] out ScalarType? type)
    {
        explicitType = explicitType.ToLowerInvariant().Trim();

        foreach (var item in PriorityOrder)
        {
            if (item.TypeName == explicitType)
            {
                type = item;
                return true;
            }
        }

        type = null;
        return false;
    }

    public static ScalarType GetInferredScalarType(string value)
    {
        foreach (var st in PriorityOrder)
        {
            if (st.Parser.CanParse(value))
            {
                return st;
            }
        }

        throw new Exception("Unexpected");
    }
}