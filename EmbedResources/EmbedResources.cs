using Mono.Cecil;

if (args.Length == 0)
{
    Console.WriteLine("EmbedResources Assembly (file[,name])+");
    return 1;
}

var hasPdb = File.Exists(Path.ChangeExtension(args[0], "pdb"));
var ass = AssemblyDefinition.ReadAssembly(args[0], new ReaderParameters { ReadSymbols = hasPdb, InMemory = true });
var resources = ass.MainModule.Resources;

foreach (var embed in args.Skip(1))
{
    var parts = embed.Split(',');
    var file = parts[0];
    var name = parts.Length > 1 ? parts[1] : Path.GetFileName(file);

    resources.Add(new EmbeddedResource(name, ManifestResourceAttributes.Public, File.OpenRead(file)));
}

Console.WriteLine("Saving assembly: {0}", args[0]);
ass.Write(args[0], new WriterParameters { WriteSymbols = hasPdb });

return 0;