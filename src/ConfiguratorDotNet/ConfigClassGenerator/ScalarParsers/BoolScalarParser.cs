namespace ConfiguratorDotNet.Runtime;

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

    public string GetParseInvocation(string valueParameterName)
    {
        return $"bool.Parse({valueParameterName}.Trim().ToLowerInvariant())";
    }
}