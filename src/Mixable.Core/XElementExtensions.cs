using System.Text;

namespace Mixable.Schema;

/// <summary>
/// Helpers for XElement.
/// </summary>
public static class XElementExtensions
{
    /// <summary>
    /// Returns all direct child elements of the given node that match the given tag name.
    /// </summary>
    public static IEnumerable<XElement> GetChildren(this XElement element, XName tagFilter)
    {
        return element.Elements().Where(x => x.Name == tagFilter);
    }

    /// <summary>
    /// Returns all child nodes of the given element.
    /// </summary>
    public static IEnumerable<XElement> GetChildren(this XElement element)
    {
        return element.Elements();
    }

    /// <summary>
    /// Returns all child nodes of the given element, with the exception of those within
    /// the <see cref="Constants.XMLNamespace"/> namespace.
    /// </summary>
    public static IEnumerable<XElement> GetFilteredChildren(this XElement element)
    {
        return element
            .GetChildren()
            .Where(x => x.Name.NamespaceName != Constants.XMLNamespace); // ignore our metadata namespace.
    }

    /// <summary>
    /// Returns a logical XPath for the given element.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="select">A selector, to modify the component parts.</param>
    /// <param name="where">A fitler, to ignore certain component parts.</param>
    public static string GetDocumentPath(
        this XElement element,
        Func<XName, string>? select = null,
        Func<XName, bool>? where = null)
    {
        static void Recurse(XElement? element, StringBuilder sb, Func<XName, string> select, Func<XName, bool> where)
        {
            if (element is not null)
            {
                Recurse(element.Parent, sb, select, where);

                if (where(element.Name))
                {
                    sb.Append('/');
                    sb.Append(select(element.Name));
                }
            }
        }

        StringBuilder sb = new();

        Recurse(
            element,
            sb,
            select ?? (x => x.ToString()),
            where ?? (x => true));

        return sb.ToString();
    }

    /// <summary>
    /// Gets the document path using only local tag names.
    /// </summary>
    public static string GetLocalDocumentPath(this XElement element)
    {
        return element.GetDocumentPath(select: x => x.LocalName);
    }
}