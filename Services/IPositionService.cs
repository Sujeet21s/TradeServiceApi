using TradeServiceApi.Models;

namespace TradeServiceApi.Services
{
    public interface IPositionService
    {
        void UpdatePosition(string securityCode, long delta);
        List<PositionDto> GetAllPositions();
        
    }
}
