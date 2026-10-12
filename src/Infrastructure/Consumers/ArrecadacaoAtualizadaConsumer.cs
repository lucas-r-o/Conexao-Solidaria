using ConexaoSolidaria.Events;
using Infrastructure.Data;
using MassTransit;

namespace Infrastructure.Consumers
{
    public class ArrecadacaoAtualizadaConsumer(ICampanhaRepository repository) : IConsumer<ArrecadacaoAtualizadaEvent>
    {
        private readonly ICampanhaRepository _campanhaRepository = repository;
        public async Task Consume(ConsumeContext<ArrecadacaoAtualizadaEvent> context)
        {
            var result = await _campanhaRepository.UpdateAsync(context.Message);
            if (!result)
                throw new InvalidOperationException($"Falha ao atualizar campanha (evento {context.MessageId})");
        }
    }
}
