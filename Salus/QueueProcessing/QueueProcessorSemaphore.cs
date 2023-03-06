namespace Salus.QueueProcessing;

internal class QueueProcessorSemaphore : IQueueProcessorSemaphore, IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    public QueueProcessorSemaphore()
    {
        _semaphore = new SemaphoreSlim(1);
    }

    public bool Start()
    {
        return _semaphore.Wait(0);
    }

    public void Stop()
    {
        _semaphore.Release();
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}
