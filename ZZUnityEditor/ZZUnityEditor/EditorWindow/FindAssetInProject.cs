using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[ExecuteInEditMode]
public class FindAssetInProject : EditorWindow
{
    private const string ENTRANCE = "ZZUnityEditor/Find Asset In Project";

    class FindResult
    {
        public FileInfo file;
        public UnityEngine.Object obj;

        public FindResult(FileInfo file, UnityEngine.Object obj)
        {
            this.file = file;
            this.obj = obj;
        }
    }

    enum FindType
    {
        ShowAllPrefab,      // 显示所有Prefab
        ComponentName,      // 组件
        GameObjectName,     // GameObject名
        MaterialName,       // 材质球名
        ShaderName,         // Shader名
        ContainsShaderName, // 包含Shader名
        Texture2DName,      // 贴图名
        NGUIAtlasName,      // NGUI图集名
        Object,             // Object
    }

    private FindType findType;
    private Dictionary<FindType, Action<FileInfo, UnityEngine.Object>> handleFuncDic = new Dictionary<FindType, Action<FileInfo, UnityEngine.Object>>();

    private string findComponentName;
    private string findGameObjectName;
    private string findMaterialName;
    private string findShaderName;
    private string findContainsShaderName;
    private string findTexture2DName;
    private string findNGUIAtlasName;
    private UnityEngine.Object findObject;

    private FileInfo[] files;
    private bool running = false;
    private int runIndex;
    private int runIndexMax;

    #region 目标类型
    private bool findPrefab = true;
    private bool findMaterial = false;
    #endregion

    #region 查找目录
    private string findDirectoryString;
    private const string KEY_findDirectoryString = "FindAssetInProject_findDirectoryString";
    #endregion

    // 项目中的MonoBehaviour脚本
    private List<Type> projectScriptList;

    private List<FindResult> findResultList = new List<FindResult>();
    private Vector2 scrollPosition;

    [MenuItem(ENTRANCE)]
    public static void ShowWindow()
    {
        GetWindow<FindAssetInProject>();
    }

    void OnEnable()
    {
        InitHandleFuncDic();
        InitProjectScriptNameList();
        LoadPref();
    }

    void OnGUI()
    {
        // 查找目标类型
        DrawFindTarget();
        // 包括目录
        DrawFindDirectory();

        DrawFindAllPrefab();
        DrawFindComponentName();

        // 查找GameObjectName
        findGameObjectName = DrawCommonFindItem(findGameObjectName, "GameObject Name:", FindType.GameObjectName);
        
        // 查找Material
        findMaterialName = DrawCommonFindItem(findMaterialName, "Material Name:", FindType.MaterialName);
        
        // 查找Shader
        findShaderName = DrawCommonFindItem(findShaderName, "Shader Name:", FindType.ShaderName);
        
        // 查找匹配Shader
        findContainsShaderName = DrawCommonFindItem(findContainsShaderName, "Contains Shader Name:", FindType.ContainsShaderName);
        
        // 查找Texture2D
        findTexture2DName = DrawCommonFindItem(findTexture2DName, "Texture2D Name:", FindType.Texture2DName);
        
        // 查找NGUI图集
        findNGUIAtlasName = DrawCommonFindItem(findNGUIAtlasName, "NGUIAtlas Name:", FindType.NGUIAtlasName);

        DrawFindObject();

        // 显示查找结果
        if (findResultList.Count > 0)
        {
            DrawFindResult();
        }
    }

    void Update()
    {
        if (running)
        {
            HandleNextFile();
        }

        if (running)
        {
            string fStr = (runIndex / (float)runIndexMax * 100f).ToString("#");
            string str = (runIndex + " / " + runIndexMax + " " + fStr + "%");
            EditorUtility.DisplayProgressBar("正在检测", str, runIndex / (float)runIndexMax);
        }
    }

    void OnDestroy()
    {
        EditorUtility.ClearProgressBar();
    }

    private void InitHandleFuncDic()
    {
        handleFuncDic.Add(FindType.ComponentName, HandleFindComponentName);
        handleFuncDic.Add(FindType.GameObjectName, HandleFindGameObjectName);
        handleFuncDic.Add(FindType.MaterialName, HandleFindMaterialName);
        handleFuncDic.Add(FindType.ShaderName, HandleFindShaderName);
        handleFuncDic.Add(FindType.ContainsShaderName, HandleFindContainsShaderName);
        handleFuncDic.Add(FindType.Texture2DName, HandleFindTexture2DName);
        handleFuncDic.Add(FindType.NGUIAtlasName, HandleFindNGUIAtlasName);
        handleFuncDic.Add(FindType.Object, HandleFindObject);
        handleFuncDic.Add(FindType.ShowAllPrefab, HandleShowAllPrefab);
    }

