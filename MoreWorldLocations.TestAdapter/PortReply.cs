using System;
using System.Collections.Generic;
using System.Linq;
using Valheim.Testing.Adapter;

namespace MoreWorldLocations.TestAdapter;

// A migration boundary for the existing probes; scenarios assert fields, not transport OK.
// The fields are read by the toolkit's KeyValueReply; which line is the answer, and when it is complete, is MWL's.
public sealed class PortReply
{
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
        // Every refusal is an InvalidOperationException, a repeated field included.
        try { Values = KeyValueReply.Parse(answers[0]); }
        catch (FormatException duplicate) { throw new InvalidOperationException(duplicate.Message, duplicate); }
    }
}
