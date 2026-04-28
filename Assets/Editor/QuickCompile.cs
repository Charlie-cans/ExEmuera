using UnityEditor;
using UnityEngine;
using System.Linq;

public class QuickCompile : EditorWindow
{
    [MenuItem("Tools/Quick Compile Check")]
    static void CompileCheck()
    {
        EditorUtility.DisplayProgressBar("Compiling", "Refreshing assets...", 0.5f);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        EditorUtility.ClearProgressBar();
        Debug.Log("[QuickCompile] Asset refresh triggered. Check Console for errors.");
    }

    [MenuItem("Tools/Pretty Print Compiler Errors")]
    static void PrintErrors()
    {
        var errors = System.AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return new System.Type[0]; } });
        Debug.Log("[QuickCompile] Checking assemblies... Check Console window (Window > General > Console) for red error lines.");
    }
}
