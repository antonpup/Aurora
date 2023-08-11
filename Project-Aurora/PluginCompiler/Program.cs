using CSScriptLib;

var path = args[0];
CSScript.RoslynEvaluator.CompileAssemblyFromFile(path, path + ".dll");
