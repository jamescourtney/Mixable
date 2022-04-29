namespace Mixable.Schema;

/// <summary>
/// Enumerates and defines known scalar types.
/// </summary>
public class ScalarType
{
    public static ScalarType Int { get; } = new()
    {
        Parser = new IntScalarParser(),
        Type = WellKnownType.Int,
    };

    public static ScalarType Double { get; } = new()
    {
        Parser = new DoubleScalarParser(),
        Type = WellKnownType.Double,
    };

    public static ScalarType Bool { get; } = new()
    {
        Parser = new BoolScalarParser(),
        Type = WellKnownType.Bool,
    };

    public static ScalarType String { get; } = new()
    {
        Parser = new StringScalarParser(),
        Type = WellKnownType.String,
    };

    private static ScalarType[] PriorityOrder = new[] { Bool, Int, Double, String };

    private ScalarType()
    {
        this.Parser = null!;
    }

    public IScalarParser Parser { get; private init; }

    public WellKnownType Type { get; private init; }

    public static bool TryGetExplicitScalarType(WellKnownType wellKnownType, [NotNullWhen(true)] out ScalarType? type)
    {
        foreach (var item in PriorityOrder)
        {
            if (wellKnownType == item.Type)
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