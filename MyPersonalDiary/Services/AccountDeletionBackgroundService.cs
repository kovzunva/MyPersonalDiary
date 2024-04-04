using MyPersonalDiary.Interfaces;

namespace MyPersonalDiary.Services
{
    public class AccountDeletionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AccountDeletionBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            IAccountService accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
            var users = await accountService.GetUsersForDeletionAsync();

            foreach (var user in users)
            {
                await accountService.DeleteAccountAsync(user);
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
