using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.SceneManagement;

namespace Equipment
{

    /*
   *场景资源打包
   *
   */
    public class SceneExport
    {
     
        /// <summary>
        /// 场景打包，打包成
        /// </summary>

        [MenuItem("Tools/mainscene打包")]
        public static void CreateSceneALL()
        {
            //清空一下缓存  
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            GameObject[] gs = scene.GetRootGameObjects();

            //打包的时候增加碰撞器
            //foreach (GameObject g in gs)
            //{
            //    MeshRenderer[] mrs = g.GetComponentsInChildren<MeshRenderer>();
            //    foreach (MeshRenderer mr in mrs)
            //    {
            //        Collider collider = mr.GetComponent<Collider>();
            //        if (collider == null)
            //        {
            //            if (mr.name.ToLower().Contains("Collider".ToLower()))
            //            {
            //                mr.gameObject.AddComponent<BoxCollider>();
            //            }
            //            else
            //            {
            //                mr.gameObject.AddComponent<MeshCollider>();
            //            }

            //        }

            //    }
               

            //}
            //将环境光数据添加到资源文件中
            if (scene.name.ToLower().Contains("skybox".ToLower()))
            {
                AudioListener al = GameObject.FindObjectOfType<AudioListener>();
                if (al!=null)
                {
                    GameObject.DestroyImmediate(al);
                }
                EditorSceneManager.SaveOpenScenes();
                RenderSettingsValue rsv = null;
                bool flag = false;
                GameObject _g;
                foreach (GameObject g in gs)
                {
                    if (g.name.Contains("main"))
                    {
                        rsv = g.GetComponent<RenderSettingsValue>();
                        if (!rsv)
                        {
                            rsv = g.AddComponent<RenderSettingsValue>();
                        }
                        flag = true;
                        _g = g;
                        break;
                    }
                }

                if (!flag)
                {
                    _g = new GameObject();
                    _g.name = "_RenderSettings";
                    rsv = _g.GetComponent<RenderSettingsValue>();
                    if (!rsv)
                    {
                        rsv = _g.AddComponent<RenderSettingsValue>();
                    }
                }

                rsv.skyMaterial = RenderSettings.skybox;
                rsv.ambientMode = RenderSettings.ambientMode;
                rsv.ambientIntensity = RenderSettings.ambientIntensity;
                rsv.ambientLight = RenderSettings.ambientLight;
                rsv.ambientSkyColor = RenderSettings.ambientSkyColor;
                rsv.ambientEquatorColor = RenderSettings.ambientEquatorColor;
                rsv.ambientGroundColor = RenderSettings.ambientGroundColor;
                rsv.isfog = RenderSettings.fog;

                rsv.fogColor = RenderSettings.fogColor;
                rsv.fogmode = RenderSettings.fogMode;
                rsv.fogStartDistance = RenderSettings.fogStartDistance;
                rsv.fogEndDistance = RenderSettings.fogEndDistance;
                rsv.fogDensityValue = RenderSettings.fogDensity;
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("增加了环境光的数据");
            }
            //清空一下缓存  
            Caching.ClearCache();
            string path = EditorUtility.SaveFolderPanel("请选择保存位置", "", "");//选择存储的位置

            // string folderPath = Application.dataPath + "/Model";//指定需要打包场景文件夹路径

            string folderPath = "";//指定需要打包场景文件夹路径

            if (path.Length != 0)
            {
                if (folderPath.Length != 0)
                {
                    try
                    {
                        string[] arrStrAudioPath = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                        for (int i = 0; i < arrStrAudioPath.Length; i++)
                        {
                            if (System.IO.Path.GetExtension(arrStrAudioPath[i]) == ".unity")
                            {
                                arrStrAudioPath[i] = arrStrAudioPath[i].Replace("\\", "/");
                                string[] _filePath = arrStrAudioPath[i].Split('/');
                                string[] _fileName = _filePath[_filePath.Length - 1].Split('.');
                                //Debug.Log("遍历文件获取文件名：" + _fileName[0]);
                                //Debug.Log("遍历文件获取文件路径：" + arrStrAudioPath[i]);

                                BuildScene(path, arrStrAudioPath[i], _fileName[0]);
                            }
                        }
                    }
                    catch
                    {
                        Debug.Log("路径不对：" + folderPath);
                    }
                }
                else
                {
                    UnityEngine.Object[] objects = Selection.objects;
                    foreach (UnityEngine.Object obj in objects)
                    {
                        string objectPath = AssetDatabase.GetAssetPath(obj);
                        if (System.IO.Path.GetExtension(objectPath) == ".unity")
                        {
                            BuildScene(path, objectPath, obj.name);
                        }
                    }
                }
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">存储路径</param>
        /// <param name="levesName">需要打包场景名</param>
        /// <param name="saveFileName">保存文件名</param>
        protected static void BuildScene(string path, string levesName, string saveFileName)
        {
            string[] levels = { levesName };
            string _path = path + "/" + saveFileName + ".unity3d";
            //打包场景  
            BuildPipeline.BuildPlayer(levels, _path, BuildTarget.WebGL, BuildOptions.BuildAdditionalStreamedScenes);
        }

        /// 将选定的多个对象进行打包, 同时包含依赖项, 不指定 AssetBundle 的 main 属性获取.
        /// </summary>
        [MenuItem("Tools/多资源选择打包")]
        private static void CreateMultipleAssetBundle()
        {
            Caching.ClearCache(); 

            if (Selection.objects.Length > 0)
            {
                //显示保存窗口
                string path = EditorUtility.SaveFilePanel("Create Multiple AssetBundle:", "", "New AssetBundle", "unity3d");

                if (path.Length > 0)
                {
                    UnityEngine.Object[] SelectedAsset = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

                    foreach (UnityEngine.Object obj in SelectedAsset)
                    {
                        Debug.Log("Create AssetBunldes name :" + obj);
                    }

                    //这里注意第二个参数就行  
                    if (BuildPipeline.BuildAssetBundle(null, SelectedAsset, path, BuildAssetBundleOptions.CollectDependencies, BuildTarget.WebGL))
                    {
                        AssetDatabase.Refresh();
                    }
                    
                }
            }
        }

        /// <summary>
        /// 清理依赖名称
        /// </summary>
        private static void ClearAllAsset()
        {
            int _length = AssetDatabase.GetAllAssetBundleNames().Length;
            string[] _oldAssetBundleNames = new string[_length];
            for (int i = 0; i < _length; i++)
            {
                _oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
            }
            for (int j = 0; j < _oldAssetBundleNames.Length; j++)
            {
                AssetDatabase.RemoveAssetBundleName(_oldAssetBundleNames[j], true);
            }

        }

        [MenuItem("Tools/目录自动场景打包")]
        public static void AutoBuildAssetBundleScene()
        {
            string savePath = EditorUtility.SaveFolderPanel("Save Scene", "save Path", "");
            string folderPath = Application.dataPath + "/Test";//指定需要打包场景文件夹路径
            if (folderPath.Length > 0)
            {
                int location = folderPath.IndexOf("Assets");
                string _folderPath = folderPath.Substring(location);
                string[] arrStrAudioPath = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                for (int i = 0; i < arrStrAudioPath.Length; i++)
                {
                    ClearAllAsset();
                    if (System.IO.Path.GetExtension(arrStrAudioPath[i]) == ".unity")
                    {
                        AddCollider(arrStrAudioPath[i]);
                        string fileName = System.IO.Path.GetFileName(arrStrAudioPath[i]);
                        int assetLocation = arrStrAudioPath[i].IndexOf("Assets");
                        string newfolderPath = arrStrAudioPath[i].Substring(assetLocation);
                        fileName = fileName + "3d";
                        SetAssetImporterName(newfolderPath, fileName);
                        BuildPipeline.BuildAssetBundles(savePath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);
                    }
                }
            }

        }

        [MenuItem("Assets/Tools/手动选择场景打包")]
        public static void ManualBuildAssetBundleScene()
        {
            string savePath = EditorUtility.SaveFolderPanel("Save Scene", "save Path", "");
            UnityEngine.Object[] objests = Selection.objects;
            foreach (UnityEngine.Object o in objests)
            {
                ClearAllAsset();
                string name = o.name + ".unity3d";
                string objectPath = AssetDatabase.GetAssetPath(o);
                AddCollider(objectPath);
                SetAssetImporterName(objectPath, name);
                BuildPipeline.BuildAssetBundles(savePath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);
            }

        }


        private static  void AddCollider(string path)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(path);
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;

            AudioListener audioListener = GameObject.FindObjectOfType<AudioListener>();
            if (audioListener)
            {
                GameObject.DestroyImmediate(audioListener);
            }
            //Debug.Log(scene.name);
            GameObject[] gs = scene.GetRootGameObjects();

            foreach (GameObject g in gs)
            {
                Transform[] ts = g.GetComponentsInChildren<Transform>();
                foreach (Transform t in ts)
                {
                    Collider collider = t.GetComponent<Collider>();
                    if (collider == null && t.GetComponent<MeshRenderer>()!=null)
                    {
                        t.gameObject.AddComponent<MeshCollider>();
                    }
                }
            }
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();
        }
        private static void SetAssetImporterName(string path, string name)
        {
          //  Debug.Log("path=" + path);
           // Debug.Log("name=" + name);
            AssetImporter _assetImporter = AssetImporter.GetAtPath(path);
            if (_assetImporter)
            {
                Debug.Log("设置成功");
                string assetBundleName = name;

                _assetImporter.assetBundleName = assetBundleName;
            }
            else
            {
                Debug.Log("路径不正确");
            }
        }


         
        

    }
}

