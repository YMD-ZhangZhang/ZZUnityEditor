using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class ZGUILayout
{
    // 绘制Select按钮
    public static void DrawSelectButton(GameObject obj)
    {
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Select", GUILayout.MinWidth(80), GUILayout.MaxWidth(80)))
        {
            Selection.objects = new UnityEngine.Object[] { obj };
        }
        GUI.backgroundColor = Color.white;
    }

    // 绘制X删除按钮
    public static bool DrawDeleteButton()
    {
        GUI.backgroundColor = Color.red;
        bool result = GUILayout.Button("X", GUILayout.MinWidth(30), GUILayout.MaxWidth(30));
        GUI.backgroundColor = Color.white;
        return result;
    }

    public static void Button(string text, System.Action callback)
    {
        if (GUILayout.Button(text))
            callback();
    }

    public static void Button(string text, System.Action callback, params GUILayoutOption[] options)
    {
        if (GUILayout.Button(text, options))
            callback();
    }

    public static void ColorButton(string text, Color color, Action callback, params GUILayoutOption[] options)
    {
        GUI.backgroundColor = color;
        if (GUILayout.Button(text, options))
            callback();
        GUI.backgroundColor = Color.white;
    }

    public static void ColorButton(GUIContent content, Color color, Action callback, params GUILayoutOption[] options)
    {
        GUI.backgroundColor = color;
        if (GUILayout.Button(content, options))
            callback();
        GUI.backgroundColor = Color.white;
    }

    // 水平绘制
    public static void Horizontal(Action drawFunc)
    {
        GUILayout.BeginHorizontal();
        drawFunc();
        GUILayout.EndHorizontal();
    }

    // 垂直绘制
    public static void Vertical(Action drawFunc)
    {
        GUILayout.BeginVertical();
        drawFunc();
        GUILayout.EndVertical();
    }

    // ScrollView
    public static void ScrollView(ref Vector2 scrollPosition, Action func, params GUILayoutOption[] options)
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, options);
        func();
        EditorGUILayout.EndScrollView();
    }
}
