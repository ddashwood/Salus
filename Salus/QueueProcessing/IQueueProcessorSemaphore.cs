namespace Salus.QueueProcessing;

public interface IQueueProcessorSemaphore
{
    /// <summary>
    /// Indicates that a process wants to start queue processing.
    /// </summary>
    /// <returns>True if no other process is processing the queue; otherwise, false.</returns>
    bool Start();

    /// <summary>
    /// Indicates that a process has stopped queue processing.
    /// </summary>
    void Stop();
}
