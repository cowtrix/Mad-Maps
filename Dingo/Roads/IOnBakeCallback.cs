namespace Dingo.Roads
{
    public interface IOnBakeCallback
    {
        int GetPriority();
        void OnBake();
    }
}