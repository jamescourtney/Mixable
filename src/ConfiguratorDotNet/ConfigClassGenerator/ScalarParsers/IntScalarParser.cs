namespace ConfiguratorDotNet.Runtime;

public class IntScalarParser : IScalarParser
{
    public bool CanParse(string value)
    {
        return int.TryParse(value.Trim(), out _);
    }

    public string GetParseInvocation(string valueParameterName)
    {
        return $"int.Parse({valueParameterName}.Trim().ToLowerInvariant())";
    }
}