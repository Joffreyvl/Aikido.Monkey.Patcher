using Microsoft.CodeAnalysis;

namespace Aikido.Logging.Dapper.Extensions;
internal static class FileLinePositionSpanExtensions
{
    public static int GetInvocationIndex(this FileLinePositionSpan lineSpan, string value)
       => lineSpan.Span.ToString().IndexOf(value);
}
