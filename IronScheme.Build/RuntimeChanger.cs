using Microsoft.Build.Framework;
using Mono.Cecil;
using Task = Microsoft.Build.Utilities.Task;

namespace IronScheme.Build
{
    public class RuntimeChanger : Task
    {
        [Required]
        public ITaskItem Input { get; set; }

        public string Runtime { get; set; } = "v2";

        public string Required32Bit { get; set; }

        [Output]
        public ITaskItem Output { get; set; }

        public override bool Execute()
        {
            Log.LogMessage("Input: {0}", Input);
            var input = Input.ItemSpec;

            Log.LogMessage("Output: {0}", Output ?? Input);
            var output = Output?.ItemSpec ?? input;

            Log.LogMessage("Runtime: {0}", Runtime);
            Log.LogMessage("Required32Bit: {0}", Required32Bit);

            var hasPdb = File.Exists(Path.ChangeExtension(input, "pdb"));
            var ass = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { ReadSymbols = hasPdb, InMemory = input == output });

            var current = FindRuntime(ass);

            if (string.Equals(Required32Bit, "true", StringComparison.InvariantCultureIgnoreCase))
            {
                ass.MainModule.Attributes |= ModuleAttributes.Required32Bit;
            }

            if (string.Equals(Required32Bit, "false", StringComparison.InvariantCultureIgnoreCase))
            {
                ass.MainModule.Attributes &= ~ModuleAttributes.Required32Bit;
            }

            current.Name = "mscorlib";
            current.PublicKeyToken = [0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89];
            current.Version = new Version("2.0.0.0");

            if (Runtime == "v4")
            {
                ass.MainModule.Runtime = TargetRuntime.Net_4_0;
            }
            else
            {
                ass.MainModule.Runtime = TargetRuntime.Net_2_0;
            }

            ass.Name.Name = Path.GetFileNameWithoutExtension(output);
            ass.MainModule.Name = Path.GetFileName(output);

            Log.LogMessage("Saving assembly: {0}", output);
            ass.Write(output, new WriterParameters { WriteSymbols = hasPdb });

            return true;
        }

        AssemblyNameReference FindRuntime(AssemblyDefinition ass)
        {
            foreach (var r in ass.MainModule.AssemblyReferences)
            {
                if (r.FullName.StartsWith("System.Private.CoreLib"))
                {
                    return r;
                }
            }

            foreach (var r in ass.MainModule.AssemblyReferences)
            {
                if (r.FullName.StartsWith("mscorlib"))
                {
                    return r;
                }
            }

            return null;
        }
    }
}
