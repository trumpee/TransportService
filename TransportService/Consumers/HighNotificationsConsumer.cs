using MassTransit;
using Serilog;
using TransportService.Services;
using Trumpee.MassTransit.Messages.Notifications;

namespace TransportService.Consumers;

internal class HighNotificationsConsumer(
    ISchedulingService schedulingService) : IConsumer<Notification>
{
    public async Task Consume(ConsumeContext<Notification> context)
    {
        var notification = context.Message;

        Log.Logger.Debug(
            "Consumer {ConsumerName} received message. Message CorrelationID is {CorrelationId}",
            nameof(MediumNotificationsConsumer), context.CorrelationId);

        await schedulingService.ProcessNotificationCommand(context, notification);
    }
}
