using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Messaging;

public class RabbitMqPersistentConnection(IConnectionFactory _connectionFactory,
    ILogger<RabbitMqPersistentConnection> _logger) : IDisposable
{
    private IConnection? _connection;
    private bool _disposed;
    private readonly object _syncRoot = new();

    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public IModel CreateModel()
    {
        if (!IsConnected)
        {
            TryConnect();
        }

        return _connection!.CreateModel();
    }

    public bool TryConnect()
    {
        _logger.LogInformation("RabbitMQ Client is trying to connect");

        lock (_syncRoot)
        {
            if (IsConnected) return true;

            try
            {
                _connection = _connectionFactory.CreateConnection();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
                return false;
            }

            if (IsConnected)
            {
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                _logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events", _connection.Endpoint.HostName);

                return true;
            }
            else
            {
                _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
                return false;
            }
        }
    }

    private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
    {
        if (_disposed) return;
        _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");
        TryConnect();
    }

    void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        if (_disposed) return;
        _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
        TryConnect();
    }

    void OnConnectionShutdown(object? sender, ShutdownEventArgs reason)
    {
        if (_disposed) return;
        _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
        TryConnect();
    }

    public void EnsureConnected()
    {
        if (!IsConnected)
        {
            TryConnect();
        }

        if (!IsConnected)
        {
            throw new InvalidOperationException("RabbitMQ Connection is not available.");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        try
        {
            _connection?.Dispose();
        }
        catch (IOException ex)
        {
            _logger.LogCritical(ex.ToString());
        }
    }
}