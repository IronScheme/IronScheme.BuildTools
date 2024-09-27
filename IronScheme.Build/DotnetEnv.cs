using Microsoft.Build.Framework;
using Mono.Cecil;
using Task = Microsoft.Build.Utilities.Task;

namespace IronScheme.Build
{
    public class DotnetEnv : Task
    {
        [Required]
        public string Action { get; set; }

        [Output]
        public string Output { get; set; }

        public override bool Execute()
        {


            return true;
        }
    }
}

/*
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

if (args.Length == 0)
{
    return 1;
}

var arg1 = GetArg(1);

var pathre = new Regex(@"^((?<tfm>.+)\s)?(?<ver>.+)\s\[(?<path>.+)\]$");

switch (args[0])
{
    case "runtime":
    {
        return 1;
    }
    case "exe":
    {
        var sdks = Run("--list-sdks");

        foreach (var line in sdks.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries).Reverse())
        {
            var m = pathre.Match(line);
            if (m.Success)
            {
                var path = m.Groups["path"].Value;
                var rootpath = Path.Combine(path, "..", "dotnet.exe");

                if (File.Exists(rootpath))
                {
                    Console.WriteLine(Path.GetFullPath(rootpath));
                    return 0;
                }
                else
                {
                    rootpath = Path.ChangeExtension(rootpath, string.Empty);
                    if (File.Exists(rootpath))
                    {
                        Console.WriteLine(Path.GetFullPath(rootpath));
                        return 0;
                    }
                }
            }
        }
        return 1;
    }
    case "sdk":
    {
        var sdks = Run("--list-sdks");

        foreach (var line in sdks.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries).Reverse())
        {
            var m = pathre.Match(line);
            if (m.Success)
            {
                var ver = m.Groups["ver"].Value;
                var path = m.Groups["path"].Value;
                var verpath = Path.Combine(path, ver);
                Console.WriteLine(verpath);
                return 0;
            }
        }
        return 1;
    }

    case "framework":
    {
        var conf = GetArg(1);
        if (conf is null)
        {
            var runtimes = Run("--list-runtimes");

            foreach (var line in runtimes.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries).Reverse())
            {
                var m = pathre.Match(line);
                if (m.Success)
                {
                    var tfm = m.Groups["tfm"].Value;
                    var ver = m.Groups["ver"].Value;
                    var path = m.Groups["path"].Value;

                    if (tfm == "Microsoft.NETCore.App")
                    {
                        var verpath = Path.Combine(path, ver);
                        Console.WriteLine(verpath);
                        return 0;
                    }
                }
            }
        }
        else
        {
            var content = File.ReadAllText(conf);
            var configFile = JsonConvert.DeserializeObject<ConfigFile>(content);
            var cname = configFile.RuntimeOptions.Framework.Name;
            var cver = configFile.RuntimeOptions.Framework.Version;

            var runtimes = Run("--list-runtimes");

            foreach (var line in runtimes.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries))
            {
                var m = pathre.Match(line);
                if (m.Success)
                {
                    var tfm = m.Groups["tfm"].Value;
                    var ver = m.Groups["ver"].Value;
                    var path = m.Groups["path"].Value;

                    if (tfm == cname &&  ver == cver)
                    {
                        var verpath = Path.Combine(path, ver);
                        Console.WriteLine(verpath);
                        return 0;
                    }
                }
            }
        }
        return 1;
    }

    default:
        return 1;
}

string GetArg(int pos)
{
    return args.Length > pos ? args[pos] : null;
}

string Run(string argz)
{
    var error = new StringWriter();
    var output = new StringWriter();

    var p = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            RedirectStandardOutput = true,
            StandardOutputEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = argz,
        }
    };

    p.OutputDataReceived += (s, e) => output.WriteLine(e.Data);

    p.Start();

    p.BeginOutputReadLine();

    var exited = p.WaitForExit(300000);

    return output.ToString().TrimEnd(Environment.NewLine.ToCharArray());
}

class ConfigFile
{
    public RuntimeOptions RuntimeOptions { get; set; }
}


class RuntimeConfigFramework
{
    public string Name { get; set; }
    public string Version { get; set; }
}

class RuntimeOptions
{
    public string Tfm { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string RollForward { get; set; }

    public RuntimeConfigFramework Framework { get; set; }

    public List<RuntimeConfigFramework> Frameworks { get; set; }

    public List<RuntimeConfigFramework> IncludedFrameworks { get; set; }

    public List<string> AdditionalProbingPaths { get; set; }
}


 */