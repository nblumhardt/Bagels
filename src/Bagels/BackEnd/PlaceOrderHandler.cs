using System;
using Bagels.Messages;
using Bagels.Transport;
using InfluxDB.LineProtocol;
using Microsoft.Framework.Logging;

namespace Bagels.BackEnd
{
    public class PlaceOrderHandler : MessageHandler<PlaceOrderCommand>
    {
        readonly ILogger<PlaceOrderHandler> _log;

        public PlaceOrderHandler(ILogger<PlaceOrderHandler> log)
        {
            _log = log;
        }

        protected override void Handle(PlaceOrderCommand message)
        {
            _log.LogInformation("Placing order {OrderId} for {Items}", message.OrderId, message.ItemIds);

            if (message.ItemIds.Length == 0)
                throw new InvalidOperationException("Invalid order - at least one item must be included");

            Metrics.Increment("bagels_ordered", message.ItemIds.Length);
        }
    }
}
