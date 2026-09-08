namespace Summary.Telegram
{
    using Core.DisplayManagement.Handlers;
    using Core.Modules;
    using Core.Navigation;
    using Core.Settings;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Summary.Telegram.Settings;
    using Summary.Telegram.Services;

    [Feature(Telegram.Features.Telegram)]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, Menu>();
            services.AddScoped<IDisplayDriver<ISite>, TelegramSettingsDisplayDriver>();

            services.AddSingleton<ITelegramClientService, TelegramClientService>();

            services.AddTransient<IConfigureOptions<TelegramSettings>, TelegramSettingsConfiguration>();
        }
    }
}