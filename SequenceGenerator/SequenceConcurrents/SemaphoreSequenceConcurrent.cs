namespace SequenceGenerator.SequenceConcurrents;
public class SemaphoreSequenceConcurrent<T> :
      ISequence<T>
    , ISequenceConcurrent<T>
    , IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ISequence<T> _sequence;

    public SemaphoreSequenceConcurrent(ISequence<T> sequence, SemaphoreSlim semaphore)
    {
        _sequence = sequence;
        _semaphore = semaphore;
    }

    public SemaphoreSequenceConcurrent(ISequence<T> sequence, int initialCount) : this(sequence, new SemaphoreSlim(initialCount))
    {
    }

    public T Current => _sequence.Current;

    public T Next()
    {
        _semaphore.Wait();

        try
        {
            return _sequence.Next();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<T> NextAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            return _sequence.Next();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }

    public void SetCurrent(T elem)
    {
        _sequence.SetCurrent(elem);
    }
}
