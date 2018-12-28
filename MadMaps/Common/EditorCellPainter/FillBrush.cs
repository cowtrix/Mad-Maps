using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

namespace MadMaps.Common.Painter
{

    public class FillBrush : MadMaps.Common.Painter.BaseBrush, MadMaps.Common.Painter.IBrush
    {
        private double _lastPaint;
        Vector2 Range = new Vector2(0, 1);
        public float Strength = 1;
        Queue<int> _openCells = new Queue<int>();
        HashSet<int> _closedCells = new HashSet<int>();

        List<int> GetCells(MadMaps.Common.Painter.IGridManager gridManager, Rect rect, MadMaps.Common.Painter.IPaintable canvas, 
            MadMaps.Common.Painter.Painter.InputState inputState)
        {
            var cells = new List<int>();
            _openCells.Clear();
            _closedCells.Clear();
            _openCells.Enqueue(gridManager.GetCell(inputState.GridPosition));
            var gridSize = gridManager.GetGridSize();
            
            while(_openCells.Count > 0)
            {
                var nextCell = _openCells.Dequeue();
                var cellCenter = gridManager.GetCellCenter(nextCell);

                if (_closedCells.Contains(nextCell))
                {
                    //Debug.Log("Skipped cell" + nextCell);
                    continue;
                }
                if(rect.size.magnitude > 0 && !rect.Contains(cellCenter))
                {
                    //Debug.LogFormat("Skipped cell {0} as {1} wasnt' inside {2}", nextCell, cellCenter, rect);
                    _closedCells.Add(nextCell);
                    continue;
                }
                _closedCells.Add(nextCell);
                var value = canvas.GetValue(nextCell);
                if(value < Range.x || value > Range.y)
                {
                    //Debug.LogFormat("Skipped cell {0} for range {1} as val was {2}", nextCell, Range, value);
                    continue;
                }
                cells.Add(nextCell);
                var gridPos = gridManager.GetCellCenter(nextCell).x0z();
                
                _openCells.Enqueue(gridManager.GetCell(gridPos + new Vector3(-1 * gridSize, 0, 0)));
                _openCells.Enqueue(gridManager.GetCell(gridPos + new Vector3(1 * gridSize, 0, 0)));
                _openCells.Enqueue(gridManager.GetCell(gridPos + new Vector3(0, 0, 1 * gridSize)));
                _openCells.Enqueue(gridManager.GetCell(gridPos + new Vector3(0, 0, -1 * gridSize)));
            }
            return cells;
        }

        public override bool Paint(float dt, MadMaps.Common.Painter.IPaintable canvas, 
            MadMaps.Common.Painter.IGridManager gridManager, MadMaps.Common.Painter.Painter.InputState inputState, 
            float minVal, float maxVal, Rect rect, Matrix4x4 TRS)
        {
            var allCells = GetCells(gridManager, rect, canvas, inputState);
            foreach(var cell in allCells)
            {
                canvas.SetValue(cell, Mathf.Clamp(Strength, minVal, maxVal));
            }
            return allCells.Count > 0;
        }

        public override void DrawSpecificGUI()
        {
            Strength = Mathf.Max(0, EditorGUILayout.FloatField("Strength", Strength));
            EditorGUILayout.MinMaxSlider("Range", ref Range.x, ref Range.y, 0, 1);
        }

        protected override void DrawSceneGizmos(MadMaps.Common.Painter.IPaintable canvas, 
            MadMaps.Common.Painter.IGridManager gridManager, 
            MadMaps.Common.Painter.Painter.InputState inputState, Rect rect, Matrix4x4 TRS)
        {
            Handles.matrix = TRS;
            var scaledRad = /*gridManager.GetGridSize()*/.5f;

            //var translatedPlanePos = /*TRS.MultiplyPoint */(inputState.PlanePosition);
            var translatedGridPos = /*TRS.MultiplyPoint */(inputState.GridPosition);
            var planeUp = /*TRS.GetRotation()**/Vector3.up;
            var planeForward = /*TRS.GetRotation()**/Vector3.forward;
            var planeRot = Quaternion.LookRotation(planeUp, planeForward);

            Handles.color = Color.red*0.5f;
            var allCells = GetCells(gridManager, rect, canvas, inputState);
            foreach(var cell in allCells)
            {
                var cellCenter = gridManager.GetCellCenter(cell).x0z(inputState.PlanePosition.y);
                Handles.RectangleHandleCap(-1, cellCenter, planeRot,
                    gridManager.GetGridSize()*Mathf.Max(.5f, scaledRad), EventType.Repaint);
            }
            Handles.color = Color.white;
            Handles.RectangleHandleCap(-1, translatedGridPos, planeRot,
                    gridManager.GetGridSize()*Mathf.Max(.5f, scaledRad), EventType.Repaint);
            Handles.matrix = Matrix4x4.identity;
        }
    }
}
#endif