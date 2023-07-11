﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using static Vivify.VivifyController;
using Object = UnityEngine.Object;

namespace Vivify.Managers
{
    internal class AssetBundleManager : IDisposable
    {
        private readonly Dictionary<string, Object> _assets = new();

        private readonly AssetBundle _mainBundle;

        [UsedImplicitly]
        private AssetBundleManager(IDifficultyBeatmap difficultyBeatmap, Config config)
        {
            if (difficultyBeatmap is not CustomDifficultyBeatmap customDifficultyBeatmap)
            {
                throw new ArgumentException(
                    $"Was not correct type. Expected: {nameof(CustomDifficultyBeatmap)}, was: {difficultyBeatmap.GetType().Name}.",
                    nameof(difficultyBeatmap));
            }

            string path = Path.Combine(((CustomBeatmapLevel)customDifficultyBeatmap.level).customLevelPath, BUNDLE);
            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"[{BUNDLE}] not found!"); // TODO: Figure out a way to not just obliterate everything
            }

            if (Heck.HeckController.DebugMode)
            {
                _mainBundle = AssetBundle.LoadFromFile(path);
            }
            else
            {
                CustomData levelCustomData = ((CustomBeatmapSaveData)customDifficultyBeatmap.beatmapSaveData).levelCustomData;
                uint assetBundleChecksum = levelCustomData.GetRequired<uint>(ASSET_BUNDLE);
                _mainBundle = AssetBundle.LoadFromFile(path, assetBundleChecksum);
            }

            if (_mainBundle == null)
            {
                throw new InvalidOperationException($"Failed to load [{path}]");
            }

            string[] assetnames = _mainBundle.GetAllAssetNames();
            foreach (string name in assetnames)
            {
                Plugin.Log.LogDebug($"Loaded [{name}].");
                Object asset = _mainBundle.LoadAsset(name);
                _assets.Add(name, asset);
            }
        }

        public void Dispose()
        {
            if (_mainBundle != null)
            {
                _mainBundle.Unload(true);
            }
        }

        internal bool TryGetAsset<T>(string assetName, [NotNullWhen(true)] out T? asset)
        {
            if (_assets.TryGetValue(assetName, out Object gameObject))
            {
                if (gameObject is T t)
                {
                    asset = t;
                    return true;
                }

                Plugin.Log.LogWarning($"Found {assetName}, but was null or not [{typeof(T).FullName}]!");
            }
            else
            {
                Plugin.Log.LogWarning($"Could not find {typeof(T).FullName} [{assetName}].");
            }

            asset = default;
            return false;
        }
    }
}
