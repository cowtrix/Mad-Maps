using UnityEngine;

#if !HURTWORLDSDK
namespace Dingo.Common.Painter
{
    public interface IGridManager
    {
        int GetCell(Vector3 position);
        int GetOffset(int x, int y);
        Vector2 GetCellMin(Vector3 pos);
        Vector2 GetCellMin(int cell);
        Vector2 GetCellMax(Vector3 pos);
        Vector2 GetCellMax(int cell);
        Vector2 GetCellCenter(int cell);
        Vector2 GetCellCenter(Vector3 pos);
        Vector2 GetRandomPointInCell(int cell);
        float GetGridSize();
    }
}
#endif