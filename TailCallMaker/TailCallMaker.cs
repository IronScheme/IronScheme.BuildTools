var fn = args[0];

var lines = new List<string>();

using (TextReader r = File.OpenText(fn))
{
    string line = null;
    while ((line = r.ReadLine()) != null)
    {
        lines.Add(line);
    }
}

for (var i = 0; i < lines.Count; i++)
{
    var line = lines[i];

    var ci = line.IndexOf(':');

    if (ci >= 0)
    {
        if (line.Substring(ci + 1).Trim().StartsWith("callvirt   instance object [Microsoft.Scripting]Microsoft.Scripting.CallTarget"))
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

using (TextWriter w = File.CreateText(fn))
{
    foreach (var line in lines)
    {
        w.WriteLine(line);
    }
}