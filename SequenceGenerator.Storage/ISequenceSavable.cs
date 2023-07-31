namespace SequenceGenerator.Storage;

public interface ISequenceSavable
{
    void Save(string sequenceName);

    Task SaveAsync(string sequenceName, CancellationToken cancellationToken = default);
}
