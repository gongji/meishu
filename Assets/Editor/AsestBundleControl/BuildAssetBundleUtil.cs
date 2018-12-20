
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;


/// <summary>
/// 资源打包工具
/// </summary>
public class BuildAssetBundleUtil
{
    

    
    /// <summary>

    [MenuItem("资源管理/控件打包/单个文件夹方式打包(无依赖)")]
    static void PackAsset()
    {
        BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.DeterministicAssetBundle;
        BuildTarget buildTarget = BuildTarget.WebGL;
        GenerateAssetBundle(buildOptions, buildTarget,false,false,false);
    }

    [MenuItem("资源管理/控件打包/多个子文件夹方式打包(无依赖)")]
    static void PackAsset2()
    {
        BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.DeterministicAssetBundle;
        BuildTarget buildTarget = BuildTarget.WebGL;
        GenerateAssetBundle(buildOptions, buildTarget, true, false, false);
    }

    [MenuItem("资源管理/控件打包/单个文件方式打包(依赖)")]
    static void PackAsset3()
    {
        BuildAssetBundleOptions buildOptions =  BuildAssetBundleOptions.DeterministicAssetBundle;
        BuildTarget buildTarget = BuildTarget.WebGL;
        GenerateAssetBundle(buildOptions, buildTarget, false, true, true);
    }

    static void GenerateAssetBundle(BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform, bool isMutil, bool isCanFileBuild, bool isCanDepenceBuild)
    {   
        //获取点击的资源对象的路径
        UnityEngine.Object selectObject = Selection.activeObject;
        if (selectObject == null)
        {
            Debug.Log("没有选中有效对象，请从新选择一个对象！");
            return;
        }
        string selectedObjectPath = AssetDatabase.GetAssetPath(selectObject);
        Debug.Log("选中的对象信息：" + selectObject + ",path " + selectedObjectPath);

        //导出AssetBundle
        AssetBundleBuild[] bundleBuilds = null;

        if (isCanFileBuild)
        {
            string[] depenceAssetPaths = AssetDatabase.GetDependencies(selectedObjectPath);
            Debug.Log("获取依赖的文件数量 " + depenceAssetPaths.Length);
            //支持文件打包
            //判断该路径的合法性
            if (FileHelper.IsFileOrDirectory(selectedObjectPath) != 1)
            {
                Debug.Log("请选择文件类型的对象");
                return;
            }
            bundleBuilds = new AssetBundleBuild[1];
            bundleBuilds[0].assetBundleName = selectObject.name;
            bundleBuilds[0].assetNames = isCanDepenceBuild ? depenceAssetPaths : new string[] { selectedObjectPath };
        }
        else
        {
            //支持文件夹打包
            //判断该路径的合法性
            if (FileHelper.IsFileOrDirectory(selectedObjectPath) != 2)
            {
                Debug.Log("请选择文件夹类型的对象");
                return;
            }
            if (!isMutil)
            {
                string assetBundleName = selectObject.name;
                Debug.Log(assetBundleName);
                AssetBundleBuild assetsBundleBuild = CreateSimpleAssetBundleBuild(assetBundleName, selectedObjectPath, SearchOption.AllDirectories);
                bundleBuilds = new AssetBundleBuild[1];
                bundleBuilds[0] = assetsBundleBuild;
            }
            else
            {
                bundleBuilds = CreateMutilBundle(selectedObjectPath);
                Debug.Log("子文件数量 " + bundleBuilds.Length);
            }

        }
        Debug.Log("start build " + bundleBuilds.Length);
        //获取当前资源包的导出路径，如果不存在则创建新的
        string exportPath = EditorUtility.OpenFolderPanel("", "", "");
        //开始打包
        BuildPipeline.BuildAssetBundles(exportPath, bundleBuilds, assetBundleOptions, targetPlatform);
        System.Diagnostics.Process.Start(exportPath);
        AssetDatabase.Refresh();
        Debug.Log("打包成功");

    }

    /// <summary>
    /// 创建多个AssetBundleBuild
    /// </summary>
    /// <param name="assetBundleName"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    static AssetBundleBuild[] CreateMutilBundle(string folderPath)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        DirectoryInfo[] childDirectories = directoryInfo.GetDirectories();
        List<AssetBundleBuild> list = new List<AssetBundleBuild>();
        foreach (var item in childDirectories)
        {
            if (!CheckFolderExistFile(item.FullName)) continue;
            string assetBundleName = item.FullName.Substring(item.FullName.LastIndexOf(@"\") + 1);
            AssetBundleBuild abb = CreateSimpleAssetBundleBuild(assetBundleName, item.FullName, SearchOption.AllDirectories);
            list.Add(abb);
        }
        return list.ToArray();
    }

    /// 创建一个AssetBundleBuild
    /// </summary>
    /// <param name="assetBundleName"></param>
    /// <param name="folderPath"></param>
    /// <param name="searchOption"></param>
    /// <returns></returns>
    static AssetBundleBuild CreateSimpleAssetBundleBuild(string assetBundleName, string folderPath, SearchOption searchOption)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        FileInfo[] fileInfos = directoryInfo.GetFiles("*", searchOption);
        AssetBundleBuild myAssetBundleBuild = new AssetBundleBuild();
        myAssetBundleBuild.assetBundleName = assetBundleName;
       
        myAssetBundleBuild.assetNames = new string[fileInfos.Length];
        for (int i = 0; i < fileInfos.Length; i++)
        {
            if (fileInfos[i].FullName.Contains(".meta"))
                continue;
            string assetName = AssetPackComm.GetAssetPath(fileInfos[i].FullName);

            myAssetBundleBuild.assetNames[i] = assetName;
        }

        return myAssetBundleBuild;
    }


    /// <summary>
    /// 检查该文件夹下是否存在有效文件
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    static bool CheckFolderExistFile(string folderPath)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        FileInfo[] files = directoryInfo.GetFiles();
        foreach (var item in files)
        {
            if (!string.IsNullOrEmpty(item.FullName) && !item.FullName.Contains(".mata"))
            {
                return true;
            }
        }
        return false;
    }

    [MenuItem("Tools/设备模型打包")]
    public static void ExportResource()
    {
        string path = EditorUtility.SaveFolderPanel("Save model", "assetbundles", "");
        if (path.Length != 0)
        {
            GameObject[] objects = Selection.gameObjects;
            foreach (GameObject obj in objects)
            {

                string objectPath = path + "/" + obj.name + ".br";
                UnityEngine.Object[] selection = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
               // EditorTool.AddColliderScript(Selection.activeGameObject);
                BuildPipeline.BuildAssetBundle(Selection.activeObject, Selection.objects, objectPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);

                //BuildPipeline.BuildAssetBundles("D:/output", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);

            }
            Debug.Log("export OK");
        }
    }

}
