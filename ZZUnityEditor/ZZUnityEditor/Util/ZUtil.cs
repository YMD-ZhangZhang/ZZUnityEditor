using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ZUtil
{ 
    // 格式化贴图格式字符串
    public static string FormatTextureString(string intput)
    {
        string output = string.Empty;
        output = intput.Replace("4444", "16");
        output = output.Replace("565", "16");
        return output;
    }

    // 格式化打印Texture2D
    public static string LogPrintTexture2D(Texture2D texture)
    {
        return (texture.width + "*" + texture.height + " " + ZUtil.FormatTextureString(texture.format.ToString()) + " >>> " + texture.name);
    }

    /// <summary>
    /// 格式化文件大小
    /// </summary>
    public static String HumanReadableFilesize(long size)
    {
        long mbX = 1024 * 1024;
        long kbX = 1024;

        long mb = size / mbX;
        long kb = (size - mb * mbX) / kbX;
        long bytes = size - mb * mbX - kb * kbX;

        if (mb > 0)
        {
            float mbf = mb + kb / 1024f;
            return mbf.ToString("#.0") + "MB";
        }

        if (kb > 0)
        {
            float kbf = kb + bytes / 1024f;
            return kbf.ToString("#.0") + "KB";
        }

        return size + "B";
    }
}
