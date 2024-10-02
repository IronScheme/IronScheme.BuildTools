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

        private void LogMessage(string message, params object[] args) => Log.LogMessage($"[{nameof(EmbedResources)}] " + message, args);

        public override bool Execute()
        {
            LogMessage("Input: {0}", Input);
            var input = Input.ItemSpec;

            LogMessage("Output: {0}", Output ?? Input);
            var output = Output?.ItemSpec ?? input;

            var hasPdb = File.Exists(Path.ChangeExtension(input, "pdb"));
            using var ass = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { ReadSymbols = hasPdb, InMemory = input == output });
            var resources = ass.MainModule.Resources;

            foreach (var embed in Resources)
            {
                var file = embed.ItemSpec;
                var mname = embed.GetMetadata("Name");
                var name = string.IsNullOrEmpty(mname) ? Path.GetFileName(file) : mname;

                LogMessage("Embedding resource: {0} as {1}", file, name);
                resources.Add(new EmbeddedResource(name, ManifestResourceAttributes.Public, File.OpenRead(file)));
            }

            LogMessage("Saving assembly: {0}", output);
            ass.Write(output, new WriterParameters { WriteSymbols = hasPdb });

            return true;
        }
    }
}
