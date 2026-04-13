using System.Collections.Generic;

// Shared static data for collected time capsule items.
public static class TimeCapsuleGameData
{
    public static readonly List<string> CollectedItems = new List<string>();

    // clear the collected items for a new game
    public static void Reset() => CollectedItems.Clear();
}
