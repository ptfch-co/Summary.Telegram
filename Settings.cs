namespace Summary.Telegram.Settings
{
    using Core.DisplayManagement.Entities;
    using Core.DisplayManagement.Handlers;
    using Core.DisplayManagement.Views;
    using Core.Entities;
    using Core.Environment.Shell;
    using Core.Settings;
    using Core.Workflows;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Services;
    using System.Threading.Tasks;

    public class TelegramSettings
    {
        public string Token { get; set; }
        public string Mobile { get; set; }
        public int? Api_Id { get; set; }
        public string Api_Hash { get; set; }
        public string Login_Code { get; set; }
        public string Phone_Hash_Code { get; set; }
        public long? User_Id { get; set; }
    }

    public class TelegramSettingsDisplayDriver : SectionDisplayDriver<ISite,
        TelegramSettings>
    {
        private readonly IShellHost _host;
        private readonly ShellSettings _shell;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly IAuthorizationService _authorize;
        private readonly ITelegramClientService _client;

        public TelegramSettingsDisplayDriver(IShellHost host,
            ShellSettings settings,
            IHttpContextAccessor httpContext,
            IAuthorizationService authorize,
            ITelegramClientService client)
        {
            _host = host;
            _shell = settings;
            _httpAccessor = httpContext;
            _authorize = authorize;
            _client = client;
        }

        public override async Task<IDisplayResult> EditAsync(TelegramSettings settings,
            BuildEditorContext context)
        {
            var user = _httpAccessor.HttpContext?.User;
            if (user is null || !await _authorize.AuthorizeAsync(user, Permissions.ManageWorkflows))
            {
                return null;
            }

            var init = Initialize<TelegramSettings>("TelegramSettings_Edit", model =>
            {
                model.Token = settings.Token;
                model.Mobile = settings.Mobile;
                model.Api_Id = settings.Api_Id;
                model.Api_Hash = settings.Api_Hash;
                model.Login_Code = settings.Login_Code;
                model.Phone_Hash_Code = settings.Phone_Hash_Code;
                model.User_Id = settings.User_Id;
            });
            return init.Location("Content:5").OnGroup("Telegram");
        }

        public override async Task<IDisplayResult> UpdateAsync(TelegramSettings settings,
            BuildEditorContext context)
        {
            var user = _httpAccessor.HttpContext?.User;
            if (user is null || !await _authorize.AuthorizeAsync(user, Permissions.ManageWorkflows))
            {
                return null;
            }
            if (context.GroupId == "Telegram")
            {
                await context.Updater.TryUpdateModelAsync(settings, Prefix);

                if (string.IsNullOrWhiteSpace(settings.Login_Code) && settings.Api_Id.HasValue)
                {
                    settings.User_Id = null;
                    settings.Phone_Hash_Code = await _client.SendCodeAsync(settings.Api_Id.Value, settings.Api_Hash, settings.Mobile);
                }
                else if (await _client.IsLoggedInAsync() is false)
                {
                    settings.User_Id =  await _client.SignInAsync(settings.Login_Code);
                }

                await _host.ReloadShellContextAsync(_shell);
            }

            return await EditAsync(settings, context);
        }
    }

    public class TelegramSettingsConfiguration : IConfigureOptions<TelegramSettings>
    {
        private readonly ISiteService _site;
        private readonly ILogger<TelegramSettingsConfiguration> _logger;

        public TelegramSettingsConfiguration(ISiteService site,
            ILogger<TelegramSettingsConfiguration> logger)
        {
            _site = site;
            _logger = logger;
        }

        public void Configure(TelegramSettings options)
        {
            var settings = _site.GetSiteSettingsAsync().GetAwaiter().GetResult().As<TelegramSettings>();
            options.Token = settings.Token;
            options.Mobile = settings.Mobile;
            options.Api_Id = settings.Api_Id;
            options.Api_Hash = settings.Api_Hash;
            options.Login_Code = settings.Login_Code;
            options.Phone_Hash_Code = settings.Phone_Hash_Code;
            options.User_Id = settings.User_Id;
        }
    }
}