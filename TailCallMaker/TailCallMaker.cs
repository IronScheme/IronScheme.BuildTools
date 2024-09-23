var fn = args[0];

var lines = File.ReadAllLines(fn);

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

File.WriteAllLines(fn, lines);