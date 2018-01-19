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

    public class CurveDrawer : GenericDrawer<AnimationCurve>
    {
        protected override AnimationCurve DrawGUIInternal(AnimationCurve target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return EditorGUILayout.CurveField(label, target);
        }
    }

    public class EnumDrawer : GenericDrawer<System.Enum>
    {
        protected override Enum DrawGUIInternal(Enum target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return EditorGUILayout.EnumPopup(label, target);
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

    public class BoolDrawer : GenericDrawer<bool>
    {
        protected override bool DrawGUIInternal(bool target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return EditorGUILayout.Toggle(label, target);
        }
    }

    public class Vector2Drawer : GenericDrawer<Vector2>
    {
        protected override Vector2 DrawGUIInternal(Vector2 target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return EditorGUILayout.Vector2Field(label, target);
        }
    }

    public class Vector3Drawer : GenericDrawer<Vector3>
    {
        protected override Vector3 DrawGUIInternal(Vector3 target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return EditorGUILayout.Vector3Field(label, target);
        }
    }

    public class Vector4Drawer : GenericDrawer<Vector4>
    {
        protected override Vector4 DrawGUIInternal(Vector4 target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return EditorGUILayout.Vector4Field(label, target);
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

    public class Vec3MinMaxDrawer : GenericDrawer<Vec3MinMax>
    {
        protected override Vec3MinMax DrawGUIInternal(Vec3MinMax target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            EditorGUILayout.LabelField("Min", GUILayout.Width(64));
            target.Min = EditorGUILayout.Vector3Field("", target.Min);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(150));
            EditorGUILayout.LabelField("Max", GUILayout.Width(64));
            target.Max = EditorGUILayout.Vector3Field("", target.Max);
            EditorGUILayout.EndHorizontal();
            return target;
        }
    }

    public class FloatMinMaxDrawer : GenericDrawer<FloatMinMax>
    {
        protected override FloatMinMax DrawGUIInternal(FloatMinMax target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            EditorGUILayout.LabelField("Min", GUILayout.Width(64));
            target.Min = EditorGUILayout.FloatField(target.Min);
            EditorGUILayout.LabelField("Max", GUILayout.Width(64));
            target.Max = EditorGUILayout.FloatField(target.Max);
            EditorGUILayout.EndHorizontal();
            return target;
        }
    }

    public class LayerMaskDrawer : GenericDrawer<LayerMask>
    {
        protected override LayerMask DrawGUIInternal(LayerMask target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            return LayerMaskFieldUtility.LayerMaskField(label, target, true);
        }
    }

    public class UnityObjectDrawer : GenericDrawer<UnityEngine.Object>
    {
        protected override UnityEngine.Object DrawGUIInternal(UnityEngine.Object target, string label = "", Type targetType = null, FieldInfo fieldInfo = null,
            object context = null)
        {
            var objType = typeof(UnityEngine.Object);
            if (targetType != null)
            {
                objType = targetType;
            }
            else if (fieldInfo != null)
            {
                objType = fieldInfo.FieldType;
            }
            return EditorGUILayout.ObjectField(label, target, objType, true);
        }
    }
    
    public class IListDrawer : GenericDrawer<IList>
    {
        protected override IList DrawGUIInternal(IList target, string label = "", Type targetType = null,
            FieldInfo fieldInfo = null,
            object context = null)
        {
            var expandedKey = fieldInfo != null ? context + fieldInfo.Name : context + "list";
            var listType = targetType.IsArray ? targetType.GetElementType() : targetType.GetGenericArguments()[0];

            if (!GenericEditor.ExpandedFieldCache.ContainsKey(expandedKey))
            {
                GenericEditor.ExpandedFieldCache[expandedKey] = false;
            }
            if (fieldInfo != null)
            {
                GenericEditor.ExpandedFieldCache[expandedKey] =
                    EditorGUILayout.Foldout(GenericEditor.ExpandedFieldCache[expandedKey], fieldInfo.Name, EditorStyles.boldFont);
            }

            if (fieldInfo == null || GenericEditor.ExpandedFieldCache[expandedKey])
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < target.Count; i++)
                {
                    var o = target[i];
                    if (!GenericEditor.ExpandedFieldCache.ContainsKey(expandedKey + i))
                    {
                        GenericEditor.ExpandedFieldCache[expandedKey + i] = false;
                    }

                    if (typeof (UnityEngine.Object).IsAssignableFrom(listType))
                    {
                        EditorGUILayout.BeginHorizontal();
                        o = EditorGUILayout.ObjectField((UnityEngine.Object)o, listType, true);
                        if (GUILayout.Button(GenericEditor.DeleteContent, EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            if (targetType.IsArray)
                            {
                                target = ((Array)target).Remove(i);
                            }
                            else
                            {
                                target.RemoveAt(i);
                            }
                            break;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        GenericEditor.ExpandedFieldCache[expandedKey + i] = EditorGUILayout.Foldout(GenericEditor.ExpandedFieldCache[expandedKey + i],
                            o != null ? string.Format("[{0}] {1}", i, o) : "Null");
                        if (GUILayout.Button(GenericEditor.DeleteContent, EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            target.RemoveAt(i);
                            break;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (GenericEditor.ExpandedFieldCache[expandedKey + i])
                        {
                            if (o != null)
                            {
                                o = GenericEditor.DrawGUI(o, o.ToString(), o.GetType(), fieldInfo, target);
                            }
                            else
                            {
                                o = GenericEditor.DrawGUI(null, "Null", listType, fieldInfo, target);
                            }
                        }
                    }
                    
                    target[i] = o;
                    EditorExtensions.Seperator();
                }
                EditorGUI.indentLevel--;
                
                if (listType.IsAbstract || listType.IsInterface || (fieldInfo != null && fieldInfo.HasAttribute<ListGenericUIAttribute>() && fieldInfo.GetAttribute<ListGenericUIAttribute>().AllowDerived))
                {
                    EditorGUILayoutX.DerivedTypeSelectButton(listType, (o) => target.Add(o));
                }
                else if(EditorGUILayoutX.IndentedButton("Add " + listType.Name))
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(listType))
                    {
                        if (targetType.IsArray)
                        {
                            target = ((Array) target).Add(null);
                        }
                        else
                        {
                            target.Add(null);
                        }
                    }
                    else
                    {
                        var newInstance = Activator.CreateInstance(listType);
                        if (newInstance != null && !newInstance.Equals(null))
                        {
                            if (targetType.IsArray)
                            {
                                target = ((Array)target).Add(newInstance);
                            }
                            else
                            {
                                target.Add(newInstance);
                            }
                        }
                    }
                }
            }
            return target;
        }
    }
}