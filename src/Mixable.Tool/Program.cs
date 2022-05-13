using Mixable.Schema;

namespace Mixable.Tool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string fileName = @"C:\git\Mixable\src\Tests\LiveTester\Base.mxml";

            MxmlFileProcessor processor = new MxmlFileProcessor(fileName, new ConsoleErrorCollector());

            processor.MergeXml();

            ISchemaVisitor[] visitors = new ISchemaVisitor[] { new CSharp.SchemaVisitor(enableFileOutput: true), new Python.SchemaVisitor() };

            processor.TryApplyVisitors(visitors);
        }
    }

    public class ConsoleErrorCollector : IErrorCollector
    {
        public bool HasErrors { get; private set; }

        public void Error(string message, string? path = null)
        {
            this.HasErrors = true;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{message}, {path}");
            Console.ResetColor();
        }

        public void Info(string message, string? path = null)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{message}, {path}");
            Console.ResetColor();
        }

        public void Warning(string message, string? path = null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{message}, {path}");
            Console.ResetColor();
        }
    }
}