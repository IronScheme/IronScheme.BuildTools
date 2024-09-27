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

        [Output]
        public ITaskItem Output { get; set; }

        public override bool Execute()
        {
            Log.LogMessage("Input: {0}", Input);
            var input = Input.ItemSpec;

            Log.LogMessage("Output: {0}", Output ?? Input);
            var output = Output?.ItemSpec ?? input;

            var hasPdb = File.Exists(Path.ChangeExtension(input, "pdb"));
            var ass = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { ReadSymbols = hasPdb, InMemory = input == output });
            var resources = ass.MainModule.Resources;

            foreach (var embed in Resources)
            {
                var file = embed.ItemSpec;
                var name = embed.GetMetadata("Name") ?? Path.GetFileName(file);

                Log.LogMessage("Embedding resource: {0} as {1}", file, name);
                resources.Add(new EmbeddedResource(name, ManifestResourceAttributes.Public, File.OpenRead(file)));
            }

            Log.LogMessage("Saving assembly: {0}", output);
            ass.Write(output, new WriterParameters { WriteSymbols = hasPdb });

            return true;
        }
    }
}
