namespace Salus.Purging;

public interface IPurgerSemaphore
{
    /// <summary>
    /// Indicates that a process wants to start purging.
    /// </summary>
    /// <returns>True if no other process is purging; otherwise, false.</returns>
    bool Start();

    /// <summary>
    /// Indicates that a process has stopped purging.
    /// </summary>
    void Stop();
}
