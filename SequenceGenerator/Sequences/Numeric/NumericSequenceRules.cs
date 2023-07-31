namespace SequenceGenerator.Sequences.Numeric;

public class NumericSequenceRules
{
    private readonly int _minValue;
    private readonly int _maxValue;

    public NumericSequenceRules() : this(Default.MinValue, Default.MaxValue)
    {
    }

    public NumericSequenceRules(int minValue, int maxValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
    }

    public int MinValue => _minValue;

    public int MaxValue => _maxValue;

    public static NumericSequenceRules Default => new(0, int.MaxValue);
}