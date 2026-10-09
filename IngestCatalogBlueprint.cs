using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GCC.Core
{
    [RequireComponent(typeof(RuntimeAssetWatcher))]
    public class CatalogNetworkClient : MonoBehaviour
    {
        [Header("Pipeline Connection")]
        [Tooltip("Target local or cloud Python server wrapper routing the Curator blueprint stream.")]
        [SerializeField] private string curatorEndpointUrl = "http://127.0.0";
        
        [Tooltip("Frequency in seconds to poll the server for state updates.")]
        [SerializeField] private float pollingIntervalSeconds = 2.0f;

        private RuntimeAssetWatcher assetWatcher;
        private bool isStreaming = false;

        private void Awake()
        {
            // Cache reference to the runtime state manager
            assetWatcher = GetComponent<RuntimeAssetWatcher>();
        }

        private void Start()
        {
            StartNetworkStream();
        }

        private void OnDestroy()
        {
            StopNetworkStream();
        }

        public void StartNetworkStream()
        {
            if (isStreaming) return;
            
            isStreaming = true;
            StartCoroutine(FetchCatalogBlueprintLoop());
            Debug.Log($"[GCC] Network network interface client initialized targeting: {curatorEndpointUrl}");
        }

        public void StopNetworkStream()
        {
            isStreaming = false;
            StopAllCoroutines();
            Debug.Log("[GCC] Network interface client stream disconnected safely.");
        }

        /// <summary>
        /// Continuous asynchronous networking loop to fetch real-time state.
        /// </summary>
        private IEnumerator FetchCatalogBlueprintLoop()
        {
            while (isStreaming)
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(curatorEndpointUrl))
                {
                    // Send the request and yield control back until download completes
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        string rawJsonData = webRequest.downloadHandler.text;
                        
                        // Hand off the valid payload to the schema parser layer
                        assetWatcher.IngestCatalogBlueprint(rawJsonData);
                    }
                    else
                    {
                        // Mask detailed socket errors cleanly to prevent engine log clutter
                        Debug.LogWarning($"[GCC Network Error]: Unable to sync context matrix over connection path.");
                    }
                }

                // Wait for the duration of the polling window before pulling fresh state definitions
                yield return new WaitForSeconds(pollingIntervalSeconds);
            }
        }
    }
}
