using System.Text.RegularExpressions;
using Mono.Cecil;

if (args.Length == 0)
{
    Console.WriteLine("Usage: RemoveReferences Assembly [Regex [ExcludeRegex] Target]");
    return 1;
}

var hasPdb = File.Exists(Path.ChangeExtension(args[0], "pdb"));
var ass = AssemblyDefinition.ReadAssembly(args[0], new ReaderParameters { ReadSymbols = hasPdb, InMemory = true });

if (args.Length == 1)
{
    foreach (var r in ass.MainModule.AssemblyReferences)
    {
        Console.WriteLine(r.FullName);
    }
    return 0;
}

var match = new Regex(args[1]);
Regex exclude = null;

if (args.Length == 4)
{
    exclude = new Regex(args[2]);
}

var target = AssemblyDefinition.ReadAssembly(exclude == null ? args[2] : args[3]);

foreach (var r in ass.MainModule.AssemblyReferences)
{
    if (match.IsMatch(r.FullName) && (exclude == null || !exclude.IsMatch(r.FullName)))
    {
        Console.WriteLine("Redirecting reference: {0} to {1}", r.FullName, target.FullName);
        r.Name = target.Name.Name;
        r.Version = target.Name.Version;
        r.PublicKeyToken = target.Name.PublicKeyToken;
    }
}

Console.WriteLine("Saving assembly: {0}", args[0]);
ass.Write(args[0], new WriterParameters { WriteSymbols = hasPdb });

return 0;