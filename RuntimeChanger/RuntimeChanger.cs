using Mono.Cecil;

if (args.Length == 0)
{
    Console.WriteLine("Usage: RuntimeChanger [/v4] [/32bit+|/32bit-] Assembly");
    return 1;
}

var v4 = false;
bool? x86 = null;

if (args.Contains("/v4"))
{
    v4 = true;
    args = Array.FindAll(args, a => a != "/v4");
}

if (args.Contains("/32bit+"))
{
    x86 = true;
    args = Array.FindAll(args, a => a != "/32bit+");
}

if (args.Contains("/32bit-"))
{
    x86 = false;
    args = Array.FindAll(args, a => a != "/32bit-");
}

var hasPdb = File.Exists(Path.ChangeExtension(args[0], "pdb"));

var ass = AssemblyDefinition.ReadAssembly(args[0], new ReaderParameters { ReadSymbols = hasPdb, InMemory = true });

var current = FindRuntime(ass);

/*
 * mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
 * mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
 * System.Private.CoreLib, Version=9.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e // net9.0 RE
 */

if (x86 == true)  
{
    ass.MainModule.Attributes |= ModuleAttributes.Required32Bit;
}

if (x86 == false)
{
    ass.MainModule.Attributes &= ~ModuleAttributes.Required32Bit;
}

current.Name = "mscorlib";
current.PublicKeyToken = [0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89];
current.Version = new Version("2.0.0.0");

if (v4)
{
    ass.MainModule.Runtime = TargetRuntime.Net_4_0;
}
else
{
    ass.MainModule.Runtime = TargetRuntime.Net_2_0;
}

ass.Name.Name = Path.GetFileNameWithoutExtension(args[0]);   
ass.MainModule.Name = Path.GetFileName(args[0]);

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

    foreach (var r in ass.MainModule.AssemblyReferences)
    {
        if (r.FullName.StartsWith("mscorlib"))
        {
            return r;
        }
    }

    return null;
}