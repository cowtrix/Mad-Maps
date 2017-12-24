using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AutoEditorWrapper
{
#if UNITY_EDITOR
    public static object ShowAutoEditorGUI(object o)
    {
        throw new Exception();
        //return EditorUtils.ShowAutoEditorGUI(o);
    }

    public static IList ListEditorNicer(
        string prefix,
        IList list,
        Type listType,
        object contextInstance,
        bool allowDerived = false,
        bool reordable = false,
        bool forceLabel = false)
    {
        throw new Exception();
        //return EditorUtils.ListEditorNicer(prefix, list, listType, contextInstance, allowDerived, reordable, forceLabel);
    }


#endif
}
