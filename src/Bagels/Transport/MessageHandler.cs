namespace Bagels.Transport
{
    public abstract class MessageHandler<TMessage> : IMessageHandler
    {
        public void Handle(object message)
        {
            Handle((TMessage) message);
        }

        protected abstract void Handle(TMessage message);
    }
}