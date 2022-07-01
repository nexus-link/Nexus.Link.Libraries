using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Queue.Model;

// This code was based on https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services#queued-background-tasks
namespace Nexus.Link.Libraries.Core.Queue.Logic
{
    public class ChannelQueue<T> : IStoppableQueue<T>
    {
        private readonly int _capacity;
        private readonly Channel<T> _channel;
        private int _items;
        private bool _full;
        private bool _stopped;

        public ChannelQueue(int capacity = int.MaxValue)
        {
            _capacity = capacity;
            var options = new UnboundedChannelOptions();
            _channel = Channel.CreateUnbounded<T>(options);
            _items = 0;
        }

        /// <inheritdoc />
        /// <exception cref="QueueFullException">Thrown when the queue has already reached its capacity.</exception>
        public async Task EnqueueAsync(T item, CancellationToken cancellationToken)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            lock (_channel)
            {
                if (_full) throw new QueueFullException($"The queue has already reached its capacity ({_capacity}).");
                _items++;
                if (_items >= _capacity) _full = true;
            }

            await _channel.Writer.WriteAsync(item, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> DequeueAsync(CancellationToken cancellationToken)
        {
            var item = await _channel.Reader.ReadAsync(cancellationToken);
            lock (_channel)
            {
                _items--;
                if (_full && _items < _capacity) _full = false;
            }
            while (_stopped) await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);

            return item;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stopped = true;
            return Task.CompletedTask;
        }
    }

    public class QueueFullException : Exception
    {
        public QueueFullException(string message) : base(message)
        {
        }
    }
}