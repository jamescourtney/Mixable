namespace ConfiguratorDotNet.Runtime;

public class DoubleScalarParser : IScalarParser
{
    public bool CanParse(string value)
    {
        return double.TryParse(value.Trim(), out _);
    }

    public string GetParseInvocation(string valueParameterName)
    {
        return $"double.Parse({valueParameterName}.Trim().ToLowerInvariant())";
    }
}