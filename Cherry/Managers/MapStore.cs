﻿using Cherry.Models;
using IPA.Loader;
using SiraUtil;
using SiraUtil.Tools;
using SiraUtil.Zenject;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cherry.Managers
{
    internal class MapStore
    {
        private readonly SiraLog _siraLog;
        private readonly SiraClient _siraClient;
        private readonly Dictionary<string, Map> _mapCache;

        public MapStore(SiraLog siraLog, SiraClient siraClient, UBinder<Plugin, PluginMetadata> metadataBinder)
        {
            _siraLog = siraLog;
            _siraClient = siraClient;
            _mapCache = new Dictionary<string, Map>();
            _siraClient.SetUserAgent(nameof(Cherry), metadataBinder.Value.HVersion);
        }

        public async Task<Map?> GetMapAsync(string key, CancellationToken? token = null)
        {
            if (!_mapCache.TryGetValue(key, out Map map))
            {
                key = key.ToLowerInvariant();
                _siraLog.Debug($"Fetching map with key '{key}'.");
                WebResponse webResponse = await _siraClient.GetAsync($"https://api.beatsaver.com/maps/id/{key}", token ?? CancellationToken.None).ConfigureAwait(false);
                _siraLog.Info("HELLO");
                if (!webResponse.IsSuccessStatusCode)
                {
                    _siraLog.Warning(webResponse.StatusCode);
                    return null;
                }
                map = webResponse.ContentToJson<Map>();
                if (!_mapCache.ContainsKey(key))
                    _mapCache.Add(key, map);
            }
            return map;
        }
    }
}