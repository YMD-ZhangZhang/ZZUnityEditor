using UnityEngine;
using UnityEditor;
//using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using System.IO;
using Exception = System.Exception;

public class ZEditor
{
    // 清理控制台
    public static void ClearConsole()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
        var type = assembly.GetType("UnityEditorInternal.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    /// <summary>
    /// 从Object中依赖收集List<Texture2D>
    /// </summary>
    public static List<Texture2D> CollectTexture2DFromObject(Object obj)
    {
        return CollectTFromObjects<Texture2D>(new Object[] { obj });
    }

    /// <summary>
    /// 从Object[]中依赖收集List<Texture2D>
    /// </summary>
    public static List<Material> CollectMaterialFromObjects(Object[] objs)
    {
        return CollectTFromObjects<Material>(objs);
    }

    /// <summary>
    /// 从Object[]中依赖收集目标 T List,T:Texture2D Material等<Texture2D>
    /// </summary>
    public static List<T> CollectTFromObjects<T>(Object[] objs) where T : class
    {
        Object[] dependencies = EditorUtility.CollectDependencies(objs);
        List<T> list = new List<T>();

        foreach (Object x in dependencies)
        {
            if (x == null)
                continue;

            if (x.GetType() == typeof(T))
                list.Add(x as T);
        }

        return list;
    }

    /// <summary>
    /// 收集指定目录下的文件,不包括子目录,不要.meta
    /// </summary>
    public static FileInfo[] CollectFileButMeta(string path)
    {
        return CollectFileTestFunc(path, (x) =>
        {
            return !x.FullName.EndsWith(".meta");
        });
    }

    /// <summary>
    /// 收集指定目录下的文件,不包括子目录,不要.meta,不要.manifest
    /// </summary>
    public static FileInfo[] CollectFileButMetaButManifest(string path)
    {
        return CollectFileTestFunc(path, (x) =>
        {
            return !x.FullName.EndsWith(".meta") && !x.FullName.EndsWith(".manifest");
        });
    }

    /// <summary>
    /// 收集指定目录下的文件,FileInfo通过testFunc测试。
    /// </summary>
    public static FileInfo[] CollectFileTestFunc(string path, System.Func<FileInfo, bool> testFunc)
    {
        DirectoryInfo folder = new DirectoryInfo(path);
        FileInfo[] files = folder.GetFiles();
        return files.Where(testFunc).ToArray();
    }

    /// <summary>
    /// 收集指定目录下的文件,包括子目录,不要.meta
    /// </summary>
    public static FileInfo[] CollectFilesWithChildrenButMeta(string path)
    {
        DirectoryInfo folder = new DirectoryInfo(path);
        FileInfo[] files = folder.GetFiles("*", SearchOption.AllDirectories);
        return files.Where(x => !x.FullName.EndsWith(".meta")).ToArray();
    }

    /// <summary>
    /// 收集指定目录下的文件,包括子目录,不要.meta,不要.manifest
    /// </summary>
    public static FileInfo[] CollectFilesWithChildrenButMetaButManifest(string path)
    {
        DirectoryInfo folder = new DirectoryInfo(path);
        FileInfo[] files = folder.GetFiles("*", SearchOption.AllDirectories);
        return files.Where(x => !x.FullName.EndsWith(".meta") && !x.FullName.EndsWith(".manifest")).ToArray();
    }

    // 判断GameObject是否含有某个string的组件名，包括子物体
    public static bool HasComponentIncludeChild(GameObject obj, string componentName)
    {
        Component[] components = obj.GetComponentsInChildren<Component>(true);
        return components.Where(x => x != null && x.GetType().Name == componentName).ToArray().Length > 0;
    }

    // 判断GameObject是否含有某个名字的GameObject，包括子物体
    public static bool GameObjectHasGameObject(GameObject obj, string gameObjectName)
    {
        Transform[] transforms = obj.GetComponentsInChildren<Transform>(true);
        foreach (Transform each in transforms)
        {
            if (each.gameObject.name == gameObjectName)
                return true;
        }
        return false;
    }

    // 判断GameObject是否含有某个名字的Material，包括子物体
    public static bool GameObjectHasMaterial(GameObject obj, string materialName)
    {
        Renderer[] array = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var each in array)
        {
            if (ZEditor.RendererHasMaterial(each, materialName))
                return true;
        }
        return false;
    }

