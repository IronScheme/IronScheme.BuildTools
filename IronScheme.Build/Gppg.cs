using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Tasks;
using System.Reflection;

namespace IronScheme.Build
{
    public class Gppg : ToolTask
    {
        /*
Usage gppg [options] filename

/babel          Generate class compatible with Managed Babel
/conflicts      Emit "conflicts" file with full conflict details
/csTokenFile    Emit tokens to separate C# file {basename}Tokens.cs
/defines        Emit "tokens" file with token name list
/errorsToConsole  Produce legacy console messages (not MSBUILD friendly)
/gplex          Generate scanner base class for GPLEX
/help           Display this help message
/line-filename:name Point #line markers at file "name"
/listing        Emit listing file, even if no errors
/no-info        Do not write extra information to parser header comment
/no-filename    Do not write the filename in the parser output file
/no-lines       Suppress the generation of #line directives
/noThrowOnError Do not exit without an error message
/out:name       Name the parser output "name"
/report         Write *.report.html file with LALR(1) parsing states
/verbose        Display extra information to console and in reports
/version        Display version information
         */

        [Required]
        public ITaskItem InputFile { get; set; }

        [Output]
        public ITaskItem OutputFile { get; set; }

        public bool Gplex { get; set; }
        public bool NoLines { get; set; }
        public bool Version {  get; set; }

        protected override string ToolName => "gppg";

        protected override string GenerateFullPathToTool()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fp = Path.Combine(path, $"{ToolName}.exe");
            if (File.Exists(fp))
            {
                return fp;
            }
            fp = Path.Combine(path, ToolName);
            if (File.Exists(fp))
            {
                return fp;
            }
            Log.LogError("Cannot find gppg exe: {0}", fp);
            return fp;
        }

        protected override string GenerateCommandLineCommands()
        {
            OutputFile ??= new TaskItem(InputFile.ItemSpec + ".cs");

            var commandLine = new CommandLineBuilderExtension();

            commandLine.AppendSwitchIfNotNull("/out:", OutputFile);

            if (Gplex) commandLine.AppendSwitch("/gplex");
            if (NoLines) commandLine.AppendSwitch("/no-lines");
            if (Version) commandLine.AppendSwitch("/version");

            commandLine.AppendFileNameIfNotNull(InputFile);

            return commandLine.ToString();
        }
    }
}
