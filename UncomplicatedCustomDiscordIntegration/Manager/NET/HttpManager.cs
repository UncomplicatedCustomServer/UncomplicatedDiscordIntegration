﻿using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Loader;
using MEC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomDiscordIntegration.API.Struct;
using PlayerHandler = Exiled.Events.Handlers.Player;

namespace UncomplicatedCustomDiscordIntegration.Manager.NET
{
#pragma warning disable IDE1006

    internal class HttpManager
    {
        /// <summary>
        /// Gets the prefix of the plugin for our APIs
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Gets the <see cref="HttpClient"/> public istance
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the UCS APIs endpoint
        /// </summary>
        public string Endpoint { get; } = "https://ucs.fcosma.it/api/v2";

        /// <summary>
        /// Gets the CreditTag storage for the plugin, downloaded from our central server
        /// </summary>
        public Dictionary<string, Triplet<string, string, bool>> Credits { get; internal set; } = new();

        /// <summary>
        /// Gets the role of the given player (as steamid@64) inside UCR
        /// </summary>
        public Dictionary<string, string> OrgPlayerRole { get; } = new();

        /// <summary>
        /// Gets the latest <see cref="Version"/> of the plugin, loaded by the UCS cloud
        /// </summary>
        public Version LatestVersion
        {
            get
            {
                if (_latestVersion is null)
                    LoadLatestVersion();
                return _latestVersion;
            }
        }

        private Version _latestVersion { get; set; } = null;

        private bool _alreadyManaged { get; set; } = false;

        /// <summary>
        /// Create a new istance of the HttpManager
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="maxErrors"></param>
        public HttpManager(string prefix)
        {
            if (!CheckForDependency())
                Timing.CallContinuously(15f, () => Log.Error("You don't have the dependency Newtonsoft.Json installed!\nPlease install it AS SOON AS POSSIBLE!\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV"));

            Prefix = prefix;
            HttpClient = new();
            LoadCreditTags();
            RegisterEvents();
        }

        internal void RegisterEvents()
        {
            PlayerHandler.Verified += OnVerified;
        }

        internal void UnregisterEvents()
        {
            PlayerHandler.Verified -= OnVerified;
        }

        public void OnVerified(VerifiedEventArgs ev) => ApplyCreditTag(ev.Player);

        private bool CheckForDependency() => Loader.Dependencies.Any(assembly => assembly.GetName().Name == "Newtonsoft.Json");

        public HttpResponseMessage HttpGetRequest(string url)
        {
            try
            {
                Task<HttpResponseMessage> Response = Task.Run(() => HttpClient.GetAsync(url));

                Response.Wait();

                return Response.Result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public HttpResponseMessage HttpPutRequest(string url, string content)
        {
            try
            {
                Task<HttpResponseMessage> Response = Task.Run(() => HttpClient.PutAsync(url, new StringContent(content, Encoding.UTF8, "text/plain")));

                Response.Wait();

                return Response.Result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string RetriveString(HttpResponseMessage response)
        {
            if (response is null)
                return string.Empty;

            return RetriveString(response.Content);
        }

        public string RetriveString(HttpContent response)
        {
            if (response is null)
                return string.Empty;

            Task<string> String = Task.Run(response.ReadAsStringAsync);

            String.Wait();

            return String.Result;
        }

        public HttpStatusCode AddServerOwner(string discordId) => HttpGetRequest($"{Endpoint}/owners/add?discordid={discordId}")?.StatusCode ?? HttpStatusCode.InternalServerError;

        public void LoadLatestVersion()
        {
            string Version = RetriveString(HttpGetRequest($"{Endpoint}/{Prefix}/version?vts=5"));

            if (Version is not null && Version != string.Empty && Version.Contains("."))
                _latestVersion = new(Version);
            else
                _latestVersion = new();
        }

        public void LoadCreditTags()
        {
            Credits = [];
            try
            {
                Dictionary<string, Dictionary<string, string>> Data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(RetriveString(HttpGetRequest("https://ucs.fcosma.it/api/credits.json")));

                foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Data.Where(kvp => kvp.Value.ContainsKey("role") && kvp.Value.ContainsKey("color") && kvp.Value.ContainsKey("override")))
                {
                    Credits.Add(kvp.Key, new(kvp.Value["role"], kvp.Value["color"], bool.Parse(kvp.Value["override"])));
                    if (kvp.Value.TryGetValue("job", out string isJob) && isJob is "true")
                        OrgPlayerRole.Add(kvp.Key, isJob);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to act HttpManager::LoadCreditTags() - {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
            }
        }

        public Triplet<string, string, bool> GetCreditTag(Player player)
        {
            if (Credits.ContainsKey(player.UserId))
                return Credits[player.UserId];

            return new(null, null, false);
        }

        public void ApplyCreditTag(Player player)
        {
            if (_alreadyManaged)
                return;

            Triplet<string, string, bool> Tag = GetCreditTag(player);

            if (player.RankName is not null && player.RankName != string.Empty)
            {
                if (Credits.Any(k => k.Value.First == player.RankName && k.Value.Second == player.RankColor))
                    _alreadyManaged = true;

                if (!Tag.Third)
                    return; // Do not override
            }

            if (_alreadyManaged)
                return;

            if (Tag.First is not null && Tag.Second is not null)
            {
                player.RankName = Tag.First;
                player.RankColor = Tag.Second;
            }
        }

        public bool IsLatestVersion(out Version latest)
        {
            latest = LatestVersion;
            if (latest.CompareTo(Plugin.Instance.Version) > 0)
                return false;

            return true;

        }

        public bool IsLatestVersion()
        {
            if (LatestVersion.CompareTo(Plugin.Instance.Version) > 0)
                return false;

            return true;
        }

        public async Task<string> TryGetNickname(string steamid)
        {
            return await HttpClient.GetStringAsync($"https://ucs.fcosma.it/api/steam/nickname?id={steamid}");
        }
    }
}