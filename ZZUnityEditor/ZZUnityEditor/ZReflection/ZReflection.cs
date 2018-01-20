using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ZReflection
{
    #region 属性
    // 获取属性模版
    public static T GetProperty<T>(object obj, string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
    {
        Type type = obj.GetType();
        PropertyInfo property = type.GetProperty(name, flags);
        object value = property.GetValue(obj, null);
        return (T)value;
    }

    // 设置属性
    public static void SetProperty(object obj, string name, object value)
    {
        Type type = obj.GetType();
        PropertyInfo property = type.GetProperty(name);
        property.SetValue(obj, value, null);
    }
    #endregion 属性

    #region 字段
    // 获取字段模版
    public static T GetField<T>(object obj, string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
    {
        Type type = obj.GetType();
        FieldInfo field = type.GetField(name, flags);
        object value = field.GetValue(obj);
        return (T)value;
    }

    // 设置字段
    public static void SetField(object obj, string name, object value)
    {
        Type type = obj.GetType();
        FieldInfo field = type.GetField(name);
        field.SetValue(obj, value);
    }
    #endregion 字段

    #region 方法
    // 调用方法
    public static T CallMethod<T>(object obj, string name, BindingFlags flags, params object[] param)
    {
        Type type = obj.GetType();
        MethodInfo method = type.GetMethod(name, flags);
        return (T)method.Invoke(obj, param);
    }
    #endregion 方法

    // 获取所有继承某类的类
    public static List<Type> GetTypeListExtendFrom(Type targetType, bool findAllBase)
    {
        List<Type> list = new List<Type>();

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                // 往上查找
                if (findAllBase)
                {
                    if (IsInherit(type, targetType))
                        list.Add(type);
                }
                else// 只找当前
                {
                    if (type.BaseType == targetType)
                    {
                        list.Add(type);
                    }
                }
            }
        }

        return list;
    }

    private static bool IsInherit(Type now, Type baseType)
    {
        while (true)
        {
            Type super = now.BaseType;
            if (super == null)
                return false;

            if (super == baseType)
                return true;

            now = super;
        }
    }
}
