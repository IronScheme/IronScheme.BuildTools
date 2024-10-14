using System.Resources;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Task = Microsoft.Build.Utilities.Task;

namespace IronScheme.Build
{
    public class EmbedResources : Task
    {
        [Required]
        public ITaskItem Input { get; set; }

        [Required]
        public ITaskItem[] Resources { get; set; } = [];

        public bool AsNetModule { get; set; }

        [Output]
        public ITaskItem Output { get; set; }

        private void LogMessage(string message, params object[] args) => Log.LogMessage($"[{nameof(EmbedResources)}] " + message, args);

        public override bool Execute()
        {
            LogMessage("Input: {0}", Input);
            var input = Input.ItemSpec;

            LogMessage("Output: {0}", Output ?? Input);
            var output = Output?.ItemSpec ?? input;

            var hasPdb = File.Exists(Path.ChangeExtension(input, "pdb"));
            using var ass = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { ThrowIfSymbolsAreNotMatching = true, ReadSymbols = hasPdb, InMemory = input == output });

            if (AsNetModule)
            {
                foreach (var embed in Resources)
                {
                    var file = embed.ItemSpec;
                    var mname = embed.GetMetadata("Name");
                    var name = string.IsNullOrEmpty(mname) ? Path.GetFileName(file) : mname;

                    var modname = Path.ChangeExtension(file, "netmodule");

                    using var modass = AssemblyDefinition.ReadAssembly(file, new ReaderParameters { ReadSymbols = false, ReadingMode = ReadingMode.Immediate });
                    modass.MainModule.Name = name;
                    modass.MainModule.Kind = ModuleKind.NetModule;
                    modass.MainModule.Write(modname, new WriterParameters { WriteSymbols = false });

                    LogMessage("Adding module: {0} as {1}", file, name);

                    var newmod = ModuleDefinition.ReadModule(modname, new ReaderParameters { ReadSymbols = false, ReadingMode = ReadingMode.Immediate });
                    ass.Modules.Add(newmod);
                    //ass.MainModule.ModuleReferences.Add(newmod); // not really a ref is it....
                }
            }
            else
            {
                var resources = ass.MainModule.Resources;

                foreach (var embed in Resources)
                {
                    var file = embed.ItemSpec;
                    var mname = embed.GetMetadata("Name");
                    var name = string.IsNullOrEmpty(mname) ? Path.GetFileName(file) : mname;

                    LogMessage("Embedding resource: {0} as {1}", file, name);
                    using var fs = File.OpenRead(file);
                    var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    resources.Add(new EmbeddedResource(name, ManifestResourceAttributes.Public, ms));
                }
            }

            LogMessage("Saving assembly: {0}", output);
            ass.Write(output, new WriterParameters { WriteSymbols = hasPdb });

            return true;
        }
    }
}
