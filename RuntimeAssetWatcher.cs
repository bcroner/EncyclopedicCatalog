using System;
using System.Collections.Generic;
using UnityEngine;

namespace GCC.Core
{
    // =========================================================================
    // SERIALIZABLE DATA SCHEMA (Matches Catalog JSON Output Structure)
    // =========================================================================
    [Serializable]
    public class StructuralSpecs
    {
        public string lighting;          // e.g., "neon_low_key"
        public string material;          // e.g., "monomolecular_carbon"
        public string emissive_color;    // e.g., "#00ffcc"
        public string name;
        public int base_height_cm;
    }

    [Serializable]
    public class CatalogAssetManifest
    {
        public string asset_id;
        public string asset_type;
        public StructuralSpecs structural_specs;
    }

    [Serializable]
    public class TangentBlueprint
    {
        public List<string> requested_context_filters;
        public int assets_compiled;
        public List<CatalogAssetManifest> manifest;
    }

    // =========================================================================
    // RUNTIME INTERACTION ORCHESTRATOR
    // =========================================================================
    public class RuntimeAssetWatcher : MonoBehaviour
    {
        [Header("Scene Node References")]
        [SerializeField] private Light targetSceneLight;
        [SerializeField] private Transform characterRootTransform;

        /// <summary>
        /// Entry point to ingest a raw JSON stream payload from the Curator Engine.
        /// </summary>
        public void IngestCatalogBlueprint(string rawJsonPayload)
        {
            if (string.IsNullOrEmpty(rawJsonPayload))
            {
                Debug.LogWarning("[GCC] Received null or empty catalog blueprint payload.");
                return;
            }

            try
            {
                // Deserialize using Unity's performant native JsonUtility
                TangentBlueprint blueprint = JsonUtility.FromJson<TangentBlueprint>(rawJsonPayload);
                Debug.Log($"[GCC] Successfully parsed blueprint. Assets compiled: {blueprint.assets_compiled}");

                ExecuteAssetModifications(blueprint);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GCC] Failed to parse runtime structural catalog specs: {ex.Message}");
            }
        }

        /// <summary>
        /// Iterates through the manifest layer and mutates state on active scene actors.
        /// </summary>
        private void ExecuteAssetModifications(TangentBlueprint blueprint)
        {
            foreach (var asset in blueprint.manifest)
            {
                Debug.Log($"[GCC] Processing dynamic modification path for ID: {asset.asset_id} ({asset.asset_type})");

                switch (asset.asset_type.ToLower())
                {
                    case "item":
                        ApplyPropModifications(asset.structural_specs);
                        break;

                    case "character":
                        ApplyCharacterModifications(asset.structural_specs);
                        break;

                    default:
                        Debug.LogWarning($"[GCC] Asset type '{asset.asset_type}' currently unhandled by runtime compiler.");
                        break;
                }
            }
        }

        private void ApplyPropModifications(StructuralSpecs specs)
        {
            if (targetSceneLight == null) return;

            // Dynamically alter scene atmosphere properties based on catalog rules
            if (!string.IsNullOrEmpty(specs.emissive_color))
            {
                if (ColorUtility.TryParseHtmlString(specs.emissive_color, out Color dynamicColor))
                {
                    targetSceneLight.color = dynamicColor;
                    Debug.Log($"[GCC] Light emission runtime override applied: {specs.emissive_color}");
                }
            }
        }

        private void ApplyCharacterModifications(StructuralSpecs specs)
        {
            if (characterRootTransform == null) return;

            // Translate cm unit variations directly into real-time Unity spatial local scales
            if (specs.base_height_cm > 0)
            {
                float calculatedScaleModifier = specs.base_height_cm / 185.0f; // Scale relative to template base 
                characterRootTransform.localScale = new Vector3(calculatedScaleModifier, calculatedScaleModifier, calculatedScaleModifier);
                Debug.Log($"[GCC] Scale optimization pipeline executed for: {specs.name} ({specs.base_height_cm}cm)");
            }
        }
    }
}
