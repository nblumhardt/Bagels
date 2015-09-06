using System;

namespace Bagels.Messages
{
    public class PlaceOrderCommand
    {
        readonly Guid _orderId;
        readonly string _customerName;
        readonly string[] _itemIds;

        public PlaceOrderCommand(Guid orderId, string customerName, string[] itemIds)
        {
            _orderId = orderId;
            _customerName = customerName;
            _itemIds = itemIds;
        }

        public string CustomerName
        {
            get { return _customerName; }
        }

        public string[] ItemIds
        {
            get { return _itemIds; }
        }

        public Guid OrderId
        {
            get { return _orderId; }
        }
    }
}
