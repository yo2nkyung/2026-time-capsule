using System.Collections.Generic;

// Static cross-scene registry for lost/restored memory objects.
public static class MemoryObjectData
{
    // Scene names of memory objects that are currently lost.
    public static readonly HashSet<string> LostObjects = new HashSet<string>();
    public static readonly HashSet<string> RestoredObjects = new HashSet<string>();

    public static void MarkLost(string sceneName) => LostObjects.Add(sceneName);
    public static void MarkRestored(string sceneName)
    {
        LostObjects.Remove(sceneName);
        RestoredObjects.Add(sceneName);
    }

    public static bool IsLost(string sceneName) => LostObjects.Contains(sceneName);
    public static bool IsRestored(string sceneName) => RestoredObjects.Contains(sceneName);

    public static void Reset()
    {
        LostObjects.Clear();
        RestoredObjects.Clear();
    }
}
