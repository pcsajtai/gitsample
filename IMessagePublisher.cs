
namespace ClassLibrary1
{
    public interface IMessagePublisher
    {
        void Subscribe<TMessage, TSubcriber>(TSubcriber messageReciever) where TSubcriber : IMessageRecievier<TMessage>;
        void Broadcast<TMessage>(TMessage message);
    }
}
