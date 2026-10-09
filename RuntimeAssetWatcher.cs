using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GCC.Core
{
    // Ensure data structures stay strictly serializable for processing
    [Serializable]
    public class AddressableVisualSpecs
    {
        public string mesh_source;          // Used as the exact Addressable asset path locator string
        public string shader_variant;
        public string emissive_color_hex;
    }

    [Serializable]
    public class ProductionAssetPayload
    {
        public string id;
        public string type;
        public AddressableVisualSpecs visuals;
    }

    [Serializable]
    public class ProductionBlueprint
    {
        public List<ProductionAssetPayload> payload;
    }

    public class RuntimeAssetWatcher : MonoBehaviour
    {
        [Header("Spawn Anchorage")]
        [SerializeField] private Transform sceneInstantiationAnchor;

        // Tracks live instantiated layout elements to clear memory on subsequent shifts
        private Dictionary<string, GameObject> activeInstantiatedAssets = new Dictionary<string, GameObject>();

        public void IngestCatalogBlueprint(string rawJsonPayload)
        {
            try
            {
                ProductionBlueprint blueprint = JsonUtility.FromJson<ProductionBlueprint>(rawJsonPayload);
                ProcessProductionPayload(blueprint);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GCC Addressables Bridge Exception]: Matrix formatting breakdown: {ex.Message}");
            }
        }

        private void ProcessProductionPayload(ProductionBlueprint blueprint)
        {
            foreach (var node in blueprint.payload)
            {
                if (node.visuals == null || string.IsNullOrEmpty(node.visuals.mesh_source)) continue;

                // If the asset instance is already active in our tangent, bypass redundant loading loops
                if (activeInstantiatedAssets.ContainsKey(node.id)) continue;

                Debug.Log($"[GCC Pipeline]: Loading asynchronous addressable asset key: {node.visuals.mesh_source}");
                
                // Execute high-speed async instantiation directly from the catalog key
                Addressables.InstantiateAsync(node.visuals.mesh_source, sceneInstantiationAnchor).Completed += (handle) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        GameObject spawnedObject = handle.Result;
                        activeInstantiatedAssets.Add(node.id, spawnedObject);
                        
                        // Pass off localized physics overrides to custom model entities here
                        ApplyRuntimeMaterialModifiers(spawnedObject, node.visuals);
                    }
                    else
                    {
                        Debug.LogWarning($"[GCC Addressables Warning]: Unable to resolve runtime mesh pointer key for: {node.visuals.mesh_source}");
                    }
                };
            }
        }

        private void ApplyRuntimeMaterialModifiers(GameObject targetObject, AddressableVisualSpecs visuals)
        {
            if (string.IsNullOrEmpty(visuals.emissive_color_hex)) return;

            Renderer meshRenderer = targetObject.GetComponentInChildren<Renderer>();
            if (meshRenderer != null && ColorUtility.TryParseHtmlString(visuals.emissive_color_hex, out Color parsedColor))
            {
                // Instantiate a runtime structural material block instance to map lighting profiles safely
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                propertyBlock.SetColor("_EmissionColor", parsedColor);
                meshRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void OnDestroy()
        {
            // Explicitly clean up allocations inside the runtime asset engine pool to block leaks
            foreach (var spawnedAsset in activeInstantiatedAssets.Values)
            {
                if (spawnedAsset != null) Addressables.ReleaseInstance(spawnedAsset);
            }
            activeInstantiatedAssets.Clear();
        }
    }
}
