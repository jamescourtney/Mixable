namespace ConfiguratorDotNet.Generator;

internal static class Constants
{
    public const string XMLNamespace = "http://configurator.net";

    public static class Metadata
    {
        public static readonly XName RootTagName = XName.Get("Metadata", XMLNamespace);

        public static readonly XName BaseFileName = XName.Get("BaseFile", XMLNamespace);

        public static readonly XName OutputXmlFileTagName = XName.Get("OutputXmlFile", XMLNamespace);

        public static readonly XName GenerateCSharptagName = XName.Get("GenerateCSharp", XMLNamespace);

        public static readonly XName NamespaceTagName = XName.Get("NamespaceName", XMLNamespace);
    }

    public static class Structure
    {
        public static readonly XName TypeAttributeName = XName.Get("Type", XMLNamespace);

        public static readonly XName ListAttributeName = XName.Get("List", XMLNamespace);

        public static readonly XName ListMergeStrategyName = XName.Get("ListMerge", XMLNamespace);
    }
}