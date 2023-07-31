namespace SequenceGenerator.SequenceConcurrents;

public interface ISequenceConcurrent<T> : ISequence<T>, IDisposable
{
    Task<T> NextAsync();
}