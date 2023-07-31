using SequenceGenerator.SequenceConcurrents;

namespace SequenceGenerator.Storage;
public interface ISequenceStorage : ISequenceLoadable, ISequenceSavable
{
}

public interface ISequenceStorageConcurrent<T> : ISequenceStorage, ISequenceConcurrent<T>
{

}
