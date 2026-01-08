using System.Text;
using System.Text.Json;
using Lancamentos.Application.Converters;
using Lancamentos.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Lancamentos.Infrastructure.Messaging;

public class RabbitMQEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQEventPublisher> _logger;
    private const string ExchangeName = "lancamentos_exchange";
    private const string QueueName = "lancamento_criado_queue";
    private const string RoutingKey = "lancamento.criado";

    public RabbitMQEventPublisher(IConfiguration configuration, ILogger<RabbitMQEventPublisher> logger)
    {
        _logger = logger;
        var connectionString = configuration["RabbitMQ:ConnectionString"] ?? "amqp://guest:guest@localhost:5672";

        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declarar exchange e queue
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(QueueName, ExchangeName, RoutingKey);

        _logger.LogInformation("RabbitMQ connection established");
    }

    public Task PublicarAsync<T>(T evento, CancellationToken cancellationToken = default) where T : class
    {
        if (!_connection.IsOpen)
        {
            _logger.LogWarning("RabbitMQ connection not available. Event not published: {EventType}", typeof(T).Name);
            return Task.CompletedTask;
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new TipoLancamentoJsonConverter() }
            };
            var message = JsonSerializer.Serialize(evento, options);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Event published to RabbitMQ: {EventType}", typeof(T).Name);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event to RabbitMQ: {EventType}", typeof(T).Name);
            // Não lançar exceção para não bloquear a criação do lançamento
            return Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
