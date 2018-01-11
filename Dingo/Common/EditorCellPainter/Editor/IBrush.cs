
using UnityEngine;

namespace Dingo.Common.Painter
{
    [SDKScript(Full = true)]
    public interface IBrush
    {
        void DrawGizmos(IGridManager gridManager, Painter.InputState inputState, Rect rect, Matrix4x4 TRS);
        void DrawGUI();
        bool Paint(float dt, IPaintable canvas, IGridManager gridManager, Painter.InputState currentInputState, float minValue, float maxValue, Rect rect, Matrix4x4 TRS);
    }
}