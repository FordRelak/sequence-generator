using System.Text;

namespace SequenceGenerator.Sequences.Template;
public class StringTemplateSequence : ISequence<string>
{
    private readonly string _template;
    private readonly Dictionary<string, Func<string>> _partOfSequenceByTemplateKey;
    private readonly StringBuilder _sb = new();

    public StringTemplateSequence(string template, Dictionary<string, Func<string>> partOfSequenceByTemplateKey)
    {
        if(string.IsNullOrWhiteSpace(template))
        {
            throw new ArgumentException($"'{nameof(template)}' cannot be null or whitespace.", nameof(template));
        }

        if(partOfSequenceByTemplateKey is null || !partOfSequenceByTemplateKey.Any())
        {
            throw new ArgumentException($"'{nameof(partOfSequenceByTemplateKey)}' cannot be null or empty.", nameof(partOfSequenceByTemplateKey));
        }

        _template = template;
        _partOfSequenceByTemplateKey = partOfSequenceByTemplateKey;

        InitCurrentValue();
    }

    private void InitCurrentValue()
    {
        _sb.Append(_template);

        FillTemplate();
    }

    private void FillTemplate()
    {
        foreach(var partOfSequenceByTemplateKey in _partOfSequenceByTemplateKey)
        {
            var valuePartSequence = partOfSequenceByTemplateKey.Value.Invoke() ?? string.Empty;

            _sb.Replace(partOfSequenceByTemplateKey.Key, valuePartSequence);
        }
    }

    public string Current => _sb.ToString();

    public string Next()
    {
        _sb.Clear();
        _sb.Append(_template);

        FillTemplate();

        return _sb.ToString();
    }

    public void SetCurrent(string elem)
    {
        if(string.IsNullOrEmpty(elem))
        {
            throw new ArgumentException($"'{nameof(elem)}' cannot be null or whitespace.", nameof(elem));
        }

        _sb.Clear();
        _sb.Append(elem);
    }
}
