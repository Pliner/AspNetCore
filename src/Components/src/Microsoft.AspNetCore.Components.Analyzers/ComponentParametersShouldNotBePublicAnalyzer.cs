﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.AspNetCore.Components.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ComponentParametersShouldNotBePublicAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "BL9993";
        private const string Category = "Encapsulation";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ComponentParametersShouldNotBePublic_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ComponentParametersShouldNotBePublic_Format), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ComponentParametersShouldNotBePublic_Description), Resources.ResourceManager, typeof(Resources));
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            var declaration = (PropertyDeclarationSyntax)context.Node;

            var parameterAttribute = declaration.AttributeLists
                .SelectMany(list => list.Attributes)
                .Where(attr => semanticModel.GetTypeInfo(attr).Type?.ToDisplayString() == ComponentsApi.ParameterAttribute.FullTypeName)
                .FirstOrDefault();

            if (parameterAttribute != null && IsPublic(declaration))
            {
                var identifierText = declaration.Identifier.Text;
                if (!string.IsNullOrEmpty(identifierText))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule,
                        declaration.GetLocation(),
                        identifierText));
                }
            }
        }

        private static bool IsPublic(PropertyDeclarationSyntax declaration)
            => declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
    }
}
