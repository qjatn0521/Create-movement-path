using UnityEngine;
using System.Collections;
using UnityEditor;
 
public static class ExportPackage {
 

    [MenuItem("Export/Export Files")]
    public static void export()
    {
        string[] projectContent = new string[] {"Assets", "ProjectSettings/TagManager.asset","ProjectSettings/InputManager.asset","ProjectSettings/ProjectSettings.asset", "ProjectSettings/DynamicsManager.asset", "Assets/Editor"};
        AssetDatabase.ExportPackage(projectContent, "packageFile.unitypackage",ExportPackageOptions.Interactive | ExportPackageOptions.Recurse |ExportPackageOptions.IncludeDependencies);
        Debug.Log("Project Exported");
    }
 
}