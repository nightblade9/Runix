using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Runix;

public static class Compiler
{
    public static IEnumerable<Diagnostic> s_LastFailures { get; private set; }
    private static MemoryStream s_ms;

    public static bool Compile(string code)
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

        s_ms = new MemoryStream();
        // write IL code into memory
        EmitResult result = compilation.Emit(s_ms);

        if (!result.Success)
        {
            PopulateCompilerErrors(result);
        }

        return result.Success;
    }

    public static object? Run()
    {
        // Call compile first, ofc
        if (s_ms == null)
        {
            throw new InvalidOperationException("Call Compile first");
        }

        // load this 'virtual' DLL so that we can use
        s_ms.Seek(0, SeekOrigin.Begin);
        Assembly assembly = Assembly.Load(s_ms.ToArray());
        s_ms.Close();
        s_ms.Dispose();

        // create instance of the desired class and call the desired function
        Type type = assembly.GetType("Testing.CustomClass");
        object obj = Activator.CreateInstance(type);
        var methodReturnValue = type.InvokeMember("Run",
            BindingFlags.Default | BindingFlags.InvokeMethod,
            null,
            obj,
            new object[] { "Hello World" });
        

        return methodReturnValue;
    }

    private static void PopulateCompilerErrors(EmitResult result)
    {
        // handle exceptions
        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
            diagnostic.IsWarningAsError ||
            diagnostic.Severity == DiagnosticSeverity.Error);

        s_LastFailures = failures;
    }
}