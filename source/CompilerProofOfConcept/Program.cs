using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        const string FileName = "Content/code.txt";
        var code = File.ReadAllText(FileName);
        CompileAndRun(code);
    }

    private static void CompileAndRun(string code)
    {
        // From: https://stackoverflow.com/a/29417053/8641842
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        // define other necessary objects for compilation
        string outputAssemblyName = Path.GetRandomFileName();

        // Load appropriate DLLs. Take a look at anywhere you have mscorlib.dll to see what else is there.
        // You might be surprised what you need to load; all this is necessary for System.Console.WriteLine(...).
        MetadataReference[] references = new MetadataReference[]
        {
            // Can't swap this out for mscorlib or System. Should be equivalent.
            MetadataReference.CreateFromFile(typeof(System.Object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Console").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
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
                ShowCompilerErrors(result);
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

    private static void ShowCompilerErrors(EmitResult result)
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
}