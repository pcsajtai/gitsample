
namespace ClassLibrary1
{
    public interface IMessageRecievier<T>
    {
        void Recieve(T message);
    }
}
