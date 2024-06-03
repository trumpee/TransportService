using MassTransit;
using Trumpee.MassTransit.Messages.Notifications;

namespace TransportService.Services;

public interface ISchedulingService
{
    Task ProcessNotificationCommand<T>(
        ConsumeContext<T> ctx, Notification notification) where T : class;
}
public class SchedulingService : ISchedulingService
{
    public async Task ProcessNotificationCommand<T>(
        ConsumeContext<T> ctx, Notification notification) where T : class
    {
        var expectedDeliveryTime = notification.Timestamp?.DateTime;
        var priority = (byte)notification.Priority!;

        var endpoint = new Uri($"queue:{notification.Recipient.Channel}");
        var sendPipe = Pipe.Execute<SendContext<Notification>>(sendContext =>
        {
            sendContext.SetPriority(priority);
        });

        if (expectedDeliveryTime is null)
        {
            await ctx.Send(endpoint, notification, sendPipe);
            return;
        }

        var isImmediate = expectedDeliveryTime - DateTime.UtcNow.AddSeconds(5) > TimeSpan.Zero;
        var sendTask = isImmediate
            ? ctx.ScheduleSend(endpoint, expectedDeliveryTime.Value, notification, sendPipe)
            : ctx.Send(endpoint, notification, sendPipe);

        await sendTask;
    }
}
