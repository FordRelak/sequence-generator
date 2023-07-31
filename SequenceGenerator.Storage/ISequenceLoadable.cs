namespace SequenceGenerator.Storage;

public interface ISequenceLoadable
{
    void Load(string sequenceName);

    Task LoadAsync(string sequenceName, CancellationToken cancellationToken = default);
}