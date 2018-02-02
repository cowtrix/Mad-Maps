namespace MadMaps.Roads
{
    public interface IOnPrebakeCallback
    {
        int GetPriority();
        void OnPrebake();
    }
}