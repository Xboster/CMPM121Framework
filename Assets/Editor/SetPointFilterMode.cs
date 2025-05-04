using UnityEngine;
using UnityEditor;
using System.IO;

public class SetPointFilterMode : EditorWindow
{
    [MenuItem("Tools/Set Point Filter Mode for Sprites")]
    static void SetPointFilterModeForSprites()
    {
        string folderPath = EditorUtility.OpenFolderPanel("Select Folder with Sprites", "Assets", "");

        if (string.IsNullOrEmpty(folderPath))
            return;

        string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { relativePath });

        int count = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed; // Optional: keeps pixel-perfect clarity
                importer.SaveAndReimport();
                count++;
            }
        }

        Debug.Log($"Updated {count} sprite(s) to use Point filter mode.");
    }
}
