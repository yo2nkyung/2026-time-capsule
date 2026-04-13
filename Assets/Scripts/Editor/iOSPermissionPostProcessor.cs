#if UNITY_EDITOR && UNITY_IOS

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

// Adds missing iOS permission text to the generated Info.plist.
public static class iOSPermissionPostProcessor
{
    private const string MotionUsageKey = "NSMotionUsageDescription";
    private const string MotionUsageValue = "Required for head tracking in VR mode.";

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS)
            return;

        string plistPath = buildPath + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict rootDict = plist.root;
        if (!rootDict.values.ContainsKey(MotionUsageKey))
        {
            rootDict.SetString(MotionUsageKey, MotionUsageValue);
        }

        plist.WriteToFile(plistPath);
    }
}

#endif
