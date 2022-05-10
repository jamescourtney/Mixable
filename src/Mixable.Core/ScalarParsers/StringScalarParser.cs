namespace Mixable.Schema;

public class StringScalarParser : IScalarParser
{
    public bool CanParse(string value)
    {
        return value is not null;
    }
}