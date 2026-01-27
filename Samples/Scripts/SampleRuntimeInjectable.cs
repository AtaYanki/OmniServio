using UnityEngine;
using System.Reflection;
using System;
using Omni.Servio;

namespace Omni.Servio.Samples
{
    /// <summary>
    /// Example component demonstrating runtime dependency injection.
    /// This component will automatically receive services when they become available at runtime.
    /// </summary>
    public class SampleRuntimeInjectable : RuntimeInjectable
    {
        [Inject] private IAudioService _audioService;
        [Inject(UseGlobal = true)] private ISaveService _saveService;
        [Inject] private IGameplayService _gameplayService;

        private bool _hasReceivedAudioService = false;
        private bool _hasReceivedSaveService = false;
        private bool _hasReceivedGameplayService = false;

        protected override void OnServiceInjected(MemberInfo memberInfo, Type serviceType, object service)
        {
            base.OnServiceInjected(memberInfo, serviceType, service);

            // Track which services have been injected
            if (serviceType == typeof(IAudioService))
            {
                _hasReceivedAudioService = true;
                Debug.Log($"[SampleRuntimeInjectable] ✓ AudioService injected at runtime! Playing welcome sound...");
                _audioService?.PlaySound("runtime_injection_success");
            }
            else if (serviceType == typeof(ISaveService))
            {
                _hasReceivedSaveService = true;
                Debug.Log($"[SampleRuntimeInjectable] ✓ SaveService injected at runtime!");
            }
            else if (serviceType == typeof(IGameplayService))
            {
                _hasReceivedGameplayService = true;
                Debug.Log($"[SampleRuntimeInjectable] ✓ GameplayService injected at runtime!");
            }

            // Check if all services are now available
            if (_hasReceivedAudioService && _hasReceivedSaveService && _hasReceivedGameplayService)
            {
                Debug.Log("[SampleRuntimeInjectable] 🎉 All services are now available! Component is fully initialized.");
                OnAllServicesReady();
            }
        }

        private void OnAllServicesReady()
        {
            // Perform initialization that requires all services
            _audioService?.PlaySound("all_services_ready");
            _saveService?.SaveGame("runtime_init");
            _gameplayService?.StartGame();
        }

        private void Update()
        {
            // Use services once they're available
            if (Input.GetKeyDown(KeyCode.U) && _audioService != null)
            {
                _audioService.PlaySound("runtime_test");
            }

            if (Input.GetKeyDown(KeyCode.V) && _saveService != null)
            {
                _saveService.SaveGame("runtime_save");
            }
        }

        private void OnGUI()
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float panelWidth = 350f;
            float panelHeight = 200f;
            float panelX = screenWidth - panelWidth - 20f;
            float panelY = 20f;

            GUILayout.BeginArea(new Rect(panelX, panelY, panelWidth, panelHeight));

            GUILayout.Label("Runtime Injectable Sample", GUI.skin.box);

            GUILayout.Space(10);
            GUILayout.Label("=== Runtime Injection Status ===", GUI.skin.box);
            GUILayout.Label($"Audio Service: {(_hasReceivedAudioService ? "✓ Injected" : "⏳ Waiting...")}");
            GUILayout.Label($"Save Service: {(_hasReceivedSaveService ? "✓ Injected" : "⏳ Waiting...")}");
            GUILayout.Label($"Gameplay Service: {(_hasReceivedGameplayService ? "✓ Injected" : "⏳ Waiting...")}");

            GUILayout.Space(10);
            GUILayout.Label("=== Controls ===", GUI.skin.box);
            GUILayout.Label("U - Test Audio (when ready)");
            GUILayout.Label("V - Test Save (when ready)");

            GUILayout.EndArea();
        }
    }
}

