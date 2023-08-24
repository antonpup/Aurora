using System;
using CSScripting;
using CSScriptLib;

if (args.Length == 0)
    return;

var path = args.JoinBy(" ");
Console.WriteLine("Compiling...\n" + path);
CSScript.RoslynEvaluator.CompileAssemblyFromFile(path, path + ".dll");
