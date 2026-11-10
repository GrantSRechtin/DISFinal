public interface IRewindable
{
    void OnTimeStarted();
    void OnTimeStopped();
    void OnStoreTimestep();
    void OnRewindTimestep(int steps);
}