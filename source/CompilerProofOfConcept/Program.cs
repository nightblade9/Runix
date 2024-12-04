using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

internal class Program
{
    private static void Main(string[] args)
    {
        const string FileName = "Content/code.txt";
        var code = File.ReadAllText(FileName);

        // From: https://stackoverflow.com/a/29417053/8641842
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        // define other necessary objects for compilation
        string outputAssemblyName = Path.GetRandomFileName();

        MetadataReference[] references = new MetadataReference[]
        {
            // Look at the assembly, not the actual class we're importing.
            // Also, Assembly.Load(...).Location doesn't seem to work, for some reason.
            MetadataReference.CreateFromFile(typeof(System.Object).Assembly.Location),
        };

        // analyse and generate IL code from syntax tree
        CSharpCompilation compilation = CSharpCompilation.Create(
            outputAssemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using (var ms = new MemoryStream())
        {
            // write IL code into memory
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                // handle exceptions
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => 
                    diagnostic.IsWarningAsError || 
                    diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (Diagnostic diagnostic in failures)
                {
                    Console.Error.WriteLine("Compiler error {0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                }
            }
            else
            {
                // load this 'virtual' DLL so that we can use
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                // create instance of the desired class and call the desired function
                Type type = assembly.GetType("CustomCode.AwesomeClass");
                object obj = Activator.CreateInstance(type);
                var methodReturnValue = type.InvokeMember("Run",
                    BindingFlags.Default | BindingFlags.InvokeMethod,
                    null,
                    obj,
                    new object[] { "Hello World" });
                
                Console.WriteLine($"The method returned {methodReturnValue ?? "nothing"}");
            }
        }
    }
}