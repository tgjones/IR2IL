namespace IR2IL;

/// <summary>
/// How evaluating an instruction interacts with memory. Ordered: a larger value is more restrictive
/// when deciding whether the instruction can be evaluated later than where it appears.
/// </summary>
internal enum MemoryEffect
{
    /// <summary>Neither reads nor writes memory, so it can be evaluated at any point.</summary>
    None,

    /// <summary>Reads memory, so it must not be moved past anything that writes memory.</summary>
    Reads,

    /// <summary>May write memory (or have other effects that can't be reordered), so it must not be moved at all.</summary>
    Writes,
}
