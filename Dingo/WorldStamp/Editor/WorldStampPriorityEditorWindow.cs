using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dingo.WorldStamp
{
    public class WorldStampPriorityEditorWindow : EditorWindow
    {
        private ReorderableList _listGUI;
        private List<WorldStamp> _list;
        private Vector2 _scroll;

        private int _lastPriority;
        private bool _needsResort;

        void OnEnable()
        {
            _listGUI = new ReorderableList(_list, typeof(WorldStamp), false, false, false, false);
            _listGUI.drawElementCallback += DrawElementCallback;
            Sort();
        }
        
        void Sort()
        {
            _list = new List<WorldStamp>(FindObjectsOfType<WorldStamp>()).OrderBy(stamp => stamp.Priority).ThenBy((stamp => stamp.transform.GetSiblingIndex())).ToList();
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            rect.height -= 2;
            var nameRect = new Rect(rect.x, rect.y, rect.width*.6f, rect.height);
            GUI.Label(nameRect, _list[index].name);

            var selectRect = new Rect(nameRect.xMax + 4, rect.y, 30, rect.height-2);
            var selectContent = EditorGUIUtility.IconContent("PreMatCube");
            selectContent.tooltip = "Select In Hierarchy";
            if (GUI.Button(selectRect, selectContent, EditorStyles.toolbarButton))
            {
                Selection.activeGameObject = _list[index].gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }

            var editRect = new Rect(selectRect.xMax + 4, rect.y, rect.width*.4f - 8 - 30, rect.height-4);
            var newPriority = EditorGUI.DelayedIntField(editRect, _list[index].Priority);
            if (newPriority != _list[index].Priority)
            {
                _list[index].Priority = newPriority;
                Sort();
            }

            if (index > 0 && _list[index].Priority < _lastPriority)
            {
                _needsResort = true;
            }
            _lastPriority = _list[index].Priority;
        }

        void OnGUI()
        {
            if (_needsResort)
            {
                _needsResort = false;
                Sort();
            }

            if (_listGUI == null || _listGUI.list != _list)
            {
                OnEnable();
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _lastPriority = int.MinValue;
            _listGUI.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }

        void OnSelectionChange()
        {
            if (Selection.activeGameObject == null)
            {
                return;
            }
            var stamp = Selection.activeGameObject.GetComponent<WorldStamp>();
            var index = _list.IndexOf(stamp);

            if (index >= 0)
            {
                _listGUI.index = index;
            }
        }
    }
}