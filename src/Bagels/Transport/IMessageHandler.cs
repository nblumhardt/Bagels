namespace Bagels.Transport
{
    public interface IMessageHandler
    {
        void Handle(object message);
    }
}
