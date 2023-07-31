namespace SequenceGenerator;

public interface ISequence<T>
{
    T Current { get; }

    T Next();

    void SetCurrent(T elem);
}