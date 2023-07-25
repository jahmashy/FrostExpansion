using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
namespace Frost.Client.Extensions
{
    public static class LocalStorageServiceExtension
    {
        public static async Task SaveItemAsEncryptedAsync<T>(this ILocalStorageService localStorageService, string key, T item)
        {
            var JsonItem = JsonSerializer.Serialize(item);
            var JsonBytes = Encoding.UTF8.GetBytes(JsonItem);
            var base64Json = Convert.ToBase64String(JsonBytes);
            await localStorageService.SetItemAsync(key,base64Json);
        }
        public static async Task<T> ReadEncryptedItemAsync<T>(this ILocalStorageService localStorageService, string key)
        {
            var base64Json = await localStorageService.GetItemAsync<string>(key);
            var JsonBytes = Convert.FromBase64String(base64Json);
            var JsonItem = Encoding.UTF8.GetString(JsonBytes);
            var item = JsonSerializer.Deserialize<T>(JsonItem);
            return item;
        }
    }
}
