using System;
using CSScripting;
using CSScriptLib;

if (args.Length == 0)
    return;

var path = args.JoinBy(" ");
Console.WriteLine("Compiling...\n" + path);

try
{
    CSScript.RoslynEvaluator.CompileAssemblyFromFile(path, path + ".dll");
}
catch (Exception e)
{
    Console.Error.WriteLine(e.Message);

    Console.In.ReadLine();
    throw;
}
