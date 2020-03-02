using System;
using System.Threading.Tasks;
using WebRequestHandling.Infrastructure.ExecutionPipeline;

namespace WebRequestHandling.Features.Order
{
    public class GetOrdersById : IQueryRequest<OrderResponse>
    {
        public string Id { get; set; }
    }

    public class OrderResponse
    {
        public string Message { get; set; }
        public Order Order { get; set; }
    }

    public class Order
    {
        public string Id { get; set; }
        public DateTimeOffset OrderDate { get; set; }
    }

    public class GetOrdersByRequestQueryRequestHandler : IQueryRequestHandler<GetOrdersById, OrderResponse>
    {
        public Task<OrderResponse> Handle(GetOrdersById getOrdersById)
        {
            return Task.FromResult(new OrderResponse { Message = getOrdersById.Id + " handled." });
        }
    }
}