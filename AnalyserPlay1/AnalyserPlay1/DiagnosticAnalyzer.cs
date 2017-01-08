using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyserPlay1
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AnalyserPlay1Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AnalyserPlay1";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
                                                       Title,
                                                       MessageFormat,
                                                       Category,
                                                       DiagnosticSeverity.Warning,
                                                       isEnabledByDefault: true,
                                                       description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyseClass, SyntaxKind.ClassDeclaration);
            context.RegisterCodeBlockAction(AnalyseClass);
        }

        private static void AnalyseClass(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node as ClassDeclarationSyntax;
            var identifier = node.Identifier;
            if (identifier.ValueText.EndsWith("Controller"))
            {
                //is a controller
                // && !identifier.GetAnnotations("AllowAnonymous").Any()
                var descendant = node.DescendantNodes((_) => true);
                var attrbuteLists = node.DescendantNodes((_) => true).Where(x => x.Kind() == SyntaxKind.AttributeList);
                if (!attrbuteLists.Any() ||
                    !ContainsAuthorizationAttribute(attrbuteLists))
                {
                    var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), identifier);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool ContainsAuthorizationAttribute(IEnumerable<SyntaxNode> attributeList)
        {
            foreach(var item in attributeList)
            {
                if (ContainsAuthorizationAttribute((item as AttributeListSyntax).Attributes))
                    return true;
            }
            return false;
        }

        private static bool ContainsAuthorizationAttribute(SeparatedSyntaxList<AttributeSyntax> attributes)
        {
            if (attributes.Any(x => (x.Name as IdentifierNameSyntax).Identifier.ValueText == "AllowAnonymous"))
                return true;
            if (attributes.Any(x => (x.Name as IdentifierNameSyntax).Identifier.ValueText == "Authorize"))
                return true;
            return false;
        }
        private static void AnalyseClass(CodeBlockAnalysisContext context)
        {
            if (context.CodeBlock.Kind() == SyntaxKind.ClassDeclaration)
            {
                var diagnostic = Diagnostic.Create(Rule, context.CodeBlock.GetLocation(), "namy name");

                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
