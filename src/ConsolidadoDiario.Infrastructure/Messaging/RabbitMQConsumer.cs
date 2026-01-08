using System.Text;
using System.Text.Json;
using ConsolidadoDiario.Application.Commands;
using ConsolidadoDiario.Application.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsolidadoDiario.Infrastructure.Messaging;

public class RabbitMQConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private const string ExchangeName = "lancamentos_exchange";
    private const string QueueName = "lancamento_criado_queue";
    private const string RoutingKey = "lancamento.criado";

    public RabbitMQConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<RabbitMQConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        var connectionString = configuration["RabbitMQ:ConnectionString"] ?? "amqp://guest:guest@localhost:5672";

        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declarar exchange e queue (idempotente)
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(QueueName, ExchangeName, RoutingKey);

        // Configurar QoS para processar uma mensagem por vez
        _channel.BasicQos(0, 1, false);

        _logger.LogInformation("RabbitMQ consumer initialized");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                _logger.LogInformation("Event received: {Message}", message);

                // Deserializar evento
                var evento = JsonSerializer.Deserialize<LancamentoCriadoEvent>(message);
                if (evento == null)
                {
                    _logger.LogWarning("Failed to deserialize event");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                // Processar evento
                using (var scope = _serviceProvider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<ProcessarLancamentoEventHandler>();
                    
                    var command = new ProcessarLancamentoEventCommand
                    {
                        LancamentoId = evento.LancamentoId,
                        UsuarioId = evento.UsuarioId,
                        Data = evento.Data,
                        Valor = evento.Valor,
                        Tipo = evento.Tipo,
                        DataCriacao = evento.DataCriacao
                    };

                    await handler.HandleAsync(command, stoppingToken);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);
                // Rejeitar mensagem e não reenviar (pode ser enviada para DLQ se configurado)
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(QueueName, autoAck: false, consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }

    // Classe para deserializar o evento do RabbitMQ
    private class LancamentoCriadoEvent
    {
        public Guid LancamentoId { get; set; }
        public Guid UsuarioId { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty; // "C" = Credito, "D" = Debito
        public DateTime DataCriacao { get; set; }
    }
}

