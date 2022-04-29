namespace Mixable.Schema;

public class IntScalarParser : IScalarParser
{
    public bool CanParse(string value)
    {
        return int.TryParse(value.Trim(), out _);
    }
}