using UnityEngine;

namespace Dingo.Common.Painter
{
    [SDKScript(Full = true)]
    public abstract class BaseBrush : IBrush
    {
        public abstract bool Paint(float dt, IPaintable canvas, IGridManager gridManager, Painter.InputState inputState, float maxValue, float maxVal, Rect rect, Matrix4x4 TRS);

        public void DrawGizmos(IGridManager gridManager, Painter.InputState inputState, Rect rect, Matrix4x4 TRS)
        {
            if (!inputState.MouseBlocked)
            {
                DrawSceneGizmos(gridManager, inputState, rect, TRS);
            }
        }

        protected abstract void DrawSceneGizmos(IGridManager gridManager, Painter.InputState inputState, Rect rect, Matrix4x4 TRS);

        public void DrawGUI()
        {
            DrawSpecificGUI();
        }

        public abstract void DrawSpecificGUI();
    }
}