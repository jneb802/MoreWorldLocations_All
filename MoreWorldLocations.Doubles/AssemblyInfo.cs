// The mod sources are compiled into the TEST assembly while these doubles are
// their own, so anything they share has to be visible across the boundary.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MoreWorldLocations.Tests")]
