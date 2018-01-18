using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Dingo.Common.Painter
{
    public class BlurBrush : BaseBrush, IBrush
    {
        public int Radius = 2;
        public float Strength = 1;
        public float Flow = 3;
        public int Iterations = 1;
        public AnimationCurve Falloff = new AnimationCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });

        public override void DrawSpecificGUI()
        {
            Strength = Mathf.Max(0, EditorGUILayout.Slider("Strength", Strength, 0, 1));
            Flow = Mathf.Max(0, EditorGUILayout.FloatField("Flow", Flow));
            Radius = Mathf.Max(0, EditorGUILayoutX.IntSlider("Radius", Radius, 0, 10));
        }

        protected override void DrawSceneGizmos(IGridManager gridManager, Painter.InputState inputState, Rect rect, Matrix4x4 TRS)
        {
            var gridSize = gridManager.GetGridSize();
            Handles.color = Color.white*0.5f;
            Handles.CircleHandleCap(-1, inputState.PlanePosition, Quaternion.LookRotation(Vector3.up), gridSize * Radius, EventType.Repaint);
            var scaledRad = gridSize * Radius;
            for (var i = -scaledRad; i <= scaledRad; i += gridSize)
            {
                for (var j = -scaledRad; j <= scaledRad; j += gridSize)
                {
                    var pos = inputState.GridPosition + new Vector3(i, 0, j);
                    var circleDist = Vector2.Distance(inputState.GridPosition.xz(), pos.xz());
                    if (circleDist > scaledRad)
                    {
                        continue;
                    }
                    Handles.RectangleHandleCap(-1, pos, Quaternion.LookRotation(Vector3.up), gridSize / 2, EventType.Repaint);
                }
            }
            Handles.color = Color.white;
            Handles.CircleHandleCap(-1, inputState.GridPosition, Quaternion.LookRotation(Vector3.up), gridSize * Radius, EventType.Repaint);
        }

        public override bool Paint(float dt, IPaintable canvas, IGridManager gridManager, Painter.InputState inputState, float minVal, float maxVal, Rect rect, Matrix4x4 TRS)
        {
            bool dirty = false;

            var gridSize = gridManager.GetGridSize();
            var scaledRad = gridSize * Radius;
            
            for (var i = -scaledRad; i <= scaledRad; i += gridSize)
            {
                for (var j = -scaledRad; j <= scaledRad; j += gridSize)
                {
                    var pos = inputState.GridPosition + new Vector3(i, 0, j);
                    if (!rect.Contains(pos.xz()))
                    {
                        continue;
                    }

                    var circleDist = Vector2.Distance(inputState.GridPosition.xz(), pos.xz());
                    if (circleDist > scaledRad)
                    {
                        continue;
                    }

                    var normalisedDist = circleDist / scaledRad;
                    var falloff = 1f;
                    if (!float.IsNaN(normalisedDist))
                    {
                        falloff = Falloff.Evaluate(normalisedDist);
                    }

                    var baseCell = gridManager.GetCell(pos);
                    float existingVal = canvas.GetValue(baseCell);
                    
                    for (var k = 0; k < Iterations; ++k)
                    {
                        var sum = 0f;
                        for (var u = -1; u <= 1; ++u)
                        {
                            for (var v = -1; v <= 1; ++v)
                            {
                                var cell = gridManager.GetCell(pos + new Vector3(u*gridSize, 0, v*gridSize));
                                sum += canvas.GetValue(cell);
                            }
                        }
                        canvas.SetValue(baseCell, Mathf.Lerp(existingVal, sum / 9f, falloff) * dt * Flow);
                    }

                    dirty = true;
                }
            }
            return dirty;
        }
    }
}
#endif