    private void LoadPref()
    {
        findDirectoryString = EditorPrefs.GetString(KEY_findDirectoryString, "Resources;ResourcesAsset;NGUI;EffectMode");
    }

    private void InitProjectScriptNameList()
    {
        projectScriptList = ZReflection.GetTypeListExtendFrom(typeof(MonoBehaviour), true);
        Debug.Log("继承于MonoBehaviour的类数量:" + projectScriptList.Count);
    }

    // 查找目标类型
    private void DrawFindTarget()
    {
        EditorGUILayout.LabelField("目标类型:");
        ZGUILayout.Horizontal(() =>
        {
            findPrefab = EditorGUILayout.Toggle("Prefab", findPrefab, GUILayout.ExpandWidth(false));
            findMaterial = EditorGUILayout.Toggle("Material", findMaterial, GUILayout.ExpandWidth(false));
        });
    }

    // 查找目标目录
    private void DrawFindDirectory()
    {
        EditorGUILayout.LabelField("包括目录:");
        findDirectoryString = EditorGUILayout.TextField(findDirectoryString);
        ZGUILayout.Button("Save", () =>
        {
            EditorPrefs.SetString(KEY_findDirectoryString, findDirectoryString);
        });
    }

    // 显示所有Prefab
    private void DrawFindAllPrefab()
    {
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("显示所有Prefab"))
        {
            findType = FindType.ShowAllPrefab;
            CollectAndRun();
        }
        GUI.backgroundColor = Color.white;
    }

    // 查找Component
    private void DrawFindComponentName()
    {
        findComponentName = EditorGUILayout.TextField("Component:", findComponentName);

        if (!running && !string.IsNullOrEmpty(findComponentName))
        {
            List<Type> ret = projectScriptList.Where(x => x.Name.ToLower().Contains(findComponentName.ToLower())).ToList();

            foreach (Type str in ret)
            {
                string className = str.Name;

                // 有相同的
                if (className.ToLower().Equals(findComponentName.ToLower()))
                {
                    findComponentName = className;
                    break;
                }

                // 相似的
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(className, GUILayout.ExpandWidth(true));
                    // 绿色按钮
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Select", GUILayout.MinWidth(50)))
                    {
                        findComponentName = className;
                        break;
                    }
                    GUI.backgroundColor = Color.white;
                }
                GUILayout.EndHorizontal();
            }
        }

        ZGUILayout.ColorButton(EditorGUIUtility.IconContent("ViewToolZoom"), Color.green, () =>
        {
            if (string.IsNullOrEmpty(findComponentName))
                return;

            findType = FindType.ComponentName;
            CollectAndRun();
        });
    }

    // 绘制查找项
    private string DrawCommonFindItem(string findContent, string label, FindType btnFindType)
    {
        ZGUILayout.Horizontal(() =>
        {
            findContent = EditorGUILayout.TextField(label, findContent);

            ZGUILayout.ColorButton(EditorGUIUtility.IconContent("ViewToolZoom"), Color.green, () =>
            {
                if (string.IsNullOrEmpty(findContent))
                    return;

                findType = btnFindType;
                CollectAndRun();
            }, GUILayout.Width(70));
        });

        return findContent;
    }

    // 查找Object
    private void DrawFindObject()
    {
        ZGUILayout.Horizontal(() =>
        {
            findObject = EditorGUILayout.ObjectField("Object:", findObject, typeof(UnityEngine.Object));

            ZGUILayout.ColorButton(EditorGUIUtility.IconContent("ViewToolZoom"), Color.green, () =>
            {
                if (findObject == null)
                    return;

                findType = FindType.Object;
                CollectAndRun();
            }, GUILayout.Width(70));
        });
    }

    private void CollectAndRun()
    {
        List<FileInfo> fileList = new List<FileInfo>();

        findDirectoryString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ZForeach(dir =>
        {
            string fullPath = Application.dataPath + Path.DirectorySeparatorChar + dir;
            Debug.Log("fullPath:" + fullPath);
            FileInfo[] targetfiles = ZEditor.CollectFilesWithChildrenButMeta(fullPath);
            fileList.AddRange(targetfiles);
        });

        files = fileList.Where(x => IsMatchFile(x)).ToArray();

        Debug.Log("总目标文件数量:" + files.Length);
        if (files.Length > 0)
            BeginRun();
    }

    // 是否匹配目标文件
    private bool IsMatchFile(FileInfo file)
    {
        List<string> matchList = new List<string>();
        if (findPrefab) matchList.Add(".prefab");
        if (findMaterial) matchList.Add(".mat");

        foreach (string x in matchList)
        {
            if (file.FullName.EndsWith(x))
                return true;
        }

        return false;
    }

    private void BeginRun()
    {
        running = true;
        runIndex = 0;
        runIndexMax = files.Length;
        findResultList.Clear();
    }

    private void HandleNextFile()
    {
        FileInfo file = files[runIndex];
        string newFile = file.FullName.Substring(file.FullName.IndexOf("Assets"));

        UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(newFile);
        if (obj != null)
        {
            handleFuncDic[findType](file, obj);
        }

        if (++runIndex == runIndexMax)
        {
            running = false;
            EditorUtility.ClearProgressBar();
            ZEditor.SelectObject(obj);
        }
    }

    private void HandleFindComponentName(FileInfo file, UnityEngine.Object obj)
    {
        GameObject go = obj as GameObject;
        if (ZEditor.HasComponentIncludeChild(go, findComponentName))
        {
            Debug.Log("找到:" + file.FullName);
            findResultList.Add(new FindResult(file, obj));
        }
    }

    private void HandleFindGameObjectName(FileInfo file, UnityEngine.Object obj)
    {
        GameObject go = obj as GameObject;
        if (ZEditor.GameObjectHasGameObject(go, findGameObjectName))
        {
            Debug.Log("找到:" + file.FullName);
            findResultList.Add(new FindResult(file, obj));
        }
    }

    private void HandleFindMaterialName(FileInfo file, UnityEngine.Object obj)
    {
        GameObject go = obj as GameObject;
        if (ZEditor.GameObjectHasMaterial(go, findMaterialName))
        {
            Debug.Log("找到:" + file.FullName);
            findResultList.Add(new FindResult(file, obj));
        }
    }

    private void HandleFindShaderName(FileInfo file, UnityEngine.Object obj)
    {
        UnityEngine.Object[] dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { obj });
        foreach (UnityEngine.Object x in dependencies)
        {
            if (x == null)
                continue;

            if (x.GetType() == typeof(Shader) && x.name == findShaderName)
            {
                Debug.Log("找到:" + file.FullName);
                findResultList.Add(new FindResult(file, obj));
            }
        }
    }

    private void HandleFindContainsShaderName(FileInfo file, UnityEngine.Object obj)
    {
        UnityEngine.Object[] dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { obj });
        foreach (UnityEngine.Object x in dependencies)
        {
            if (x == null)
                continue;

            if (x.GetType() == typeof(Shader) && x.name.Contains(findContainsShaderName))
            {
                Debug.Log("找到:" + file.FullName);
                findResultList.Add(new FindResult(file, obj));
            }
        }
    }

    private void HandleFindTexture2DName(FileInfo file, UnityEngine.Object obj)
    {
        List<Texture2D> texture2DList = ZEditor.CollectTexture2DFromObject(obj);
        foreach (Texture2D each in texture2DList)
        {
            if (each.name == findTexture2DName)
            {
                Debug.Log("找到:" + file.FullName);
                findResultList.Add(new FindResult(file, obj));
                break;
            }
        }
    }

    private void HandleFindNGUIAtlasName(FileInfo file, UnityEngine.Object obj)
    {
        UnityEngine.Object[] dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { obj });
        foreach (UnityEngine.Object x in dependencies)
        {
            if (x == null)
                continue;

            if (x.GetType().ToString() == "UIAtlas" && x.name == findNGUIAtlasName)
            {
                Debug.Log("找到:" + file.FullName);
                findResultList.Add(new FindResult(file, obj));
            }
        }
    }

    private void HandleFindObject(FileInfo file, UnityEngine.Object obj)
    {
        UnityEngine.Object[] dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { obj });
        foreach (UnityEngine.Object x in dependencies)
        {
            if (x == null)
                continue;

            if (x == findObject)
            {
                Debug.Log("找到:" + file.FullName);
                findResultList.Add(new FindResult(file, obj));
            }
        }
    }

    private void HandleShowAllPrefab(FileInfo file, UnityEngine.Object obj)
    {
        findResultList.Add(new FindResult(file, obj));
    }

    private void DrawFindResult()
    {
        GUILayout.Label(">>>>>统计数量:" + findResultList.Count);
        FindResult delete = null;

        ZGUILayout.ScrollView(ref scrollPosition, () =>
        {
            findResultList.ZForeachWithIndex((result, index) =>
            {
                ZGUILayout.Horizontal(() =>
                {
                    // 序号
                    GUILayout.Label((index + 1).ToString(), GUILayout.Width(30));
                    // 选中按钮
                    ZGUILayout.ColorButton(result.obj.name, Color.green, () => { ZEditor.SelectObject(result.obj); }, GUILayout.MinWidth(50));
                    // 删除按钮
                    ZGUILayout.ColorButton("X", Color.red, () => { delete = result; }, GUILayout.Width(50));
                });
            });
        }, GUILayout.ExpandWidth(true));

        if (delete != null) findResultList.Remove(delete);
    }
}
