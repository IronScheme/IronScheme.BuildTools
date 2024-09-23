using Mono.Cecil;
using System.Text.RegularExpressions;

if (args.Length == 0)
{
    Console.WriteLine("Usage: NamespaceRenamer [-r] assembly (src=tgt)+ ");
    return 1;
}

var refs = false;
if (args.Contains("-r", StringComparer.InvariantCultureIgnoreCase))
{
    refs = true;
    args = Array.FindAll(args, x => !x.Equals("-r", StringComparison.InvariantCultureIgnoreCase));
}

var tx = new Regex("^(?<src>.+)=(?<tgt>.+)$", RegexOptions.Compiled);

var assname = args[0];

var renames = new List<Transform>();
var except = new List<string>();

foreach (var arg in args)
{
    var m = tx.Match(arg);
    if (m.Success)
    {
        renames.Add(new Transform
        {
            Source = m.Groups["src"].Value,
            Target = m.Groups["tgt"].Value,
        });
    }
    else
    {
        except.Add(arg);
    }
}

var hasPdb = File.Exists(Path.ChangeExtension(assname, "pdb"));
var ass = AssemblyDefinition.ReadAssembly(assname, new ReaderParameters { ReadSymbols = hasPdb, InMemory = true});

if (!refs)
{
    Console.WriteLine("Renaming namespaces in {0}", ass.FullName);
    Rename(ass.MainModule.Types);
}
else
{
    Console.WriteLine("Renaming imported namespaces in {0}", ass.FullName);
    Rename(ass.MainModule.GetTypeReferences());
}

Console.WriteLine("Saving assembly: {0}", assname);
ass.Write(assname, new WriterParameters { WriteSymbols = hasPdb });

return 0;

void Rename(IEnumerable<TypeReference> types)
{
    foreach (var t in types)
    {
        if (!except.Contains(t.Namespace))
        {
            foreach (var r in renames)
            {
                if (t.Namespace.StartsWith(r.Source))
                {
                    var oldtn = t.FullName;
                    t.Namespace = t.Namespace.Replace(r.Source, r.Target);
                    Console.WriteLine("{0} -> {1}", oldtn, t.FullName);
                }
            }
        }
    }
}

class Transform
{
    public string Source, Target;
}
