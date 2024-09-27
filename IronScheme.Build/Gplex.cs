using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Tasks;
using System.Reflection;

namespace IronScheme.Build
{
    public class Gplex : ToolTask
    {
        /*
GPLEX: Usage
gplex [options] filename
default filename extension is ".lex"
  options:  /babel           -- create extra interface for Managed Babel scanner
            /caseInsensitive -- create a case-insensitive automaton
            /check           -- create automaton but do not create output file
            /codePage:NN     -- default codepage NN if no unicode prefix (BOM)
            /codePageHelp    -- display codepage help
            /classes         -- use character equivalence classes
            /errorsToConsole -- legacy error-messages (not MSBUILD friendly)
            /frame:path      -- use "path" as frame file
            /help            -- display this usage message
            /info            -- scanner has header comment (on by default)
            /listing         -- emit listing even if no errors
            /noCompress      -- do not compress scanner tables
            /noCompressMap   -- do not compress character map
            /noCompressNext  -- do not compress nextstate tables
            /noFiles         -- no file input buffers, string input only
            /noMinimize      -- do not minimize the states of the dfsa
            /noParser        -- create stand-alone scanner
            /noPersistBuffer -- do not retain input buffer throughout processing
            /noEmbedBuffers  -- write buffers to separate GplexBuffers file
            /out:path        -- send output to filename "path"
            /out:-           -- send output to Console.Out
            /parseOnly       -- syntax check only, do not create automaton
            /stack           -- enable built-in stacking of start states
            /squeeze         -- sacrifice speed for small size
            /summary         -- emit statistics to list file
            /unicode         -- generate a unicode enabled scanner
            /verbose         -- chatter on about progress
            /version         -- give version information for GPLEX
         */

        [Required]
        public ITaskItem InputFile { get; set; }

        [Output]
        public ITaskItem OutputFile { get; set; }

        public ITaskItem Frame { get; set; }

        public bool Stack { get; set; }
        public bool Version {  get; set; }

        protected override string ToolName => "gplex";

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
            Log.LogError("Cannot find gplex exe: {0}", fp);
            return fp;
        }

        protected override string GenerateCommandLineCommands()
        {
            OutputFile ??= new TaskItem(InputFile.ItemSpec + ".cs");

            var commandLine = new CommandLineBuilderExtension();

            commandLine.AppendSwitchIfNotNull("/out:", OutputFile);
            
            commandLine.AppendSwitchIfNotNull("/frame:", Frame);
            if (Stack) commandLine.AppendSwitch("/stack");
            if (Version) commandLine.AppendSwitch("/version");

            commandLine.AppendFileNameIfNotNull(InputFile);

            return commandLine.ToString();
        }
    }
}
