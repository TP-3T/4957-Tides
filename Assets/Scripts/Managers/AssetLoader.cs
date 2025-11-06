using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TTT.Managers
{
    public static class AssetLoader<T>
    {
        public static IEnumerator Load(AssetReference reference, Action<T> callback)
        {
            AsyncOperationHandle<T> assetHandle = Addressables.LoadAssetAsync<T>(reference);
            assetHandle.Completed += ValidateResult;
            yield return assetHandle;
            Debug.Log("Continuing after loading.");
            callback(assetHandle.Result);
        }

        private static void ValidateResult(AsyncOperationHandle<T> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new Exception($"Operation for {handle.DebugName} failed!");
            }
        }
    }
}
