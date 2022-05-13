using System.Xml.XPath;

namespace Mixable.Schema.Metadata;

internal static class MetadataParseHelpers
{
    public static string? ParseFilePath(this XElement element, string relativePath, string path)
    {
        string? value = element.ParseOptionalString(path, null);
        if (value is null)
        {
            return null;
        }

        return System.IO.Path.Combine(relativePath, value);
    }

    public static string? ParseOptionalString(
        this XElement element,
        string path,
        string? defaultValue)
    {
        return element.XPathSelectElement(path)?.Value ?? defaultValue;
    }

    public static bool ParseOptionalBool(
        this XElement element,
        string path,
        IErrorCollector errorCollector,
        bool defaultValue)
    {
        string? value = element.ParseOptionalString(path, null);
        if (value is null)
        {
            return defaultValue;
        }

        return InterpretBool(value, element, path, errorCollector);
    }

    private static bool InterpretBool(string value, XElement element, string path, IErrorCollector errorCollector)
    {
        string originalValue = value;

        value = value.Trim().ToLowerInvariant();
        if (value == "true")
        {
            return true;
        }
        else if (value == "false")
        {
            return false;
        }
        else
        {
            errorCollector.Error($"Unable to parse '{originalValue}' as a boolean value.", element.GetLocalDocumentPath() + "/" + path);
            return false;
        }
    }
}