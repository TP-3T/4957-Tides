using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TTT.Managers
{
    public static class AssetLoader<T>
    {
        public static IEnumerator Load(
            AssetReference reference,
            Action<T> callback
        )
        {
            AsyncOperationHandle<T> assetHandle =
                Addressables.LoadAssetAsync<T>(reference);
            assetHandle.Completed += ValidateResult;
            yield return assetHandle;
            Debug.Log("Continuing after loading.");
            callback(assetHandle.Result);
        }

        public static IEnumerator LoadGroup(
            string groupName,
            Action<T> callback
        )
        {
            var assetHandle = Addressables.LoadAssetsAsync<T>(
                groupName,
                callback,
                Addressables.MergeMode.Union, //We want to have all assets with the same group
                false //We don;t want to release if an asset fails to load
            );
            yield return assetHandle;
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
