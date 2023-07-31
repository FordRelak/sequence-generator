using MongoDB.Driver;
using SequenceGenerator.SequenceConcurrents;

namespace SequenceGenerator.Storage.MongoDB;
public class MongoDBSequenceStorage<T>
    : ISequenceStorage
    , ISequence<T>
    , ISequenceStorageConcurrent<T>
{
    private readonly string _connectionString;
    private readonly string _databaseName;
    private readonly string _collectionName;
    private readonly ISequence<T> _sequence;
    private readonly ISequenceConcurrent<T> _sequenceConcurrent;
    private List<T> _sequenceElements = new();
    private IMongoClient _mongoClient;
    private IMongoDatabase _database;
    private readonly SemaphoreSlim _dbSemaphore = new(1);

    public MongoDBSequenceStorage(
        string connectionString,
        string databaseName,
        string collectionName,
        ISequence<T> sequence)
    {
        if(string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or whitespace.", nameof(connectionString));
        }

        if(string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException($"'{nameof(databaseName)}' cannot be null or whitespace.", nameof(databaseName));
        }

        if(string.IsNullOrWhiteSpace(collectionName))
        {
            throw new ArgumentException($"'{nameof(collectionName)}' cannot be null or whitespace.", nameof(collectionName));
        }

        _sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
        _connectionString = connectionString;
        _databaseName = databaseName;
        _collectionName = collectionName;
        InitMongoDB();
    }

    public MongoDBSequenceStorage(
        string connectionString,
        string databaseName,
        string collectionName,
        ISequence<T> sequence,
        int semaphoreInitialCount) : this(connectionString, databaseName, collectionName, sequence)
    {
        _sequenceConcurrent = new SemaphoreSequenceConcurrent<T>(sequence, semaphoreInitialCount);
    }

    public T Current => _sequence.Current;

    public T Next()
    {
        T elem;

        if(_sequenceConcurrent is not null)
        {
            elem = _sequenceConcurrent.Next();
        }
        else
        {
            elem = _sequence.Next();
        }

        _sequenceElements.Add(elem);

        return elem;
    }

    public void SetCurrent(T elem)
    {
        _sequence.SetCurrent(elem);
    }

    public void Load(string sequenceName)
    {
        LoadAsync(sequenceName).Wait();
    }

    public async Task LoadAsync(string sequenceName, CancellationToken cancellationToken = default)
    {
        await _dbSemaphore.WaitAsync(cancellationToken);

        try
        {
            var collection = _database.GetCollection<SequenceDocument<T>>(_collectionName);

            var filterByName = Builders<SequenceDocument<T>>.Filter.Eq(sequenceDocument => sequenceDocument.Name, sequenceName);

            var sequenceDocument = await collection.Find(filterByName).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if(sequenceDocument is null || !sequenceDocument.Elements.Any())
            {
                _sequenceElements.Add(_sequence.Current);

                return;
            }

            var lastSequenceElem = sequenceDocument.Elements.Last();

            _sequenceElements = sequenceDocument.Elements;
            _sequence.SetCurrent(lastSequenceElem);
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    public void Save(string sequenceName)
    {
        SaveAsync(sequenceName).Wait();
    }

    public async Task SaveAsync(string sequenceName, CancellationToken cancellationToken = default)
    {
        if(!_sequenceElements.Any())
        {
            return;
        }

        await _dbSemaphore.WaitAsync(cancellationToken);

        try
        {
            var collection = _database.GetCollection<SequenceDocument<T>>(_collectionName);
            var filterByName = Builders<SequenceDocument<T>>.Filter.Eq(sequenceDocument => sequenceDocument.Name, sequenceName);

            var sequenceDocument = await collection.Find(filterByName).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if(sequenceDocument is null)
            {
                sequenceDocument = new SequenceDocument<T>()
                {
                    Elements = _sequenceElements,
                    Name = sequenceName
                };

                await collection.InsertOneAsync(sequenceDocument, cancellationToken: cancellationToken);
            }
            else
            {
                sequenceDocument.Elements = _sequenceElements;

                var updateFilterByName = Builders<SequenceDocument<T>>.Update.Set(sequenceDocument => sequenceDocument.Elements, _sequenceElements);

                await collection.UpdateOneAsync(filterByName, updateFilterByName, cancellationToken: cancellationToken);
            }
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    private void InitMongoDB()
    {
        _mongoClient = new MongoClient(_connectionString);

        _database = _mongoClient.GetDatabase(_databaseName);
    }

    public Task<T> NextAsync()
    {
        if(_sequence is ISequenceConcurrent<T> sequenceConcurrent)
        {
            return sequenceConcurrent.NextAsync();
        }

        if(_sequenceConcurrent is not null)
        {
            return _sequenceConcurrent.NextAsync();
        }

        return Task.FromResult(_sequence.Next());
    }

    public void Dispose()
    {
        if(_sequence is IDisposable disposable)
        {
            disposable.Dispose();

            return;
        }

        _sequenceConcurrent.Dispose();
    }
}
