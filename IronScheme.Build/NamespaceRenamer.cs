using Microsoft.Build.Framework;
using Mono.Cecil;
using Task = Microsoft.Build.Utilities.Task;

namespace IronScheme.Build
{
    public class NamespaceRenamer : Task
    {
        [Required]
        public ITaskItem Input { get; set; }

        [Required]
        public ITaskItem[] Renames { get; set; } = [];

        public bool References { get; set; } = false;

        public string[] Except { get; set; } = [];

        [Output]
        public ITaskItem Output { get; set; }

        public override bool Execute()
        {
            Log.LogMessage("Input: {0}", Input);
            var input = Input.ItemSpec;

            Log.LogMessage("Output: {0}", Output ?? Input);
            var output = Output?.ItemSpec ?? input;

            Log.LogMessage("References: {0}", References);

            var hasPdb = File.Exists(Path.ChangeExtension(input, "pdb"));
            var ass = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { ReadSymbols = hasPdb, InMemory = input == output });

            if (!References)
            {
                Log.LogMessage("Renaming namespaces in {0}", ass.FullName);
                Rename(ass.MainModule.Types);
            }
            else
            {
                Log.LogMessage("Renaming imported namespaces in {0}", ass.FullName);
                Rename(ass.MainModule.GetTypeReferences());
            }

            Log.LogMessage("Saving assembly: {0}", output);
            ass.Write(output, new WriterParameters { WriteSymbols = hasPdb });

            return true;
        }

        void Rename(IEnumerable<TypeReference> types)
        {
            foreach (var t in types)
            {
                if (!Except.Contains(t.Namespace))
                {
                    foreach (var r in Renames)
                    {
                        var source = r.GetMetadata("Source");
                        var target = r.GetMetadata("Target");

                        if (t.Namespace.StartsWith(source))
                        {
                            var oldtn = t.FullName;
                            t.Namespace = t.Namespace.Replace(source, target);
                            Log.LogMessage("{0} -> {1}", oldtn, t.FullName);
                        }
                    }
                }
            }
        }
    }
}
