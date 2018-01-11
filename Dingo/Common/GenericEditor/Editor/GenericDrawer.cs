using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Dingo.Common.GenericEditor
{
    public interface IGenericDrawer
    {
        object DrawGUI(object target, string label = "", Type targetType = null, FieldInfo fieldInfo = null, object context = null);
    }

    public interface ITypedGenericDrawer<T> : IGenericDrawer
    {
    }

    public abstract class GenericDrawer<T> : ITypedGenericDrawer<T>
    {
        public object DrawGUI(object target, string label = "", Type targetType = null, FieldInfo fieldInfo = null, object context = null)
        {
            return DrawGUIInternal((T)target, label, targetType, fieldInfo, context);
        }
        protected abstract T DrawGUIInternal(T target, string label = "", Type targetType = null, FieldInfo fieldInfo = null, object context = null); 
    }

    public class FloatDrawer : GenericDrawer<float>
    {
        protected override float DrawGUIInternal(float target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return EditorGUILayout.FloatField(label, target);
        }
    }

    public class IntDrawer : GenericDrawer<int>
    {
        protected override int DrawGUIInternal(int target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return EditorGUILayout.IntField(label, target);
        }
    }

    public class StringDrawer : GenericDrawer<string>
    {
        protected override string DrawGUIInternal(string target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return EditorGUILayout.TextField(label, target);
        }
    }

    public class ColorDrawer : GenericDrawer<Color>
    {
        protected override Color DrawGUIInternal(Color target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return EditorGUILayout.ColorField(label, target);
        }
    }

    public class IListDrawer : GenericDrawer<IList>
    {
        protected override IList DrawGUIInternal(IList target, string label = "", Type targetType = null,
            FieldInfo fieldInfo = null,
            object context = null)
        {
            if (fieldInfo != null)
            {
                if (!GenericEditor.ExpandedCache.ContainsKey(fieldInfo))
                {
                    GenericEditor.ExpandedCache[fieldInfo] = false;
                }
                GenericEditor.ExpandedCache[fieldInfo] =
                    EditorGUILayout.Foldout(GenericEditor.ExpandedCache[fieldInfo], fieldInfo.Name);
            }
            
            if (fieldInfo != null && GenericEditor.ExpandedCache[fieldInfo])
            {
                for (var i = 0; i < target.Count; i++)
                {
                    var o = target[i];
                    GenericEditor.DrawGUI(o);
                    target[i] = o;
                    EditorExtensions.Seperator();
                }
                var listType = targetType.GetGenericArguments()[0];
                if (listType.IsAbstract || listType.IsInterface || (fieldInfo != null && fieldInfo.HasAttribute<ListGenericUIAttribute>() && fieldInfo.GetAttribute<ListGenericUIAttribute>().AllowDerived))
                {
                    EditorGUILayoutX.DerivedTypeSelectButton(listType, (o) => target.Add(o));
                }
                else
                {
                    target.Add(Activator.CreateInstance(listType));
                }
            }
            return target;
        }
    }
}