namespace SequenceGenerator.Sequences.Numeric;
public class StringNumericSequence : ISequence<string>
{
    private readonly NumericSequenceRules _config;
    private int _current;

    public StringNumericSequence() : this(NumericSequenceRules.Default)
    {
    }

    public StringNumericSequence(NumericSequenceRules config)
    {
        _config = config;

        Reset();
    }

    public string Current => _current.ToString();

    public string Next()
    {
        if(CanMove())
        {
            _current++;
        }
        else
        {
            Reset();
        }

        return Current;
    }

    private void Reset()
    {
        _current = _config.MinValue;
    }

    private bool CanMove()
    {
        if(_current >= _config.MaxValue)
        {
            return false;
        }

        return true;
    }

    public void SetCurrent(string elem)
    {
        if(int.TryParse(elem, out var number))
        {
            _current = number;

            return;
        }

        throw new ArgumentException($"Failed to convert int '{elem}'", nameof(elem));
    }
}