    // 判断Renderer是否含有某个名字的Material
    public static bool RendererHasMaterial(Renderer renderer, string materialName)
    {
        return renderer.sharedMaterials.Where(x => x != null && x.name == materialName).ToArray().Length > 0;
    }

    // 构建从根节点到目标节点的全路径
    public static string BuildPath(Transform tran)
    {
        string path = tran.name;
        Transform parent = tran.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    // 构建从目标节点到目标节点的全路径
    public static string BuildPathTo(Transform tran, Transform target)
    {
        string path = tran.name;
        Transform parent = tran.parent;

        while (parent != null && parent != target)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    // 统计父节点数量
    public static int CountParent(Transform tran)
    {
        int count = 0;

        while (tran.parent != null)
        {
            count++;
            tran = tran.parent;
        }

        return count;
    }

    // 获取第N个父节点
    public static Transform GetNParent(Transform tran, int n)
    {
        if (tran.parent == null) return null;
        if (n <= 0) return tran.parent;

        while (tran.parent != null)
        {
            if (--n > 0)
            {
                tran = tran.parent;
                continue;
            }

            return tran.parent;
        }

        return null;
    }

    // 判断某节点是否是其父节点的最后一个节点
    public static bool IsLastChildInParent(Transform tran)
    {
        Transform parent = tran.parent;
        if (parent == null)
            return false;

        if (parent.childCount > 0)
        {
            if (parent.GetChild(parent.childCount - 1) == tran)
            {
                return true;
            }
        }
        return false;
    }

    // Bitmap to Texture2D
    public static Texture2D BitmapToTexture2D(Bitmap bitMap)
    {
        return ByteToTexture2D(BitmapToBytes(bitMap));
    }

    // Bitmap to byte[]
    public static byte[] BitmapToBytes(Bitmap bitMap)
    {
        MemoryStream ms = null;
        try
        {
            ms = new MemoryStream();
            bitMap.Save(ms, bitMap.RawFormat);
            return ms.ToArray();
        }
        catch (Exception e)
        {
            throw e;
        }
        finally
        {
            ms.Close();
        }
    }

    // byte[] to Texture2D
    public static Texture2D ByteToTexture2D(byte[] bytes)
    {
        if (bytes == null)
            return null;

        Texture2D t2d = new Texture2D(0, 0);
        t2d.LoadImage(bytes);
        return t2d;
    }

    /// <summary>
    /// 联合 较新 收集单个物体下的指定组件，包括子物体，自动去重复组件，尤其适用于Project视图
    /// </summary>
    public static List<T> CollectComponentOnOne<T>(Object obj, bool debug = true) where T : Component
    {
        return CollectComponentOnObjects<T>(new Object[] { obj }, debug);
    }

    /// <summary>
    /// 联合 较新 收集选中物体下的指定组件，包括子物体，自动去重复组件，尤其适用于Project视图
    /// </summary>
    public static List<T> CollectComponentOnSelection<T>(bool debug = true) where T : Component
    {
        return CollectComponentOnObjects<T>(Selection.objects, debug);
    }

    /// <summary>
    /// 联合 较新 收集Object[]下的指定组件，包括子物体，自动去重复组件，尤其适用于Project视图
    /// </summary>
    public static List<T> CollectComponentOnObjects<T>(Object[] objs, bool debug = true) where T : Component
    {
        List<GameObject> selectGameObjectList = objs.Where(x => x is GameObject).Select(x => x as GameObject).ToList();
        if (debug) Debug.Log("命中的GameObject数量:" + selectGameObjectList.Count);

        HashSet<T> retSet = new HashSet<T>();
        selectGameObjectList.ForEach(x =>
        {
            foreach (T t in x.GetComponentsInChildren<T>())
            {
                retSet.Add(t);
            }
        });

        if (debug) Debug.Log("命中Component数量:" + retSet.Count);
        //retSet.ToList().ForEach(x => Debug.Log("name:" + x.gameObject.name));

        return retSet.ToList();
    }

    // 收集组件
    public static List<T> CollectComponent<T>() where T : Component
    {
        List<T> resultList = new List<T>();
        List<GameObject> allSceneObjs = ZEditor.GetAllObjInProject();

        if (allSceneObjs.Count <= 0)
        {
            Debug.Log("请在场景中选中一个GameObject");
            return resultList;
        }

        foreach (GameObject each in allSceneObjs)
        {
            T r = each.GetComponent<T>();
            if (r == null) continue;
            resultList.Add(r);
        }

        return resultList;
    }

    /// <summary>
    /// Build Color Use 0-255
    /// </summary>
    public static UnityEngine.Color BuildColor(float r, float g, float b, float a = 255)
    {
        return new UnityEngine.Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    /// <summary>
    /// 从Renderer收集MainTexture 返回List<Object>
    /// </summary>
    public static List<Object> CollectMainTextureFromRenderer(Renderer[] renderers)
    {
        List<Object> list = new List<Object>();
        foreach (Renderer each in renderers)
        {
            List<Object> tempList = CollectMainTextureFromMaterial(each.sharedMaterials);
            list.AddRange(tempList);
        }
        return list;
    }

    /// <summary>
    /// 从Material收集MainTexture 返回List<Object>
    /// </summary>
    public static List<Object> CollectMainTextureFromMaterial(Material[] materials)
    {
        List<Object> list = new List<Object>();
        foreach (Material each in materials)
        {
            if (each.mainTexture == null)
            {
                Debug.Log("没有mainTexture:" + each.name);
                continue;
            }

            list.Add(each.mainTexture);
        }
        return list;
    }

    /// <summary>
    /// 获取UnityEngine.Object资源的全路径
    /// </summary>
    public static string GetAssetFullPath(Object asset)
    {
        return Application.dataPath.Replace("Assets", "") + AssetDatabase.GetAssetPath(asset);
    }

    /// <summary>
    /// UnityEngine.Object转对应的FileInfo
    /// </summary>
    public static FileInfo AssetToFileInfo(Object asset)
    {
        return new FileInfo(GetAssetFullPath(asset));
    }

    #region 收集GameObject
    // 获取选中的所有物体，包括子物体的GameObject
    public static List<GameObject> GetAllObjInProject()
    {
        List<GameObject> objList = new List<GameObject>();
        List<Transform> allTran = GetAllUITran();

        foreach (Transform Obj in allTran)
        {
            if (Obj == null) continue;
            if (Obj.hideFlags == HideFlags.NotEditable) continue;
            if (Obj.hideFlags == HideFlags.HideAndDontSave) continue;
            objList.Add(Obj.gameObject);
        }

        return objList;
    }

    // 获取选中的所有物体，包括子物体的Transform
    private static List<Transform> GetAllUITran()
    {
        List<Transform> list = new List<Transform>();
        Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);

        foreach (Transform parent in transforms)
        {
            list.Add(parent);
            CollectChilds(parent, list);
        }

        return list;
    }

    // 递归收集子GameObject
    private static void CollectChilds(Transform parent, List<Transform> list)
    {
        foreach (Transform child in parent)
        {
            list.Add(child);
            CollectChilds(child, list);
        }
    }
    #endregion 收集GameObject

    public static void SelectObject(UnityEngine.Object obj)
    {
        Selection.objects = new UnityEngine.Object[] { obj };
    }
}
