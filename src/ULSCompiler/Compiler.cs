using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ULSCompiler
{
    internal class AnalyzerLoader : IAnalyzerAssemblyLoader
    {
        string logFile = "D:\\Temp\\codegen_log_5.txt";

        private void Log(string text)
        {
            File.AppendAllText(logFile, text + Environment.NewLine);
        }

        public void AddDependencyLocation(string fullPath)
        {
            Log(" #### AddDependencyLocation: " + fullPath);
        }

        public Assembly LoadFromPath(string fullPath)
        {
            Log(" #### LoadFromPath: " + fullPath);
            return Assembly.LoadFrom(fullPath);
        }
    }

    internal class MyResolver : MetadataReferenceResolver
    {
        string logFile = "D:\\Temp\\codegen_log_5.txt";

        private void Log(string text)
        {
            File.AppendAllText(logFile, text + Environment.NewLine);
        }

        public override bool Equals(object? other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string? baseFilePath, MetadataReferenceProperties properties)
        {
            /*Log(" ##### reference: " + reference);
            Log(" ##### baseFilePath: " + baseFilePath);
            Log(" ##### properties.Kind: " + properties.Kind);*/
            var b = ImmutableArray.CreateBuilder<PortableExecutableReference>();
            b.Add(PortableExecutableReference.CreateFromFile(reference));
            return b.ToImmutableArray();
        }
    }

    internal class MyRewriter : CSharpSyntaxRewriter
    {
        string logFile = "D:\\Temp\\codegen_log_5.txt";

        private void Log(string text)
        {
            File.AppendAllText(logFile, text + Environment.NewLine);
        }

        public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            Log(" *** VisitExpressionStatement 1: " + node);
            var childs = node.ChildNodes();
            foreach (var cn in childs)
            {
                if (cn is AssignmentExpressionSyntax)
                {
                    BlockSyntax newNode = SyntaxFactory.Block()
                        .AddStatements(
                            node,
                            SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("Console"),
                                    SyntaxFactory.IdentifierName("WriteLine")))
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal("Hello World")))))))
                    );
                    Log(" *** VisitExpressionStatement 2: " + newNode);
                    return newNode;
                }
            }

            return base.VisitExpressionStatement(node);
        }
    }

    internal class Compiler
    {
        string logFile = "D:\\Temp\\codegen_log_5.txt";

        private void Log(string text)
        {
            File.AppendAllText(logFile, text + Environment.NewLine);
        }

        public void Compile(string[] args)
        {
            Log("Compile BEGIN");

            string basePath = Directory.GetCurrentDirectory();
            Log("basePath: " + basePath);

            /*for (int i = 0; i < args.Length; i++)
            {
                if (args[i].EndsWith(".rsp"))
                {
                    string[] lines = File.ReadAllLines(args[i].Substring(1));
                    foreach (var line in lines)
                    {
                        string[] lineArgs = line.Split(' ');
                        foreach (var item in lineArgs)
                        {
                            Log(item);
                        }
                    }
                }
            }*/
            
            var res = CSharpCommandLineParser.Default.Parse(args, basePath, null, null);
            List<SyntaxTree> trees = new List<SyntaxTree>();
            foreach (var srcs in res.SourceFiles)
            {
                var rw = new MyRewriter();

                var stree = CSharpSyntaxTree.ParseText(File.ReadAllText(srcs.Path), res.ParseOptions, basePath, res.Encoding);

                var newRoot = rw.Visit(stree.GetRoot());
                stree = stree.WithRootAndOptions(newRoot, stree.Options);
                trees.Add(stree);
            }

            var analyzerRefs = res.ResolveAnalyzerReferences(new AnalyzerLoader());

            var references = res.ResolveMetadataReferences(new MyResolver());
            
            CSharpCompilation compilation = CSharpCompilation.Create(
                res.CompilationName,
                trees,
                references,
                res.CompilationOptions
                );
            
            

            var comp_res = compilation.Emit(res.GetOutputFilePath(res.OutputFileName));
            Log("success: " + comp_res.Success);
            foreach (var dg in comp_res.Diagnostics)
            {
                Log("dg: " + dg);
            }

            /*for (int i = 0; i < args.Length; i++)
            {
                Log(args[i]);

                if (args[i].EndsWith(".rsp"))
                {
                    string[] lines = File.ReadAllLines(args[i].Substring(1));
                    Log("=============");
                    Log("RSP begin");
                    foreach (string line in lines)
                    {

                        string[] lineargs = line.Split(' ');
                        foreach (var arg in lineargs)
                        {
                            Log(arg);
                        }
                    }
                    Log("RSP end");
                    Log("=============");
                }
            }*/
            Log("Compile END");
        }
    }
}
