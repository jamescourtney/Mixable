﻿namespace ConfiguratorDotNet.Schema;

internal class XmlMetadata
{
    private string? namespaceName;
    private string? baseFileName;
    private string? outputXmlName;
    private bool? generateCSharp;

    public XmlMetadata(XElement root)
    {
        XElement? metadataElement = root
            .GetChildren()
            .SingleOrDefault(x => x.Name == Constants.Metadata.RootTagName);

        if (metadataElement is null)
        {
            throw new ConfiguratorDotNetException("Missing <Metadata> element.");
        }

        Dictionary<XName, string> children = metadataElement.GetChildren().ToDictionary(x => x.Name, x => x.Value);

        children.TryGetValue(Constants.Metadata.NamespaceTagName, out this.namespaceName);
        children.TryGetValue(Constants.Metadata.OutputXmlFileTagName, out this.outputXmlName);
        children.TryGetValue(Constants.Metadata.BaseFileName, out this.baseFileName);

        if (children.TryGetValue(Constants.Metadata.GenerateCSharptagName, out string? generateCSharp))
        {
            this.generateCSharp = generateCSharp.Trim().ToLowerInvariant() switch
            {
                "true" => true,
                "false" => false,
                _ => null,
            };
        }
    }

    public bool ValidateAsTemplateFile([NotNullWhen(false)] out string? error)
    {
        if (string.IsNullOrEmpty(this.NamespaceName))
        {
            error = "Template XML files must include a Namespace.";
            return false;
        }

        if (!string.IsNullOrEmpty(this.BaseFileName))
        {
            error = "Template XML files may not include a base file name";
            return false;
        }

        error = null;
        return true;
    }

    public bool ValidateAsOverrideFile([NotNullWhen(false)] out string? error)
    {
        if (!string.IsNullOrEmpty(this.NamespaceName))
        {
            error = "Override files may not specify a namespace name.";
            return false;
        }

        if (string.IsNullOrEmpty(this.BaseFileName))
        {
            error = "Override files must include a base file name.";
            return false;
        }

        error = null;
        return true;
    }

    public string? NamespaceName => this.namespaceName;

    public string? BaseFileName => this.baseFileName;

    public string? OutputXmlFileName => this.outputXmlName;

    public bool? GenerateCSharp => this.generateCSharp;
}
