using System.Runtime.CompilerServices;

namespace Mixable.Schema;

public static class MixableInternal
{
    [ExcludeFromCodeCoverage]
    public static void Assert(
        [DoesNotReturnIf(false)] bool condition,
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        if (!condition)
        {
            throw new MixableInternalException(message, memberName, fileName, lineNumber);
        }
    }
}

[ExcludeFromCodeCoverage]
public class MixableInternalException : Exception
{
    public MixableInternalException(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = -1) : base($"Mixable Internal Error! Message = '{message}'.File = '{System.IO.Path.GetFileName(fileName)}', Member = '{memberName}:{lineNumber}'")
    {
    }
}