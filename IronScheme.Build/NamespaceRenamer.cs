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

        private void LogMessage(string message, params object[] args) => Log.LogMessage($"[{nameof(NamespaceRenamer)}] " + message, args);

        public override bool Execute()
        {
            LogMessage("Input: {0}", Input);
            var input = Input.ItemSpec;

            LogMessage("Output: {0}", Output ?? Input);
            var output = Output?.ItemSpec ?? input;

            LogMessage("References: {0}", References);

            var hasPdb = File.Exists(Path.ChangeExtension(input, "pdb"));
            using var ass = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { ThrowIfSymbolsAreNotMatching = true, ReadSymbols = hasPdb, InMemory = input == output });

            if (!References)
            {
                LogMessage("Renaming namespaces in {0}", ass.FullName);
                Rename(ass.MainModule.Types);
            }
            else
            {
                LogMessage("Renaming imported namespaces in {0}", ass.FullName);
                Rename(ass.MainModule.GetTypeReferences());
            }

            LogMessage("Saving assembly: {0}", output);
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
                        var target = r.GetMetadata("TargetNS");

                        if (t.Namespace.StartsWith(source))
                        {
                            var oldtn = t.FullName;
                            t.Namespace = t.Namespace.Replace(source, target);
                            LogMessage("{0} -> {1}", oldtn, t.FullName);
                        }
                    }
                }
            }
        }
    }
}
