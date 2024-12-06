using System.Reflection;

namespace Runix.CSharpCompiler;

public class CodeExecutor
{
    /// <summary>
    /// Runs the code associated to the assembly we presumably just compiled.
    /// Returns whatever the invoked method returns, if anything. (Else, null.)
    /// </summary>
    public object? Run(Assembly assembly, string classNameAndNamespace, string methodName, params object[]? methodArgs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(classNameAndNamespace);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        // create instance of the desired class and call the desired function
        Type type = assembly.GetType(classNameAndNamespace);
        if (type == null)
        {
            throw new ArgumentException($"Can't find class {classNameAndNamespace} in assembly {assembly.FullName}");
        }
        
        object obj = Activator.CreateInstance(type);
        var methodReturnValue = type.InvokeMember(methodName,
            BindingFlags.Default | BindingFlags.InvokeMethod,
            null, obj, methodArgs);

        return methodReturnValue;
    }
}
