using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Task = Microsoft.Build.Utilities.Task;

namespace IronScheme.Build
{
    public class ReferenceRemover : Task
    {
        [Required]
        public ITaskItem Input { get; set; }

        [Required]
        public ITaskItem Target { get; set; }

        [Required]
        public string Include { get; set; }
        public string Exclude { get; set; }

        [Output]
        public ITaskItem Output { get; set; }

        public override bool Execute()
        {
            Log.LogMessage("Input: {0}", Input);
            var input = Input.ItemSpec;

            Log.LogMessage("Output: {0}", Output ?? Input);
            var output = Output?.ItemSpec ?? input;

            Log.LogMessage("Target: {0}", Target);
            var target = Target.ItemSpec;

            var hasPdb = File.Exists(Path.ChangeExtension(input, "pdb"));
            var ass = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { ReadSymbols = hasPdb, InMemory = input == output });

            var targetass = AssemblyDefinition.ReadAssembly(target);

            var match = new Regex(Include);
            var exclude = Exclude is null ? null : new Regex(Exclude);

            foreach (var r in ass.MainModule.AssemblyReferences)
            {
                if (match.IsMatch(r.FullName) && (exclude == null || !exclude.IsMatch(r.FullName)))
                {
                    Log.LogMessage("Redirecting reference: {0} to {1}", r.FullName, targetass.FullName);
                    r.Name = targetass.Name.Name;
                    r.Version = targetass.Name.Version;
                    r.PublicKeyToken = targetass.Name.PublicKeyToken;
                }
            }

            Log.LogMessage("Saving assembly: {0}", output);
            ass.Write(output, new WriterParameters { WriteSymbols = hasPdb });

            return true;
        }
    }
}
