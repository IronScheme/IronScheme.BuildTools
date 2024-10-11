using Microsoft.Build.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using Task = Microsoft.Build.Utilities.Task;

namespace IronScheme.Build
{
    public class PdbConvert : Task
    {
        [Required]
        public ITaskItem Input { get; set; }

        [Required]
        public string DebugType { get; set; }

        bool IncludeSource { get; set; }

        [Output]
        public ITaskItem Output { get; set; }

        private void LogMessage(string message, params object[] args) => Log.LogMessage($"[{nameof(PdbConvert)}] " + message, args);

        public override bool Execute()
        {
            LogMessage("Input: {0}", Input);
            var input = Input.ItemSpec;

            LogMessage("Output: {0}", Output ?? Input);
            var output = Output?.ItemSpec ?? input;

            var hasPdb = File.Exists(Path.ChangeExtension(input, "pdb"));

            if (!hasPdb)
            {
                Log.LogWarning("Skipping convert PDB as there is none for {0}", input);
                return true;
            }

            using var ass = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { ThrowIfSymbolsAreNotMatching = true, ReadSymbols = hasPdb, InMemory = input == output });

            LogMessage("Current symbol reader: {0}", ass.MainModule.SymbolReader.GetType().Name);

            var symwriter = GetWriterProvider();

            LogMessage("Target symbol reader: {0}", symwriter?.GetType().Name ?? "None");

            LogMessage("Saving assembly: {0}", output);
            ass.Write(output, new WriterParameters { WriteSymbols = hasPdb, SymbolWriterProvider = symwriter});

            return true;


            ISymbolWriterProvider GetWriterProvider(bool embedSource = false)
            {
                switch (DebugType.ToLowerInvariant())
                {
                    case "native":
                    case "full":
                    case "pdbonly":
                    case "windows":
                        return new NativePdbWriterProvider();
                    case "portable":
                        return new PortablePdbWriterProvider(embedSource);
                    case "embedded":
                        return new EmbeddedPortablePdbWriterProvider(embedSource);
                    default:
                        return null;
                }
            }
        }
    }
}
