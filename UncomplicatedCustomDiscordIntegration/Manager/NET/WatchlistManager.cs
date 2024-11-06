using Exiled.API.Features;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedDiscordIntegration.API.Features;

namespace UncomplicatedDiscordIntegration.Manager.NET
{
    internal class WatchlistManager
    {
        public static HttpClient HttpClient => Plugin.Instance.httpManager.HttpClient;

        public readonly static string Endpoint = "https://api.ucserver.it/foxbase/v1";

        public static bool CanProceed => Plugin.Instance.Config.UCDInternalWatchlistIdAccess is not null && Plugin.Instance.Config.UCDInternalWatchlistIdAccess != string.Empty;

        public static string Id => Plugin.Instance.Config.UCDInternalWatchlistIdAccess;

        public async static void Init()
        {
            if (Plugin.Instance.Config.UCDInternalWatchlistIdAccess == string.Empty)
            {
                HttpResponseMessage message = await HttpClient.PostAsync($"{Endpoint}/database", new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>()
                {
                    {
                        "type",
                        "associative"
                    }
                }), Encoding.UTF8, "application/json"));

                if (message.StatusCode is not HttpStatusCode.Created)
                {
                    Log.Warn($"Failed to create a remote database on UCS@FoxBase!\nIt seems that you've exceeded the database limit, please contact us as soon as possible!\nFoxBase says: {message.StatusCode}");
                    return;
                }

                Dictionary<string, string> answer = JsonConvert.DeserializeObject<Dictionary<string, string>>(await message.Content.ReadAsStringAsync());
                Plugin.Instance.Config.UCDInternalWatchlistIdAccess = answer["id"];

                Log.Info($"Successfully created a new instance of a remote database on UCS@FoxBase!\nKey: {Plugin.Instance.Config.UCDInternalWatchlistIdAccess}");

                await Task.Delay(1000);
                if (Plugin.Instance.Config.UCDInternalWatchlistIdAccess != string.Empty)
                    File.WriteAllText(Paths.Config, File.ReadAllText(Paths.Config).Replace("u_c_d_internal_watchlist_id_access: ''", $"u_c_d_internal_watchlist_id_access: '{Plugin.Instance.Config.UCDInternalWatchlistIdAccess}'"));
            }
        }

#nullable enable
        public static async Task<WatchlistEntry?> Get(Player player)
        {
            if (!CanProceed)
                return null;

            return await Get(player.UserId);
        }

        public static async Task<WatchlistEntry?> Get(string player)
        {
            if (!CanProceed)
                return null;

            HttpResponseMessage message = await HttpClient.GetAsync($"{Endpoint}/database/{Id}/{player}");

            if (message.StatusCode == HttpStatusCode.NotFound)
                return null;
            else if (message.StatusCode != HttpStatusCode.OK)
            {
                Log.Warn($"Error while trying to fetch WatchlistEntry for every user from FoxBase Database {Id}!\nExpected 404 Not Found or 200 OK, got {(int)message.StatusCode} {message.StatusCode}!");
                return null;
            }

            return JsonConvert.DeserializeObject<WatchlistEntry>(await message.Content.ReadAsStringAsync());
        }

        public static async Task<WatchlistEntry[]?> Get()
        {
            if (!CanProceed)
                return null;

            HttpResponseMessage message = await HttpClient.GetAsync($"{Endpoint}/database/{Id}");

            if (message.StatusCode == HttpStatusCode.NotFound)
                return null;
            else if (message.StatusCode != HttpStatusCode.OK)
            {
                Log.Warn($"Error while trying to fetch WatchlistEntry for every user from FoxBase Database {Id}!\nExpected 404 Not Found or 200 OK, got {(int)message.StatusCode} {message.StatusCode}!");
                return null;
            }

            return JsonConvert.DeserializeObject<Dictionary<string, WatchlistEntry>>(await message.Content.ReadAsStringAsync())?.Values.ToArray(); // Is an associative array (or object)
        }

        public static async Task<bool> Add(Player author, Player target, string reason) => await Add(new(author, target, reason));

        public static async Task<bool> Add(WatchlistEntry entry)
        {
            HttpResponseMessage message = await HttpClient.PostAsync($"{Endpoint}/database/{Id}/{entry.TargetId}", new StringContent(JsonConvert.SerializeObject(entry), Encoding.UTF8, "application/json"));

            if (message.StatusCode is not HttpStatusCode.Created)
            {
                Log.Warn($"Failed to create a new entry inside the FoxBase Database {Id}!\nExpected 201 Created, got {(int)message.StatusCode} {message.StatusCode}!");
                return false;
            }

            return true;
        }

        public static async Task<bool> Remove(string targetId)
        {
            HttpResponseMessage message = await HttpClient.DeleteAsync($"{Endpoint}/database/{Id}/{targetId}");

            if (message.StatusCode is not HttpStatusCode.NoContent)
            {
                Log.Warn($"Failed to delete an existing entry inside the FoxBase Database {Id}!\nExpected 204 No Content, got {(int)message.StatusCode} {message.StatusCode}!");
                return false;
            }

            return true;
        }

        public static async Task<bool> Clear()
        {
            HttpResponseMessage message = await HttpClient.PutAsync($"{Endpoint}/database/{Id}", new StringContent("{}", Encoding.UTF8, "application/json"));

            if (message.StatusCode is not HttpStatusCode.NoContent)
            {
                Log.Warn($"Failed to delete the FoxBase Database {Id}!\nExpected 204 No Content, got {(int)message.StatusCode} {message.StatusCode}!");
                return false;
            }

            return true;
        }

        public static WatchlistEntry? GetSync(string steamId)
        {
            Task<WatchlistEntry?> task = Task.Run(() => Get(steamId));
            task.Wait();

            return task.Result;
        }

#nullable disable
        public static T Sync<T>(Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
