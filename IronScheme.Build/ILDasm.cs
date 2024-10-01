using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace IronScheme.Build
{
    public class ILDasm : ToolTask
    {
        /*
Usage: ildasm [options] <file_name> [options]

Options for output redirection:
  /OUT=<file name>    Direct output to file rather than to GUI.
  /TEXT               Direct output to console window rather than to GUI.

  /HTML               Output in HTML format (valid with /OUT option only).
  /RTF                Output in rich text format (invalid with /TEXT option).
Options for GUI or file/console output (EXE and DLL files only):
  /BYTES              Show actual bytes (in hex) as instruction comments.
  /RAWEH              Show exception handling clauses in raw form.
  /TOKENS             Show metadata tokens of classes and members.
  /SOURCE             Show original source lines as comments.
  /LINENUM            Include references to original source lines.
  /VISIBILITY=<vis>[+<vis>...]    Only disassemble the items with specified
          visibility. (<vis> = PUB | PRI | FAM | ASM | FAA | FOA | PSC)
  /PUBONLY            Only disassemble the public items (same as /VIS=PUB).
  /QUOTEALLNAMES      Include all names into single quotes.
  /NOCA               Suppress output of custom attributes.
  /CAVERBAL           Output CA blobs in verbal form (default - in binary form).
  /NOBAR              Suppress disassembly progress bar window pop-up.

The following options are valid for file/console output only:
Options for EXE and DLL files:
  /UTF8               Use UTF-8 encoding for output (default - ANSI).
  /UNICODE            Use UNICODE encoding for output.
  /NOIL               Suppress IL assembler code output.
  /FORWARD            Use forward class declaration.
  /TYPELIST           Output full list of types (to preserve type ordering in round-trip).
  /PROJECT            Display .NET projection view if input is a .winmd file.
  /HEADERS            Include file headers information in the output.
  /ITEM=<class>[::<method>[(<sig>)]  Disassemble the specified item only

  /STATS              Include statistics on the image.
  /CLASSLIST          Include list of classes defined in the module.
  /ALL                Combination of /HEADER,/BYTES,/STATS,/CLASSLIST,/TOKENS

Options for EXE,DLL,OBJ and LIB files:
  /METADATA[=<specifier>] Show MetaData, where <specifier> is:
          MDHEADER    Show MetaData header information and sizes.
          HEX         Show more things in hex as well as words.
          CSV         Show the record counts and heap sizes.
          UNREX       Show unresolved externals.
          SCHEMA      Show the MetaData header and schema information.
          RAW         Show the raw MetaData tables.
          HEAPS       Show the raw heaps.
          VALIDATE    Validate the consistency of the metadata.
Options for LIB files only:
  /OBJECTFILE=<obj_file_name> Show MetaData of a single object file in library

         */

        [Required]
        public ITaskItem InputFile { get; set; }

        [Output]
        public ITaskItem OutputFile { get; set; }

        public bool Linenum { get; set; }

        protected override string ToolName => "ildasm";

        public override bool Execute()
        {
            //Debugger.Launch();
            return base.Execute();
        }

        string _nativeToolPath;

        protected override string GenerateFullPathToTool()
        {
            _nativeToolPath ??= TryFindToolPath();
            var path = _nativeToolPath;
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
            var fxpath = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile(ToolName, TargetDotNetFrameworkVersion.Latest);
            
            if (File.Exists(fxpath))
            {
                return fxpath;
            }

            fxpath = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile(Path.ChangeExtension(ToolName, ".exe"), TargetDotNetFrameworkVersion.Latest); 

            if (File.Exists(fxpath))
            {
                return fxpath;
            }

            addNoBar = false;

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

        bool addNoBar = true;

        protected override string GenerateCommandLineCommands()
        {
            _nativeToolPath ??= TryFindToolPath();

            var commandLine = new CommandLineBuilderExtension();

            commandLine.AppendSwitchIfNotNull("-out=", OutputFile);

            if (addNoBar) commandLine.AppendSwitch("-nobar");
            if (Linenum) commandLine.AppendSwitch("-linenum");

            commandLine.AppendFileNameIfNotNull(InputFile);
            
            return commandLine.ToString();
        }
    }
}
