using Mono.Cecil;

if (args.Length == 0)
{
    Console.WriteLine("RuntimeChanger Assembly");
    return 1;
}

var hasPdb = File.Exists(Path.ChangeExtension(args[0], "pdb"));

var ass = AssemblyDefinition.ReadAssembly(args[0], new ReaderParameters { ReadSymbols = hasPdb, InMemory = true });

var current = FindRuntime(ass);

/*
 * mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
 * mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
 * System.Private.CoreLib, Version=9.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e // net9.0 RE
 */

current.Name = "mscorlib";
current.Version = new Version("2.0.0.0");
current.PublicKeyToken = [0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89];
ass.MainModule.Runtime = TargetRuntime.Net_2_0;

Console.WriteLine("Saving assembly: {0}", args[0]);
ass.Write(args[0], new WriterParameters { WriteSymbols = hasPdb });

return 0;
AssemblyNameReference FindRuntime(AssemblyDefinition ass)
{
    foreach (var r in ass.MainModule.AssemblyReferences)
    {
        if (r.FullName.StartsWith("System.Private.CoreLib"))
        {
            return r;
        }
    }

    return null;
}