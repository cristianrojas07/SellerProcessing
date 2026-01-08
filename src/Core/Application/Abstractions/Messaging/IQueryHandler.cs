using Domain.Common;

namespace Application.Abstractions.Messaging;

public interface IQuery { }

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
