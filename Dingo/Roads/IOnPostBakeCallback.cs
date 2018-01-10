namespace Dingo.Roads
{
    public interface IOnPostBakeCallback
    {
        int GetPriority();
        void OnPostBake();
    }
}