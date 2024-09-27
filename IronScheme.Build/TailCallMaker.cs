using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace IronScheme.Build
{
    public class TailCallMaker : Task
    {
        [Required]
        public ITaskItem Input { get; set; }

        [Output]
        public ITaskItem Output { get; set; }

        public override bool Execute()
        {
            Log.LogMessage("Input: {0}", Input);
            var input = Input.ItemSpec;

            Log.LogMessage("Output: {0}", Output ?? Input);
            var output = Output?.ItemSpec ?? input;

            var lines = File.ReadAllLines(input);

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                var ci = line.IndexOf(':');

                if (ci >= 0)
                {
                    if (line.Substring(ci + 1).Trim().StartsWith("callvirt   instance object [IronScheme.Scripting]Microsoft.Scripting.CallTarget"))
                    {
                        lines[i] = line.Replace("callvirt", "call");
                    }
                    else
                    if (line.Substring(ci + 1).Trim().StartsWith("ret"))
                    {
                        var j = 1;
                        var prevline = lines[i - j];

                        ci = prevline.IndexOf(':');

                        while (ci < 0)
                        {
                            j++;
                            prevline = lines[i - j];
                            ci = prevline.IndexOf(':');
                        }

                        var prevcmd = prevline.Substring(ci + 1).Trim();

                        if (prevcmd.StartsWith("call") && (prevcmd.Contains("::Invoke(") || prevcmd.Contains("::Call(")))
                        {
                            lines[i - j] = "tail. " + prevline;
                        }
                    }
                }
            }

            Log.LogMessage("Writing output to: {0}", output);
            File.WriteAllLines(output, lines);

            return true;
        }
    }
}
