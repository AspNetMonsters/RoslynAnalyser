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
            context.RegisterSyntaxNodeAction(AnalyseClass, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyseClass(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node as ClassDeclarationSyntax;
            var identifier = node.Identifier;
            if (identifier.ValueText.EndsWith("Controller"))
            {
                //is a controller, probably
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
    }
}
