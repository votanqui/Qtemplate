using System.Threading.Channels;
using Qtemplate.Domain.Entities;

namespace Qtemplate.Middleware.RequestLogs;

public sealed class RequestLogQueue
{
    private readonly Channel<RequestLog> _channel =
        Channel.CreateBounded<RequestLog>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

    public void Enqueue(RequestLog log) => _channel.Writer.TryWrite(log);

    public ChannelReader<RequestLog> Reader => _channel.Reader;
}