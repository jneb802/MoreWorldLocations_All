using Xunit;

// The harness stands in for a running game, and a game has one of each: one
// ZDOMan, one WorldGenerator, one registered Heightmap. Those are statics on the
// shims, so two test classes running at once are two worlds writing to one set
// of globals.
//
// xunit runs separate collections in parallel by default. With the terrain
// bridge's tests added, the net48 runner started dying in native code --
// mono_assembly_names_equal_flags under mono_domain_assembly_search. Measured:
// with parallel collections it aborted on 3 of 8 runs (twice in a row, then once
// more in a control of three); with this attribute, 6 of 6 clean, and either
// half of the suite alone was always clean. That is a race, so the counts are
// the claim and not "fixed" -- but it is the same crash shape that was recorded
// as an unexplained one-off during the work on f034a3f, and that one-off now has
// a candidate cause it did not have before.
//
// The suite runs in 0.2 s, so serialising it costs nothing worth having.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
