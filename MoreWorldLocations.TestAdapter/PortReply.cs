using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MoreWorldLocations.TestAdapter;

// A migration boundary for the existing probes; scenarios assert fields, not transport OK.
public sealed class PortReply
{
    private static readonly Regex Fields = new Regex(@"(?:^|\s)([A-Za-z][A-Za-z0-9]*)=('[^']*'|\S+)", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250));
    public string[] Lines { get; }
    public IReadOnlyDictionary<string, string> Values { get; }
    public bool Complete => (!Values.TryGetValue("retry", out string? retry) || !retry.Equals("true", StringComparison.OrdinalIgnoreCase)) &&
        (!Values.TryGetValue("shipments", out string? shipments) || shipments != "-1") &&
        (!Values.TryGetValue("manifests", out string? manifests) || manifests != "-1");
    public PortReply(IEnumerable<string> lines)
    {
        Lines = lines.ToArray();
        string? error = Lines.FirstOrDefault(x => x.StartsWith("ERROR:", StringComparison.Ordinal));
        if (error != null) throw new InvalidOperationException(error);
        string[] answers = Lines.Where(x => (x.StartsWith("OK: MWL_", StringComparison.Ordinal) || x.StartsWith("OK: Teleported to MWL port ", StringComparison.Ordinal))).ToArray();
        if (answers.Length != 1) throw new InvalidOperationException("Expected one complete MWL reply, not silence or multiple replies.");
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match match in Fields.Matches(answers[0]))
        {
            string key = match.Groups[1].Value;
            if (values.ContainsKey(key)) throw new InvalidOperationException("Duplicate result field: " + key);
            values.Add(key, match.Groups[2].Value.Trim('\''));
        }
        Values = values;
    }
}
