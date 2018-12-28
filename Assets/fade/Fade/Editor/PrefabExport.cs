using UnityEngine;
using System.Collections;
using UnityEditor;

public class PrefabExport {

    /// <summary>
    /// prefeb 预制物导出
    /// </summary>
    [MenuItem("Tools/多个资源打包")]
    public static void ExportResource()
    {
        AssetBundleBuild[] builds = new AssetBundleBuild[1];
        Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        string[] TestAsset = new string[selects.Length];
        for (int i = 0; i < selects.Length; i++)
        {
            TestAsset[i] = AssetDatabase.GetAssetPath(selects[i]);
            Debug.Log(TestAsset[i]);
        }
        builds[0].assetNames = TestAsset;
        builds[0].assetBundleName = selects[0].name;
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/StreamingAssets/prefeb", builds, BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.WebGL);
        AssetDatabase.Refresh();
    }

    
}
