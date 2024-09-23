using Mono.Cecil;
using System.Text.RegularExpressions;

var refs = false;
if (args.Contains("-r"))
{
    refs = true;
    args = Array.FindAll(args, x => x != "-r");
}

var tx = new Regex("^(?<src>.+)=(?<tgt>.+)$", RegexOptions.Compiled);

var assname = args[0];
var snk = Directory.GetFiles(Path.GetDirectoryName(Path.GetFullPath(assname)), "*.snk").FirstOrDefault();

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

if (!refs && snk != null)
{
    using var sn = File.OpenRead(snk);
    ass.Write(assname, new WriterParameters { StrongNameKeyPair = new System.Reflection.StrongNameKeyPair(sn), WriteSymbols = hasPdb });
}
else
{
    ass.Write(assname, new WriterParameters { WriteSymbols = hasPdb });
}

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
