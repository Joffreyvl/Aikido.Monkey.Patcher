﻿using Aikido.Logging.Dapper.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Aikido.Logging.Dapper;

[Generator]
public class QueryInterceptorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: (syntaxNode, _) => IsQueryAsyncInvocation(syntaxNode),
            transform: (syntaxContext, _) => GetInvocationWithLocation(syntaxContext))
            .Where(invocationInfo => invocationInfo != null);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(
            compilation,
            (context, source) => Execute(context, source.Right)
            );
    }

    private void Execute(SourceProductionContext context, ImmutableArray<(InvocationExpressionSyntax, Location)?> invocations)
    {
        //var @namespace = "Aikido.Logging.Dapper";
        var @namespace = "TestTool";

        var interceptsLocationAttributes = GenerateInterceptsLocationAttributes(invocations);

        var generatedSource = $@"
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace {@namespace}
{{
    internal static class QueryLoggerInterceptor
    {{
        private static ILogger? logger;

        public static void ConfigureLogger(ILogger logger)
        {{
            ArgumentNullException.ThrowIfNull(logger);
            QueryLoggerInterceptor.logger = logger;
        }}

{interceptsLocationAttributes}
        public static Task QueryAsync(this SqlConnection sqlConnection, string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {{
            logger?.LogInformation(""Query: {{query}}"", sql);
            return sqlConnection.QueryAsync(sql, param, transaction, commandTimeout, commandType);
        }}
    }}
}}
";

        context.AddSource("QueryInterceptor.g.cs", SourceText.From(generatedSource, Encoding.UTF8));
    }

    private static string GenerateInterceptsLocationAttributes(ImmutableArray<(InvocationExpressionSyntax, Location)?> invocations)
    {
        var interceptsLocationBuilder = new StringBuilder();

        foreach (var invocationInfo in invocations)
        {
            if (invocationInfo is null)
                continue;

            var (invocation, location) = invocationInfo.Value;

            if (location.SourceTree?.FilePath is null)
                continue;

            var filePath = location.SourceTree?.FilePath;
            var lineSpan = location.GetLineSpan();
            var lineNumber = lineSpan.StartLinePosition.Line + 1;
            var invocationIndex = lineSpan.GetInvocationIndex(".QueryAsync") + 1;

            interceptsLocationBuilder.AppendLine($@"        [InterceptsLocation(""{filePath}"", {lineNumber}, {invocationIndex})]");
        }

        return interceptsLocationBuilder.ToString();
    }

    private static bool IsQueryAsyncInvocation(SyntaxNode node)
    {
        if (node is InvocationExpressionSyntax invocation)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            return memberAccess?.Name.Identifier.Text == "QueryAsync";
        }
        return false;
    }

    private static (InvocationExpressionSyntax, Location)? GetInvocationWithLocation(GeneratorSyntaxContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var location = invocation.GetLocation();
        return (invocation, location);
    }
}