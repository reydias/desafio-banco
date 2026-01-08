using Lancamentos.Application.DTOs;
using Lancamentos.Application.Queries;
using Lancamentos.Domain.Entities;
using Lancamentos.Domain.Interfaces;

namespace Lancamentos.Application.Handlers;

public class ObterLancamentoHandler
{
    private readonly ILancamentoRepository _repository;

    public ObterLancamentoHandler(ILancamentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<LancamentoDTO?> HandleAsync(ObterLancamentoQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Id.HasValue)
        {
            var lancamento = await _repository.ObterPorIdEUsuarioAsync(query.Id.Value, query.UsuarioId, cancellationToken);
            return lancamento == null ? null : MapToDTO(lancamento);
        }

        return null;
    }

    public async Task<IEnumerable<LancamentoDTO>> HandleListAsync(ObterLancamentoQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<Domain.Entities.Lancamento> lancamentos;

        if (query.Data.HasValue)
        {
            lancamentos = await _repository.ObterPorDataEUsuarioAsync(query.Data.Value, query.UsuarioId, cancellationToken);
        }
        else
        {
            lancamentos = await _repository.ObterTodosPorUsuarioAsync(query.UsuarioId, cancellationToken);
        }

        // Filtrar por tipo se especificado
        if (query.Tipo.HasValue)
        {
            lancamentos = lancamentos.Where(l => l.Tipo == query.Tipo.Value);
        }

        return lancamentos.Select(MapToDTO);
    }

    private static LancamentoDTO MapToDTO(Domain.Entities.Lancamento lancamento)
    {
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

