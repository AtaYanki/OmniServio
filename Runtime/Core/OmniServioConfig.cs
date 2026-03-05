using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Omni.Servio
{
    public class OmniServioConfig : ScriptableObject
    {
        [Header("Dependency Injection Settings")]
        [Tooltip("Default exception handler mode for dependency injection errors.")]
        [SerializeField] private InjectionExceptionHandlerMode defaultExceptionHandlerMode = InjectionExceptionHandlerMode.Warning;

        [Header("Bootstrap Scene Settings")]
        [SerializeField] private string globalBootstrapSceneName;

#if UNITY_EDITOR
        [Tooltip("Reference to the global bootstrap scene. This scene will be loaded first to initialize services.")]
        [SerializeField] private SceneAsset globalBootstrapScene;
#endif

        [Tooltip("Whether to automatically load the bootstrap scene when entering Play mode in the editor.")]
        [SerializeField] private bool autoLoadBootstrapSceneInEditor = true;

        [Tooltip("Whether to load the bootstrap scene at runtime (for builds).")]
        [SerializeField] private bool loadBootstrapSceneAtRuntime = false;

        public InjectionExceptionHandlerMode DefaultExceptionHandlerMode => defaultExceptionHandlerMode;

#if UNITY_EDITOR
        public SceneAsset GlobalBootstrapScene => globalBootstrapScene;
#endif

        public string GlobalBootstrapSceneName => globalBootstrapSceneName;

        public bool AutoLoadBootstrapSceneInEditor => autoLoadBootstrapSceneInEditor;

        public bool LoadBootstrapSceneAtRuntime => loadBootstrapSceneAtRuntime;

        public string GetBootstrapScenePath()
        {
#if UNITY_EDITOR
            if (globalBootstrapScene == null)
            {
                return null;
            }
            return AssetDatabase.GetAssetPath(globalBootstrapScene);
#else
            return null;
#endif
        }

        public string GetBootstrapSceneName()
        {
            return globalBootstrapSceneName;
        }

        public void SetExceptionHandlerMode(InjectionExceptionHandlerMode mode)
        {
            defaultExceptionHandlerMode = mode;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        public void SetGlobalBootstrapScene(SceneAsset scene)
        {
            globalBootstrapScene = scene;
            if (scene != null)
                globalBootstrapSceneName = scene.name;
            EditorUtility.SetDirty(this);
        }

        private void OnValidate()
        {
            if (globalBootstrapScene != null)
                globalBootstrapSceneName = globalBootstrapScene.name;
        }
#endif

        public void SetGlobalBootstrapSceneName(string sceneName)
        {
            globalBootstrapSceneName = sceneName;
        }

        public static OmniServioConfig Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

#if UNITY_EDITOR
                _instance = Resources.Load<OmniServioConfig>("OmniServioConfig");

                if (_instance == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:OmniServioConfig");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        _instance = AssetDatabase.LoadAssetAtPath<OmniServioConfig>(path);
                    }
                }
#else
                _instance = Resources.Load<OmniServioConfig>("OmniServioConfig");
#endif

                return _instance;
            }
        }

        private static OmniServioConfig _instance;

        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }
    }
}
