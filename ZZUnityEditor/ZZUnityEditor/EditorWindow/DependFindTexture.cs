using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class DependFindTexture : EditorWindow
{
    /*
    Object依赖方式查找Texture
    */
    private const string ENTRANCE = "ZZUnityEditor/DependFindTexture";

    private List<Texture2D> findResultList = new List<Texture2D>();
    private Vector2 scrollPosition;

    [MenuItem(ENTRANCE)]
    public static void ShowWindow()
    {  
        GetWindow<DependFindTexture>();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("请选中一个Object");
        ZGUILayout.ColorButton("依赖查找贴图", Color.green, BeginDependFind);

        // 显示查找结果
        if (findResultList.Count > 0)
        {
            DrawFindResult();
        }
    }

    private void BeginDependFind()
    {
        Debug.Log("开始");
        List<Texture2D> list = ZEditor.CollectTFromObjects<Texture2D>(Selection.objects);
        Debug.Log("查找结果数量:" + list.Count);
        findResultList = list;

        DebugFindResult();
    }

    private void DebugFindResult()
    {
        findResultList.ForEach(x =>
        {
            FileInfo file = ZEditor.AssetToFileInfo(x);

            string sourcePath = AssetDatabase.GetAssetPath(x);
            TextureImporter texImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;

            string originFormat = null;
            string formatPC = null;
            string formatAndroid = null;
            string formatIOS = null;
            string alphaSource = null;

            if (texImporter)
            {
                // 原格式
                originFormat = x.format.ToString();

                // PC
                TextureImporterPlatformSettings settingPC = texImporter.GetPlatformTextureSettings("Standalone");
                formatPC = settingPC.format.ToString();

                // Android
                TextureImporterPlatformSettings settingAndroid = texImporter.GetPlatformTextureSettings("Android");
                formatAndroid = settingAndroid.format.ToString();

                // IOS
                TextureImporterPlatformSettings settingIOS = texImporter.GetPlatformTextureSettings("iPhone");
                formatIOS = settingIOS.format.ToString();

                alphaSource = texImporter.alphaSource.ToString();
            }

            string output = x.width + "*" + x.height;
            output += " ★origin:" + originFormat;
            output += " ★PC:" + formatPC;
            output += " ★Android:" + formatAndroid;
            output += " ★IOS:" + formatIOS;
            output += " ★Alpha:" + alphaSource;
            output += " ★size:" + ZUtil.HumanReadableFilesize(file.Length);
            output += " ★" + x.name + file.Extension;
            Debug.Log(output, x);
        });
    }

    private void DrawFindResult()
    {
        ZGUILayout.ScrollView(ref scrollPosition, () =>
        {
            findResultList.ZForeachWithIndex((texture, index) =>
            {
                FileInfo file = ZEditor.AssetToFileInfo(texture);

                ZGUILayout.Horizontal(() =>
                {
                    // 序号
                    GUILayout.Label((index + 1).ToString(), GUILayout.Width(30));
                    EditorGUILayout.ObjectField(texture.name + file.Extension, texture, typeof(Texture), true, GUILayout.Width(250));

                    // 修改按钮
                    ZGUILayout.ColorButton("Modify", Color.green, () => { Modify(texture); }, GUILayout.Width(50));
                });

                ZGUILayout.Horizontal(() =>
                {
                    GUILayout.Label("Info:", GUILayout.Width(30));

                    // msg
                    string msg = texture.width + "x" + texture.height;
                    msg += " ★" + ZUtil.HumanReadableFilesize(file.Length);

                    EditorGUILayout.LabelField(msg);
                });
            });
        }, GUILayout.ExpandWidth(true));
    }

    private void Modify(Texture2D texture)
    {
        /*
        string sourcePath = AssetDatabase.GetAssetPath(x);
        TextureImporter texImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
        texImporter.filterMode = FilterMode.Trilinear;
        AssetDatabase.ImportAsset(sourcePath);
        */

        // TODO 此处可以添加自定义行为
        Debug.Log("处理:" + texture.name);
    }
}
