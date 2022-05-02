using System.Linq;

namespace ConfiguratorDotNet.Generator;

internal static class Extension
{
    public static IEnumerable<XElement> GetChildren(this XElement element)
    {
        return element.Elements().OfType<XElement>();
    }
}