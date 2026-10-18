using ConexaoSolidaria.Events;
using Infrastructure.Data;
using MassTransit;

namespace Infrastructure.Consumers
{
    public class ArrecadacaoAtualizadaConsumer(ICampaignRepository repository) : IConsumer<ArrecadacaoAtualizadaEvent>
    {
        private readonly ICampaignRepository _campaignRepository = repository;
        public async Task Consume(ConsumeContext<ArrecadacaoAtualizadaEvent> context)
        {
            var result = await _campaignRepository.UpdateAsync(context.Message);
            if (!result)
                throw new InvalidOperationException($"Falha ao atualizar campanha (evento {context.MessageId})");
        }
    }
}
