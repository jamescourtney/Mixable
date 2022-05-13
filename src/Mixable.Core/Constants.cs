namespace Mixable.Schema;

public static class Constants
{
    public const string XMLNamespace = "https://github.com/jamescourtney/mixable";

    public static class Tags
    {
        public static readonly XName MetdataTagName = XName.Get("Metadata", XMLNamespace);

        public static readonly XName ListTemplateTagName = XName.Get("ListItemTemplate", XMLNamespace);
    }

    public static class Attributes
    {
        public static readonly XName Flags = XName.Get("Flags", XMLNamespace);

        public static readonly XName Type = XName.Get("Type", XMLNamespace);

        public static readonly XName ListMerge = XName.Get("ListMerge", XMLNamespace);
    }
}