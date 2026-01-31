

using Microsoft.AspNetCore.Mvc;
using TradeServiceApi.Models;

namespace TradeServiceApi.Endpoints.ServiceEndpointHandler
{
    public static class ServiceEndpoints
    {
        public static RouteGroupBuilder MapServiceEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api")
                            .WithTags("Transactions");

            group.MapPost("/transaction", TransactionAsync);

            group.MapGet("/positions", PositionAsync);
            group.MapGet("/alltransactions", GetAllTransactionAsync);            

            return group;
        }
        private static Task<IResult> TransactionAsync([FromBody] TransactionDto request,
                ServiceEndpointHandler handler,
                CancellationToken cts)
                        => handler.HandleTransactionServiceAsync(request, cts);
        private static Task<IResult> GetAllTransactionAsync(
                ServiceEndpointHandler handler,
                CancellationToken cts)
                        => handler.HandleGetAllTransactionAsync(cts);

        private static Task<IResult> PositionAsync(
                ServiceEndpointHandler handler,
                CancellationToken cts)
                        => handler.HandlePositionServiceAsync(cts);
    }
}
