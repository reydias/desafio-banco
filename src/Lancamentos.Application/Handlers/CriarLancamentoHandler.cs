using Lancamentos.Application.Commands;
using Lancamentos.Application.DTOs;
using Lancamentos.Domain.Entities;
using Lancamentos.Domain.Events;
using Lancamentos.Domain.Interfaces;

namespace Lancamentos.Application.Handlers;

public class CriarLancamentoHandler
{
    private readonly ILancamentoRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public CriarLancamentoHandler(
        ILancamentoRepository repository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<LancamentoDTO> HandleAsync(CriarLancamentoCommand command, CancellationToken cancellationToken = default)
    {
        // Validações
        if (command.Valor <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.", nameof(command.Valor));

        if (string.IsNullOrWhiteSpace(command.Descricao))
            throw new ArgumentException("A descrição é obrigatória.", nameof(command.Descricao));

        // Criar lançamento
        var lancamento = new Lancamento(
            command.UsuarioId,
            command.Data,
            command.Valor,
            command.Tipo,
            command.Descricao
        );

        await _repository.AdicionarAsync(lancamento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publicar evento (assíncrono, não bloqueia)
        var evento = new LancamentoCriadoEvent(
            lancamento.Id,
            lancamento.UsuarioId,
            lancamento.Data,
            lancamento.Valor,
            lancamento.Tipo,
            lancamento.DataCriacao
        );

        await _eventPublisher.PublicarAsync(evento, cancellationToken);

        return new LancamentoDTO
        {
            Id = lancamento.Id,
            Data = lancamento.Data,
            Valor = lancamento.Valor,
            Tipo = lancamento.Tipo,
            Descricao = lancamento.Descricao,
            DataCriacao = lancamento.DataCriacao
        };
    }
}

