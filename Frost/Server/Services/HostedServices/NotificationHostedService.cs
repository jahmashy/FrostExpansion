using Frost.Server.Repositories;
using Frost.Server.Services.MailServices;

namespace Frost.Server.Services.HostedServices
{
    internal interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }

    internal class ScopedProcessingService : IScopedProcessingService
    {
        private IMailService _mailService;
        private IPropertyRepository _propertyDbService;
        public ScopedProcessingService(IMailService mailService, IPropertyRepository propertyDbService)
        {
            _mailService = mailService;
            _propertyDbService = propertyDbService;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<(string userMail, string offerTitle)> data = _propertyDbService.GetOffersAboutToExpire();
                foreach(var item in data)
                {
                    _mailService.NotifyUserAboutExpiringOfferAsync(item.userMail,item.offerTitle);
                }
                await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
            }
        }
    }
    public class NotificationHostedService : BackgroundService
    {
        public IServiceProvider Services { get; }
        public NotificationHostedService(IServiceProvider services) {
            Services = services;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedProcessingService>();

                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
        }
    }
}
