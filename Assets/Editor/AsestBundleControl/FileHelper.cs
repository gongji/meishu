using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class FileHelper
{
    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {

                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }

    public static void CopyFile(string inFile, string outFile)
    {
        using (FileStream fs = new FileStream(inFile, FileMode.Open))
        {
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);

            if (buffer.Length > 0)
            {
                using (FileStream ws = new FileStream(outFile, FileMode.Create))
                {
                    ws.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }

    /// <summary>
    /// 获取文件大小，单位KB
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static long GetFileSize(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        long size = (long)Mathf.Ceil(fileInfo.Length / 1024f);//单位KB
        return size;
    }

    /// <summary>
    /// 判断路径的是否为文件路径或文件夹路径
    /// 1：文件路径
    /// 2：文件夹路径
    /// 0：都不是
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static int IsFileOrDirectory(string path)
    {
        if (File.Exists(path))
            return 1;
        else if (Directory.Exists(path))
            return 2;
        else return 0;
    }

    public static string GetFileName(string folderPath, string extension)
    {
        string[] files = System.IO.Directory.GetFiles(folderPath, "*");
        if (files.Length > 0)
        {
            foreach (var item in files)
            {
                if (Path.GetExtension(item).Contains(extension))
                {
                    return Path.GetFileName(item);
                }
            }
        }
        return null;
    }
}
