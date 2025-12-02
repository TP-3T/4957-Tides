using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TTT.Managers
{
    public static class AssetLoader<T>
    {
        // Track all loaded handles to prevent memory leaks
        private static List<AsyncOperationHandle> loadedHandles = new();

        public static IEnumerator Load(
            AssetReference reference,
            Action<T> callback
        )
        {
            AsyncOperationHandle<T> assetHandle =
                Addressables.LoadAssetAsync<T>(reference);
            assetHandle.Completed += ValidateResult;
            loadedHandles.Add(assetHandle);
            yield return assetHandle;
            Debug.Log("Continuing after loading.");
            callback(assetHandle.Result);
        }

        public static IEnumerator LoadGroup(
            string groupName,
            Action<T> callback
        )
        {
            var assetHandle = Addressables.LoadAssetsAsync<T>(groupName);
            loadedHandles.Add(assetHandle);
            while (!assetHandle.IsDone)
            {
                yield return assetHandle;
            }
            foreach (T item in assetHandle.Result)
            {
                callback(item);
            }
        }

        /// <summary>
        /// Release all loaded Addressable assets to prevent memory leaks.
        /// Call this when changing scenes or when assets are no longer needed.
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var handle in loadedHandles)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            loadedHandles.Clear();
            Debug.Log($"Released {loadedHandles.Count} Addressable asset handles");
        }

        private static void ValidateResult(AsyncOperationHandle<T> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new Exception(
                    $"Operation for {handle.DebugName} failed!"
                );
            }
        }
    }
}
