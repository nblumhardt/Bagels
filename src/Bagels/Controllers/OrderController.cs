using Bagels.Messages;
using Bagels.Transport;
using Bagels.ViewModels.Order;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;
using System;
using System.Linq;

namespace Bagels.Controllers
{
    public class OrderController : Controller
    {
        readonly IMessageBus _messageBus;
        readonly ILogger _log;

        public OrderController(IMessageBus messageBus, ILogger<OrderController> log)
        {
            _messageBus = messageBus;
            _log = log;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Place(OrderViewModel order)
        {
            var orderId = Guid.NewGuid();
            var itemIds = new[] { order.ItemId1, order.ItemId2 }.Where(i => !string.IsNullOrEmpty(i)).ToArray();

            _log.LogInformation("Placing order {OrderId} ({ItemCount} items) for {CustomerName}", orderId, itemIds.Length, order.CustomerName);

            var command = new PlaceOrderCommand(orderId, order.CustomerName, itemIds);
            _messageBus.Publish(command);

            return RedirectToAction(nameof(Thanks));
        }

        [HttpGet]
        public IActionResult Thanks()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
