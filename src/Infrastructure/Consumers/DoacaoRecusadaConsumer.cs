using ConexaoSolidaria.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Consumers
{
    public class DoacaoRecusadaConsumer(ILogger<DoacaoRecusadaConsumer> logger) : IConsumer<DoacaoResultadoEvent>
    {
        private readonly ILogger<DoacaoRecusadaConsumer> _logger = logger;
        public async Task Consume(ConsumeContext<DoacaoResultadoEvent> context)
        {
            var msg = context.Message;

            _logger.LogWarning(
                "[EMAIL SIMULADO] CampanhaId: {CampanhaId} | DoadorId: {DoadorId} | Motivo: {Motivo} | Em: {OcorridoEm}",
                msg.CampanhaId, msg.DoadorId, msg.Motivo, msg.OcorridoEm);

            await Task.CompletedTask;
        }
    }
}
