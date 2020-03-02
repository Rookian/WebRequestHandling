using System.Threading.Tasks;
using WebRequestHandling.Infrastructure.ExecutionPipeline;

namespace WebRequestHandling.Features.Order
{
    public class CreateOrder : ICommandRequest<CreateOrderResponse>
    {
        public int Amount { get; set; }
    }

    public class CreateOrderResponse
    {
        public string Message { get; set; }
    }

    public class CreateOrderHandler : ICommandRequestHandler<CreateOrder, CreateOrderResponse>
    {
        public Task<CreateOrderResponse> Handle(CreateOrder request)
        {
            return Task.FromResult(new CreateOrderResponse { Message = $"Order with amount of {request.Amount} created." });
        }
    }
}