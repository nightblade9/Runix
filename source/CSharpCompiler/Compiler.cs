using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Runix.CSharpCompiler;

public class Compiler
{
    public IEnumerable<Diagnostic> LastFailures { get; private set; }
    public Assembly OutputAssembly { get; private set; }
    
    private MemoryStream _memoryStream;

    public bool Compile(string code)
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

        _memoryStream = new MemoryStream();
        // write IL code into memory
        EmitResult result = compilation.Emit(_memoryStream);

        if (!result.Success)
        {
            PopulateCompilerErrors(result);
            _memoryStream.Close();
            _memoryStream.Dispose();
            return false;
        }

        OutputCompiledAssembly();

        return true;
    }

    private void OutputCompiledAssembly()
    {
        // Call compile first, ofc
        if (_memoryStream == null)
        {
            throw new InvalidOperationException("Call Compile first!");
        }

        // load this 'virtual' DLL so that we can use
        _memoryStream.Seek(0, SeekOrigin.Begin);
        OutputAssembly = Assembly.Load(_memoryStream.ToArray());
        _memoryStream.Close();
        _memoryStream.Dispose();
    }
    
    private void PopulateCompilerErrors(EmitResult result)
    {
        // handle exceptions
        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
            diagnostic.IsWarningAsError ||
            diagnostic.Severity == DiagnosticSeverity.Error);

        LastFailures = failures;
    }
}