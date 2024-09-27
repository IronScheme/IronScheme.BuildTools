using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Tasks;
using System.Reflection;

namespace IronScheme.Build
{
    public class ILAsm : ToolTask
    {
        /*
Usage: ilasm [Options] <sourcefile> [Options]

Options:
/NOLOGO         Don't type the logo
/QUIET          Don't report assembly progress
/NOAUTOINHERIT  Disable inheriting from System.Object by default
/DLL            Compile to .dll
/EXE            Compile to .exe (default)
/PDB            Create the PDB file without enabling debug info tracking
/APPCONTAINER   Create an AppContainer exe or dll
/DEBUG          Disable JIT optimization, create PDB file, use sequence points from PDB
/DEBUG=IMPL     Disable JIT optimization, create PDB file, use implicit sequence points
/DEBUG=OPT      Enable JIT optimization, create PDB file, use implicit sequence points
/OPTIMIZE       Optimize long instructions to short
/FOLD           Fold the identical method bodies into one
/CLOCK          Measure and report compilation times
/RESOURCE=<res_file>    Link the specified resource file (*.res)
                        into resulting .exe or .dll
/OUTPUT=<targetfile>    Compile to file with specified name
                        (user must provide extension, if any)
/KEY=<keyfile>      Compile with strong signature
                        (<keyfile> contains private key)
/KEY=@<keysource>   Compile with strong signature
                        (<keysource> is the private key source name)
/INCLUDE=<path>     Set path to search for #include'd files
/SUBSYSTEM=<int>    Set Subsystem value in the NT Optional header
/SSVER=<int>.<int>  Set Subsystem version number in the NT Optional header
/FLAGS=<int>        Set CLR ImageFlags value in the CLR header
/ALIGNMENT=<int>    Set FileAlignment value in the NT Optional header
/BASE=<int>     Set ImageBase value in the NT Optional header (max 2GB for 32-bit images)
/STACK=<int>    Set SizeOfStackReserve value in the NT Optional header
/MDV=<version_string>   Set Metadata version string
/MSV=<int>.<int>   Set Metadata stream version (<major>.<minor>)
/PE64           Create a 64bit image (PE32+)
/HIGHENTROPYVA  Set High Entropy Virtual Address capable PE32+ images (default for /APPCONTAINER)
/NOCORSTUB      Suppress generation of CORExeMain stub
/STRIPRELOC     Indicate that no base relocations are needed
/ITANIUM        Target processor: Intel Itanium
/X64            Target processor: 64bit AMD processor
/ARM            Target processor: ARM (AArch32) processor
/ARM64          Target processor: ARM64 (AArch64) processor
/32BITPREFERRED Create a 32BitPreferred image (PE32)
/ENC=<file>     Create Edit-and-Continue deltas from specified source file

Key may be '-' or '/'
Options are recognized by first 3 characters (except ARM/ARM64)
Default source file extension is .il

         */

        [Required]
        public ITaskItem[] InputFiles { get; set; }

        [Output]
        [Required]
        public ITaskItem OutputFile { get; set; }

        public string MetadataVersion { get; set; }

        protected override string ToolName => "ilasm";

        protected override string GenerateFullPathToTool()
        {
            var path = TryFindToolPath();
            if (path is null)
            {
                Log.LogError($"{ToolName} not found");
            }
            else
            {
                //Console.WriteLine($"{ToolName} found at '{path}'");
            }
            return path;
        }

        private string TryFindToolPath()
        {
            var fxpath = ToolLocationHelper.GetPathToDotNetFrameworkFile(ToolName, TargetDotNetFrameworkVersion.Latest);

            if (File.Exists(fxpath))
            {
                return fxpath;
            }

            fxpath = ToolLocationHelper.GetPathToDotNetFrameworkFile(Path.ChangeExtension(ToolName, ".exe"), TargetDotNetFrameworkVersion.Latest);

            if (File.Exists(fxpath))
            {
                return fxpath;
            }

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            fxpath = Path.Combine(path, ToolName);

            if (File.Exists(fxpath))
            {
                return fxpath;
            }

            fxpath = Path.ChangeExtension(fxpath, ".exe");

            if (File.Exists(fxpath))
            {
                return fxpath;
            }

            return null;
        }

        protected override string GenerateCommandLineCommands()
        {
            var commandLine = new CommandLineBuilderExtension();

            commandLine.AppendSwitchIfNotNull("-out=", OutputFile);
            commandLine.AppendSwitch("-nologo");
            commandLine.AppendSwitch("-dll");
            commandLine.AppendSwitch("-quiet");

            commandLine.AppendSwitchIfNotNull("-mdv=", MetadataVersion);

            foreach (var input in InputFiles)
            {
                commandLine.AppendFileNameIfNotNull(input);
            }

            return commandLine.ToString();
        }
    }
}
