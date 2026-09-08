namespace Summary.Telegram.Services
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;
    using Summary.Telegram.Settings;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using TL;
    using WTelegram;

    public interface ITelegramClientService
    {
        Task<Client> GetClientAsync(int? api_hi = null, string api_hash = null);
        Task<string> SendCodeAsync(int api_id, string api_hash, string mobile);
        Task<bool> IsLoggedInAsync();
        Task<long> SignInAsync(string code);
    }

    public class TelegramClientService : ITelegramClientService, IDisposable
    {
        private Client _client;

        private readonly TelegramSettings _options;

        private readonly IWebHostEnvironment _environment;

        public TelegramClientService(
            IOptions<TelegramSettings> options,
            IWebHostEnvironment environment)
        {
            _options = options.Value;
            _environment = environment;
        }

        public async Task<Client> GetClientAsync(int? api_id = null, string api_hash = null)
        {
            if (_client != null) return _client;

            _client = new Client(what => what switch
            {
                "api_id" => (api_id ?? _options.Api_Id.Value).ToString(),
                "api_hash" => api_hash ?? _options.Api_Hash,
                "session_pathname" => Path.Combine(_environment.ContentRootPath, "telegram.session"),
                _ => null
            });

            await _client.ConnectAsync();

            return _client;
        }

        [Obsolete]
        public async Task<string> SendCodeAsync(int api_id, string api_hash, string mobile)
        {
            var client = await GetClientAsync(api_id, api_hash);

            var code_base = await client.Auth_SendCode(
                mobile,
                api_id,
                api_hash,
                new CodeSettings()
            );

            return code_base is Auth_SentCode auth ?
                auth.phone_code_hash :
                throw new NotImplementedException();
        }

        public async Task<bool> IsLoggedInAsync()
        {
            if (_options.Api_Id.HasValue is false) return false;

            var client = await GetClientAsync();

            try
            {
                var users = await client.Users_GetUsers(new InputUserBase[] { new InputUserSelf() });
                return users.Length > 0;
            }
            catch (RpcException)
            {
                return false;
            }
        }

        [Obsolete]
        public async Task<long> SignInAsync(string code)
        {
            var client = await GetClientAsync();

            var authorization = await client.Auth_SignIn(
                _options.Mobile,
                _options.Phone_Hash_Code,
                code
            );

            var auth = authorization as Auth_Authorization;

            var user = auth.user as User;

            return user.ID;
        }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }
    }
}