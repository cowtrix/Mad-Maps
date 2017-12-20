#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Dingo.Common;
using Dingo.Terrains;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[Serializable]
public class SwapPair<T>
{
    public List<T> From = new List<T>();
    public T To;
}

public class WorldStampSwapPrefabEditorWindow : EditorWindow
{
    public List<Dingo.WorldStamp.WorldStamp> StampList = new List<Dingo.WorldStamp.WorldStamp>();
    public List<SwapPair<GameObject>> ObjectsToSwap = new List<SwapPair<GameObject>>();
    public List<SwapPair<SplatPrototypeWrapper>> SplatsToSwap = new List<SwapPair<SplatPrototypeWrapper>>();
    public List<SwapPair<DetailPrototypeWrapper>> DetailsToSwap = new List<SwapPair<DetailPrototypeWrapper>>();

    private Vector2 _scroll;

    [MenuItem("Tools/Level/Stamp Object Swapper")]
    public static void FindReferencesInPrefabs()
    {
        GetWindow<WorldStampSwapPrefabEditorWindow>();
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        EditorUtils.TitledSeparator("Basic Info");
        EditorUtils.ShowAutoEditorGUI(this);
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("Go"))
        {
            Apply();
        }

        if (GUILayout.Button("JSONCopy"))
        {
            Debug.Log(JsonUtility.ToJson(this));
        }
    }

    private void Apply()
    {
        foreach (var worldStamp in StampList)
        {
            if (worldStamp == null) continue;

            foreach (var swapPair in ObjectsToSwap)
            {
                if (!(swapPair != null && swapPair.From != null && swapPair.To != null))
                {
                    continue;
                }

                foreach (var fromObject in swapPair.From)
                {
                    if (fromObject == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < worldStamp.Data.Objects.Count; i++)
                    {
                        var prefabObjectData = worldStamp.Data.Objects[i];
                        if (fromObject == prefabObjectData.Prefab)
                        {
                            prefabObjectData.Prefab = swapPair.To;
                        }

                        worldStamp.Data.Objects[i] = prefabObjectData;
                        EditorUtility.SetDirty(worldStamp);
                    }
                }
            }

            foreach (var swapPair in SplatsToSwap)
            {
                if (!(swapPair != null && swapPair.From != null && swapPair.To != null))
                {
                    continue;
                }

                foreach (var fromObject in swapPair.From)
                {
                    if (fromObject == null || fromObject == swapPair.To)
                    {
                        continue;
                    }

                    var item = worldStamp.Data.SplatData.FirstOrDefault(x => x.Wrapper == fromObject);

                    if (item != null)
                    {
                        item.Wrapper = swapPair.To;
                        EditorUtility.SetDirty(worldStamp);
                    }
                }
            }

            foreach (var swapPair in DetailsToSwap)
            {
                if (!(swapPair != null && swapPair.From != null && swapPair.To != null))
                {
                    continue;
                }

                foreach (var fromObject in swapPair.From)
                {
                    if (fromObject == null || fromObject == swapPair.To)
                    {
                        continue;
                    }

                    var item = worldStamp.Data.DetailData.FirstOrDefault(x => x.Wrapper == fromObject);

                    if (item != null)
                    {
                        item.Wrapper = swapPair.To;
                        EditorUtility.SetDirty(worldStamp);
                    }
                }
            }
        }
    }
}
#endif