using System.Threading.Channels;
using AuditLogEntity = Qtemplate.Domain.Entities.AuditLog;

namespace Qtemplate.Infrastructure.Services.AuditLog;

public sealed class AuditLogQueue
{
    private readonly Channel<AuditLogEntity> _channel =
        Channel.CreateBounded<AuditLogEntity>(new BoundedChannelOptions(5_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

    public void Enqueue(AuditLogEntity entry) => _channel.Writer.TryWrite(entry);

    public ChannelReader<AuditLogEntity> Reader => _channel.Reader;
}