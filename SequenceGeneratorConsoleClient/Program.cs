using SequenceGenerator;
using SequenceGenerator.SequenceConcurrents;
using SequenceGenerator.Sequences.Numeric;
using SequenceGenerator.Sequences.Template;
using SequenceGenerator.Storage.MongoDB;
using System.Security.Cryptography;

//ProcessNumericSequence();

//ProcessTemplateSequence();

//ProcessSequenceConcurrent();

//await ProcessSequenceConcurrentAsync();

//await ProcessMongoDBNumericSequence();

//await ProcessMongoDBSequenceConcurrentAsync();

//await ProcessMongoDBSequenceConcurrent();

static void ProcessNumericSequence()
{
    var config = new NumericSequenceRules(minValue: 3, maxValue: 5);

    ISequence<string> stringSequence = new StringNumericSequence(config);

    Console.WriteLine(stringSequence.Current);

    for(var i = 0; i < 10; i++)
    {
        Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} | current - {stringSequence.Current} | value - {stringSequence.Next()}");
    }
}

static void ProcessTemplateSequence()
{
    var rnd = new Random();

    ISequence<string> sequence = new StringTemplateSequence("[date]|[num]", new Dictionary<string, Func<string>>()
    {
        { "[date]", DateTime.Now.Date.Year.ToString },
        { "[num]",  () => rnd.Next().ToString() }
    });

    for(var i = 0; i < 5; i++)
    {
        Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} | current - {sequence.Current} | value - {sequence.Next()}");
        Thread.Sleep(1000);
    }
}

static void ProcessSequenceConcurrent()
{
    ISequence<string> stringNumericSequence = new StringNumericSequence();

    using ISequenceConcurrent<string> stringSequenceConcurrent = new SemaphoreSequenceConcurrent<string>(stringNumericSequence, 1);

    var threads = new List<Thread>();

    Console.WriteLine("Tasks creating.");

    for(var i = 0; i < 10; i++)
    {
        threads.Add(new Thread(() => AccessToSequence()));
    }

    threads.ForEach(thread => thread.Start());

    threads.ForEach(thread => thread.Join());

    Console.WriteLine("All tasks completed.");

    void AccessToSequence()
    {
        var value = stringSequenceConcurrent.Next();

        Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} | current - {stringSequenceConcurrent.Current} | value - {value}");
    }
}

static async Task ProcessSequenceConcurrentAsync()
{
    ISequence<string> stringNumericSequence = new StringNumericSequence();

    using ISequenceConcurrent<string> stringSequenceConcurrent = new SemaphoreSequenceConcurrent<string>(stringNumericSequence, 1);

    var tasks = new List<Task>();

    Console.WriteLine("Tasks creating.");

    for(var i = 0; i < 10; i++)
    {
        tasks.Add(AccessToSequenceAsync());
    }

    await Task.WhenAll(tasks);

    Console.WriteLine("All tasks completed.");

    async Task AccessToSequenceAsync()
    {
        var value = await stringSequenceConcurrent.NextAsync();

        Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} | current - {stringSequenceConcurrent.Current} | value - {value}");
    }
}

static async Task ProcessMongoDBNumericSequence()
{
    const string connectionString = "mongodb://root:example@localhost:27017";
    const string databaseName = "for-test";
    const string collectionName = "sequences";
    const string sequenceName = "numeric";

    var numericSequenceRules = new NumericSequenceRules(minValue: 3, maxValue: 100);

    ISequence<string> sequence = new StringNumericSequence(numericSequenceRules);

    var mongoDBSequenceStorage = new MongoDBSequenceStorage<string>(connectionString, databaseName, collectionName, sequence);

    await mongoDBSequenceStorage.LoadAsync(sequenceName);

    for(var i = 0; i < 150; i++)
    {
        Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} | current - {mongoDBSequenceStorage.Current} | value - {mongoDBSequenceStorage.Next()}");
    }

    await mongoDBSequenceStorage.SaveAsync(sequenceName);
}

static async Task ProcessMongoDBSequenceConcurrentAsync()
{
    const string connectionString = "mongodb://root:example@localhost:27017";
    const string databaseName = "for-test";
    const string collectionName = "sequences";
    const string sequenceName = "numeric";

    var numericSequenceRules = new NumericSequenceRules(minValue: 3, maxValue: 100);

    ISequence<string> sequence = new StringTemplateSequence("[date]|[num]", new Dictionary<string, Func<string>>()
    {
        { "[date]", DateTime.Now.Date.Year.ToString },
        { "[num]",  () => RandomNumberGenerator.GetInt32(1000).ToString() }
    });

    var mongoDBSequenceStorage = new MongoDBSequenceStorage<string>(connectionString, databaseName, collectionName, sequence, semaphoreInitialCount: 1);
    await mongoDBSequenceStorage.LoadAsync(sequenceName);

    var tasks = new List<Task>();

    Console.WriteLine("Tasks creating.");

    for(var i = 0; i < 10; i++)
    {
        tasks.Add(AccessToSequenceAsync(sequenceName, mongoDBSequenceStorage));
    }

    await Task.WhenAll(tasks);

    await mongoDBSequenceStorage.SaveAsync(sequenceName);

    Console.WriteLine("All tasks completed.");

    static Task AccessToSequenceAsync(string sequenceName, MongoDBSequenceStorage<string> mongoDBSequenceStorage)
    {
        Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} | current - {mongoDBSequenceStorage.Current} | value - {mongoDBSequenceStorage.Next()}");

        return Task.CompletedTask;
    }
}

static async Task ProcessMongoDBSequenceConcurrent()
{
    const string connectionString = "mongodb://root:example@localhost:27017";
    const string databaseName = "for-test";
    const string collectionName = "sequences";
    const string sequenceName = "numeric";

    var numericSequenceRules = new NumericSequenceRules(minValue: 3, maxValue: 100);

    //ISequence<string> sequence = new StringTemplateSequence("[date]|[num]", new Dictionary<string, Func<string>>()
    //{
    //    { "[date]", DateTime.Now.Date.Year.ToString },
    //    { "[num]",  () => RandomNumberGenerator.GetInt32(1000).ToString() }
    //});

    ISequence<string> sequence = new StringNumericSequence(numericSequenceRules);

    var mongoDBSequenceStorage = new MongoDBSequenceStorage<string>(connectionString, databaseName, collectionName, sequence, semaphoreInitialCount: 1);
    await mongoDBSequenceStorage.LoadAsync(sequenceName);

    var tasks = new List<Thread>();

    Console.WriteLine("Threads creating.");

    for(var i = 0; i < 10; i++)
    {
        tasks.Add(new Thread(() => AccessToSequence(sequenceName, mongoDBSequenceStorage)));
    }

    tasks.ForEach(thread => thread.Start());
    tasks.ForEach(thread => thread.Join());

    await mongoDBSequenceStorage.SaveAsync(sequenceName);

    Console.WriteLine("All threads completed.");

    static void AccessToSequence(string sequenceName, MongoDBSequenceStorage<string> mongoDBSequenceStorage)
    {
        Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} | current - {mongoDBSequenceStorage.Current} | value - {mongoDBSequenceStorage.Next()}");
    }
}