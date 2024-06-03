using MassTransit;
using Serilog;
using TransportService.Extensions;
using Trumpee.MassTransit.Messages.Notifications;

namespace TransportService;

internal class HighNotificationsConsumer : IConsumer<Notification>
{
    public async Task Consume(ConsumeContext<Notification> context)
    {
        var notification = context.Message;

        Log.Logger.Debug(
            "Consumer {ConsumerName} received message. Message CorrelationID is {CorrelationId}",
            nameof(MediumNotificationsConsumer), context.CorrelationId);

        await context.ProcessNotificationCommand(notification);
    }
}