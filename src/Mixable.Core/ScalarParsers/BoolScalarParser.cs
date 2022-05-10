namespace Mixable.Schema;

public class BoolScalarParser : IScalarParser
{
    public bool CanParse(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "true" or "false" => true,
            _ => false,
        };
    }
}