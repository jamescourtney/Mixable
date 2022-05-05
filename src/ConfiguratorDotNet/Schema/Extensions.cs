using System.Linq;
using System.Text;

namespace ConfiguratorDotNet.Schema;

public static class Extensions
{
    public static IEnumerable<XElement> GetChildren(this XElement element, XName tagFilter)
    {
        return element.Elements().OfType<XElement>().Where(x => x.Name == tagFilter);
    }

    public static IEnumerable<XElement> GetChildren(this XElement element)
    {
        return element.Elements().OfType<XElement>();
    }

    public static IEnumerable<XElement> GetFilteredChildren(this XElement element)
    {
        return element
            .GetChildren()
            .Where(x => x.Name.NamespaceName != Constants.XMLNamespace); // ignore our metadata namespace.
    }

    public static string GetDocumentPath(this XElement element, Func<XName, string>? callback = null)
    {
        static void Recurse(XElement? element, StringBuilder sb, Func<XName, string> callback)
        {
            if (element is not null)
            {
                Recurse(element.Parent, sb, callback);
                sb.Append('/');
                sb.Append(callback(element.Name));
            }
        }

        StringBuilder sb = new();
        Recurse(element, sb, callback ?? (x => x.ToString()));

        return sb.ToString();
    }
}