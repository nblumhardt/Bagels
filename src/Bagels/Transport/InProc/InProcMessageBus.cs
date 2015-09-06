using System;
using System.Collections.Concurrent;
using System.Threading;
using Serilog;
using Serilog.Context;
using InfluxDB.LineProtocol;
using System.Collections.Generic;

namespace Bagels.Transport.InProc
{
    public class InProcMessageBus : IMessageBus, IDisposable
    {
        readonly IServiceProvider _handlers;
        readonly ILogger _log = Log.ForContext<InProcMessageBus>();
        readonly ConcurrentQueue<object> _messages = new ConcurrentQueue<object>();
        readonly Thread _worker;
        volatile bool _stopping;

        // Autofac's IIndex<,> type useful here...
        public InProcMessageBus(IServiceProvider handlers)
        {
            if (handlers == null) throw new ArgumentNullException("handlers");
            _handlers = handlers;
            _worker = new Thread(Run);
        }

        public void Start()
        {
            _log.Debug("Starting message bus");
            _worker.Start();
        }

        public void Publish(object message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _messages.Enqueue(message);

            Metrics.Increment("message_published", tags: new Dictionary<string, string>
            {
                { "message_type", message.GetType().Name }
            });
        }

        void Run()
        {
            while (!_stopping)
            {
                object message;
                if (_messages.TryDequeue(out message))
                {
                    using (LogContext.PushProperty("MessageId", Guid.NewGuid()))
                    {
                        var messageType = message.GetType();
                        Log.Debug("Processing message of type {MessageType}", messageType);
                        try
                        {
                            var handler = (IMessageHandler)_handlers.GetService(typeof(MessageHandler<>).MakeGenericType(messageType));

                            using (Metrics.Time("message_handled", tags: new Dictionary<string, string>
                            {
                                { "message_type", message.GetType().Name },
                                { "handler_type", handler.GetType().Name }
                            }))
                            {
                                handler.Handle(message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex, "Failed to process message");
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void Dispose()
        {
            _log.Debug("Shutting down message bus");
            _stopping = true;
            _worker.Join();
        }
    }
}
