using System.Runtime.CompilerServices;

namespace Mixable.Schema;

internal static class MixableInternal
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
internal class MixableInternalException : Exception
{
    public MixableInternalException(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string fileName = "",
        [CallerLineNumber] int lineNumber = -1) : base($"Mixable Internal Error! Message = '{message}'.File = '{System.IO.Path.GetFileName(fileName)}', Member = '{memberName}:{lineNumber}'")
    {
    }
}