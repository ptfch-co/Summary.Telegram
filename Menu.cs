namespace Summary.Telegram
{
    using Microsoft.Extensions.Localization;
    using Core.Navigation;
    using System;
    using System.Threading.Tasks;

    public class Menu : INavigationProvider
    {
        private readonly IStringLocalizer<Menu> _localizer;

        public Menu(IStringLocalizer<Menu> localizer)
        {
            _localizer = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase)) return Task.CompletedTask;

            builder.Add(_localizer["Configuration"], configuration =>
            {
                configuration.Add(_localizer["Settings"], settings =>
                {
                    settings.Add(_localizer["تلگرام"], _localizer["تلگرام"], itemBuilder =>
                    {
                        itemBuilder.Action("Index", "Admin", new { area = "Core.Settings", groupId = "Telegram" }).LocalNav();
                    });
                });
            });

            return Task.CompletedTask;
        }
    }
}