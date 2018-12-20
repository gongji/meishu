using UnityEngine;
using System.Collections;
using UnityEditor;

public class PrefabExport {

    /// <summary>
    /// prefeb 预制物导出
    /// </summary>
    [MenuItem("Tools/对象打包")]
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
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/StreamingAssets/Test", builds, BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.WebGL);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// prefeb 预制物导出
    /// </summary>
    [MenuItem("Tools/旧的设备打包方式")]
    public static void ExportEquipment()
    {
        string path = EditorUtility.SaveFolderPanel("Save model", "assetbundles", "");
        if (path.Length != 0)
        {
            GameObject[] objects = Selection.gameObjects;
            foreach (GameObject obj in objects)
            {

                string objectPath = path + "/" + obj.name + ".unity3d";
                UnityEngine.Object[] selection = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
                // EditorTool.AddColliderScript(Selection.activeGameObject);
                BuildPipeline.BuildAssetBundle(Selection.activeObject, Selection.objects, objectPath, BuildAssetBundleOptions.CollectDependencies, BuildTarget.WebGL);

                // BuildPipeline.bui

                // public static bool BuildAssetBundle(Object mainAsset, Object[] assets, string pathName, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform);
                //Obsolete public static bool BuildAssetBundle(Object mainAsset, Object[] assets, string pathName, out uint crc, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform);



                //BuildPipeline.BuildAssetBundles("E:/output", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);

            }
            Debug.Log("export OK");
        }
    }


}
