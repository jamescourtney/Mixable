namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Defines how lists may be merged when applying an override to a template.
/// </summary>
public enum ListMergePolicy
{
    /// <summary>
    /// The lists are concatenated.
    /// </summary>
    Concatenate = 0,

    /// <summary>
    /// The parent's content is replaced by the child.
    /// </summary>
    Replace = 1,
}
