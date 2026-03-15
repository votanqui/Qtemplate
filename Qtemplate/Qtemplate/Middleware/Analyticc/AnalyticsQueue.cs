using System.Threading.Channels;
using Qtemplate.Domain.Entities;

namespace Qtemplate.Middleware.Analyticc;

public sealed class AnalyticsQueue
{
    private readonly Channel<Analytics> _channel =
        Channel.CreateBounded<Analytics>(new BoundedChannelOptions(50_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,   // chỉ AnalyticsDrainService đọc
            SingleWriter = false   // nhiều request thread ghi đồng thời
        });

    public void Enqueue(Analytics item) => _channel.Writer.TryWrite(item);

    public ChannelReader<Analytics> Reader => _channel.Reader;
}