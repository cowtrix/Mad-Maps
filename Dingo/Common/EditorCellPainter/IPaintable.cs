using System.Collections.Generic;

#if !HURTWORLDSDK
namespace Dingo.Common.Painter
{
    public interface IPaintable
    {
        void SetValue(int cell, float val);
        float GetValue(int cell);
        IEnumerator<KeyValuePair<int, float>> AllValues();
        void RemoveCell(int cell);
    }
}
#endif