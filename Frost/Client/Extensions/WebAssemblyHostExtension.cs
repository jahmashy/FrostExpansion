using Blazored.LocalStorage;
using Frost.Client.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Frost.Client.Extensions
{
    public static class WebAssemblyHostExtension
    {
        public async static Task SetDefaultCultureAsync(this WebAssemblyHost host)
        {
            var localStorage = host.Services.GetRequiredService<ILocalStorageService>();
            var cultureFromLS = await localStorage.GetItemAsync<string>("culture");

            CultureInfo culture;

            if (cultureFromLS != null)
            {
                culture = new CultureInfo(cultureFromLS);
            }
            else
            {
                culture = new CultureInfo("pl-PL");
            }

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
