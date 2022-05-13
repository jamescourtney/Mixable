using System.Text;

namespace Mixable.Core;

public class IndentedCodeWriter
{
    private int indentSize;

    private readonly string oneIdent;
    private readonly string blockStart;
    private readonly string blockEnd;

    public IndentedCodeWriter(string blockStart, string blockEnd, int indentSize)
    {
        this.blockStart = blockStart;
        this.blockEnd = blockEnd;
        this.oneIdent = new string(' ', indentSize);
    }

    public StringBuilder StringBuilder { get; } = new();

    public IDisposable WithBlock()
    {
        return new SimpleDisposable(this);
    }

    public void AppendLine(string line)
    {
        for (int i = 0; i < this.indentSize; ++i)
        {
            this.StringBuilder.Append(this.oneIdent);
        }

        this.StringBuilder.AppendLine(line);
    }

    private class SimpleDisposable : IDisposable
    {
        private readonly IndentedCodeWriter writer;

        public SimpleDisposable(IndentedCodeWriter writer)
        {
            this.writer = writer;
            writer.AppendLine(writer.blockStart);
            this.writer.indentSize++;
        }

        public void Dispose()
        {
            writer.indentSize--;
            writer.AppendLine(writer.blockEnd);
            writer.AppendLine(string.Empty);
        }
    }
}