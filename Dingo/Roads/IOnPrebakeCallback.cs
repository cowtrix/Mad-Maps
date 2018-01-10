namespace Dingo.Roads
{
    public interface IOnPrebakeCallback
    {
        int GetPriority();
        void OnPrebake();
    }
}