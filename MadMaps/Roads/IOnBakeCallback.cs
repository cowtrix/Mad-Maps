namespace MadMaps.Roads
{
    public interface IOnBakeCallback
    {
        int GetPriority();
        void OnBake();
    }
}