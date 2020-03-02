using System.Threading.Tasks;

namespace WebRequestHandling.Infrastructure.ExecutionPipeline
{
    public interface IQueryRequestHandler<in TRequest, TResponse> where TRequest : IQueryRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request);
    }

    public interface ICommandRequestHandler<in TRequest, TResponse> where TRequest : ICommandRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request);
    }

    public interface ICommandRequest<TResponse> { }
    public interface IQueryRequest<TResponse> { }
}