using System;
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

        private void LogMessage(string message, params object[] args) => Log.LogMessage($"[{nameof(ReferenceRemover)}] " + message, args);

        public override bool Execute()
        {
            LogMessage("Input: {0}", Input);
            var input = Input.ItemSpec;

            LogMessage("Output: {0}", Output ?? Input);
            var output = Output?.ItemSpec ?? input;

            LogMessage("Target: {0}", Target);
            var target = Target.ItemSpec;

            var hasPdb = File.Exists(Path.ChangeExtension(input, "pdb"));
            using var ass = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { ReadSymbols = hasPdb, InMemory = input == output });

            using var targetass = AssemblyDefinition.ReadAssembly(target, new ReaderParameters { InMemory = true });

            var match = new Regex(Include);
            var exclude = Exclude is null ? null : new Regex(Exclude);

            foreach (var r in ass.MainModule.AssemblyReferences)
            {
                if (match.IsMatch(r.FullName) && (exclude == null || !exclude.IsMatch(r.FullName)))
                {
                    LogMessage("Redirecting reference: {0} to {1}", r.FullName, targetass.FullName);
                    r.Name = targetass.Name.Name;
                    r.Version = targetass.Name.Version;
                    r.PublicKeyToken = targetass.Name.PublicKeyToken;
                }
            }

            LogMessage("Saving assembly: {0}", output);
            ass.Write(output, new WriterParameters { WriteSymbols = hasPdb });

            return true;
        }
    }
}
