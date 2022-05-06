namespace ConfiguratorDotNet.Runtime;

public class DoubleScalarParser : IScalarParser
{
    public bool CanParse(string value)
    {
        return double.TryParse(value.Trim(), out _);
    }
}