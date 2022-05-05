﻿namespace ConfiguratorDotNet.Schema;

public record struct MetadataAttributes
{
    public string? TypeName { get; init; }

    public ListMergePolicy? ListMergePolicy { get; init; }

    public bool? List { get; init; }

    internal static MetadataAttributes Extract(XElement element)
    {
        return new MetadataAttributes
        {
            TypeName = element.Attribute(Constants.Attributes.Type)?.Value,

            List = element.Attribute(Constants.Attributes.List)?.Value?.ToLowerInvariant() switch
            {
                "true" => true,
                "false" => false,
                _ => null,
            },

            ListMergePolicy = element.Attribute(Constants.Attributes.ListMerge)?.Value?.ToLowerInvariant() switch
            {
                string s => (ListMergePolicy)Enum.Parse(typeof(ListMergePolicy), s, ignoreCase: true),
                _ => null,
            }
        };
    }
}