using TradeServiceApi.Models;
using TradeServiceApi.Repositories;

namespace TradeServiceApi.Services
{
    public class PositionService : IPositionService
    {
        private readonly IInMemoryRepository _repository;
        private readonly ILogger<PositionService> _logger;

        public PositionService(IInMemoryRepository repository, ILogger<PositionService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public void UpdatePosition(string securityCode, long delta)
        {
            _logger.LogInformation($"Updating position: {securityCode}, Delta: {delta}");
            _repository.AddOrUpdatePosition(securityCode, delta);
        }

        public List<PositionDto> GetAllPositions()
        {
            var positions = _repository.GetAllPositions();
            return positions.Select(p => new PositionDto
            {
                SecurityCode = p.SecurityCode,
                NetQuantity = p.NetQuantity > 0 ? $"+{p.NetQuantity}" : p.NetQuantity.ToString(),
                LastUpdatedDate = p.LastUpdatedDate
            }).OrderBy(p => p.SecurityCode).ToList();
        }
    }
}
