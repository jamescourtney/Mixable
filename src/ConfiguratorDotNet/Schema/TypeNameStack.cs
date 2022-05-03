namespace ConfiguratorDotNet.Schema;

internal class TypeNameStack
{
    private readonly LinkedList<string> parts = new();

    public void Push(string value)
    {
        this.parts.AddLast(value);
    }

    public void Pop()
    {
        this.parts.RemoveLast();
    }

    public override string ToString()
    {
        return string.Join("_", this.parts);
    }
}