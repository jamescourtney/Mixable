namespace ConfiguratorDotNet.Runtime;

public interface IScalarParser
{
    bool CanParse(string value);

    string GetParseInvocation(string valueParameterName);
}
