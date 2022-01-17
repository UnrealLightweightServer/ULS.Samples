// See https://aka.ms/new-console-template for more information
using ULSCompiler;

Console.WriteLine("Hello, World!");

Compiler compiler = new Compiler(); 
compiler.Compile(args);

Console.WriteLine("Bye");