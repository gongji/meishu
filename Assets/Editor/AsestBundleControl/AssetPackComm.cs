using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetPackComm
{
    public static readonly string assetsFolderName = "";
    public static readonly string mainAssetsFolderName = "MainAssets";
    public static readonly string icoFolderName = "ICO";


    /// <summary>
    /// 获取导出资源的文件夹
    /// </summary>
    /// <param name="folderName"></param>
    /// <returns></returns>
    public static string GetExportRootFolder(string folderName)
    {
        return Path.Combine(Application.dataPath, "ExportResources/" + folderName);
    }

    /// <summary>
    /// 获取Untiy Assets路径
    /// </summary>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    public static string GetAssetPath(string fullPath)
    {
        int index = fullPath.IndexOf("Assets");
        return fullPath.Substring(index);
    }



}
