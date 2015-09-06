namespace Bagels.Transport
{
    public interface IMessageBus
    {
        void Publish(object message);
    }
}
