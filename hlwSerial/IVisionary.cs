namespace hlwSerial
{
    public interface IVisionary<T> : ISerializable where T : ISerializable

    {

    void ApplyVision(T item);

    }
}