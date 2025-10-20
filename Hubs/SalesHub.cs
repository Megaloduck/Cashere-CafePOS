namespace CafePOS.API.Hubs
{
    using Microsoft.AspNetCore.SignalR;

    public class SalesHub : Hub
    {
        public async Task NotifySaleCompleted(string orderNumber, decimal amount)
        {
            await Clients.All.SendAsync("SaleCompleted", orderNumber, amount);
        }

        public async Task NotifyOrderStatusChanged(int orderId, string status)
        {
            await Clients.All.SendAsync("OrderStatusChanged", orderId, status);
        }
    }
}