using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Tasks;

namespace IronScheme.Build
{
    public class ILRepack : ToolTask
    {
        /*
Syntax: ILRepack.exe [Options] /out:<path> <path_to_primary> [<other_assemblies> ...]

 - /help              displays this help
 - @<path>.rsp        response file containing additional arguments, one per line
 - /log:<logfile>     enable logging to a file (default is disabled)
 - /verbose           more detailed logging

 - /out:<path>        target assembly path, symbol/config/doc files will be written here as well
 - <path_to_primary>  primary assembly, gives the name, version to the merged one
 - <other_assemblies> other assemblies to merge with the primary one
 - /wildcards         allows (and resolves) file wildcards (e.g. *.dll) in input assemblies

 - /lib:<path>        path(s) to search directories to resolve referenced assemblies
                      (can be specified multiple times).
                      If you get 'unable to resolve assembly' errors specify a path to a directory
                      where the assembly can be found.

 - /target:kind       target assembly kind [library|exe|winexe], default is same as primary assembly
 - /ver:M.X.Y.Z       target assembly version
 - /keyfile:<path>    keyfile to sign the output assembly
 - /keycontainer:<c>  key container
 - /delaysign         set the key, but don't sign the assembly

 - /internalize       make all types except in the first assembly 'internal'.
                      Types in the transitive closure of public API remain public.
 - /internalizeassembly:<path>
                      Internalize a specific assembly name (no extension).
                      May be specified more than once (one per assembly to internalize).
                      If specified, no need to also specify /internalize.
 - /internalize:<exclude_file>
                      Each line is either a regex/ full type name not to internalize
                      or an assembly name not to internalize (.dll extension optional)
 - /renameinternalized
                      rename each internalized type to a new unique name
 - /excludeinternalizeserializable
                      do not internalize types marked as Serializable

 - /allowdup:Type     keep duplicates of the specified type, may be specified more than once
 - /allowdup          if no other /allowdup arguments specified, allow all duplicate types
 - /union             merges types with identical names into one
 - /repackdrop:RepackDropAttribute
                      allows dropping members denoted by this attribute name when merging
 - /allowduplicateresources
                      allows to duplicate resources in output assembly (by default they're ignored)
 - /noRepackRes       do not add the resource 'ILRepack.List' with all merged assembly names

 - /copyattrs         copy assembly attributes (by default only the primary assembly attributes are copied)
 - /attr:<path>       take assembly attributes from the given assembly file
 - /allowMultiple     when copyattrs is specified, allows multiple attributes (if type allows)
 - /targetplatform:P  specify target platform (v1, v1.1, v2, v4 supported)
 - /keepotherversionreferences
                      take reference assembly version into account when removing references

 - /skipconfig        skips merging config files
 - /illink            merge IL Linker files
 - /xmldocs           merges XML documentation as well
 - /ndebug            disables symbol file generation (omit this if you want symbols and debug information)
 - /zeropekind        allows assemblies with Zero PeKind (but obviously only IL will get merged)
 - /index             stores file:line debug information as type/method attributes (requires PDB)

 - /parallel          use as many CPUs as possible to merge the assemblies
 - /pause             pause execution once completed (good for debugging)

 - /usefullpublickeyforreferences - NOT IMPLEMENTED
 - /align             - NOT IMPLEMENTED
 - /closed            - NOT IMPLEMENTED

Note: for compatibility purposes, all Options are case insensitive, and can be specified using '/', '-' or '--' prefix.

         */

        [Required]
        public ITaskItem[] Input { get; set; }

        [Output]
        [Required]
        public ITaskItem Output { get; set; }

        protected override string ToolName => "ilrepack";

        protected override string GenerateFullPathToTool()
        {
            return ToolName;
        }

        protected override string GenerateCommandLineCommands()
        {
            var commandLine = new CommandLineBuilderExtension();

            commandLine.AppendSwitchIfNotNull("-out:", Output);
            commandLine.AppendSwitch("-noRepackRes");
            commandLine.AppendSwitch("-parallel");

            foreach (var input in Input)
            {
                commandLine.AppendFileNameIfNotNull(input);
            }

            return commandLine.ToString();
        }
    }
}
