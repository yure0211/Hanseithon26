using Hanseithon.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hanseithon.EditorTools
{
    internal static class PufferfishPrefabSetup
    {
        private const string TargetScenePath = "Assets/Scenes/InGame_4.unity";
        private const string TargetPrefabPath = "Assets/Prefabs/Blowfish.prefab";
        private const string OriginalObjectName = "blowfish";
        private const string PrefabObjectName = "Blowfish";

        [MenuItem("Tools/Hanseithon/Setup InGame 4 Pufferfish")]
        private static void ApplyToActiveScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != TargetScenePath)
            {
                return;
            }

            ApplyToScene(scene);
        }

        public static void ApplyToInGame4FromBatch()
        {
            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
            ApplyToScene(scene);
        }

        private static void ApplyToScene(Scene scene)
        {

            GameObject pufferfish = FindRootObject(scene, PrefabObjectName) ??
                                    FindRootObject(scene, OriginalObjectName);
            if (pufferfish == null)
            {
                Debug.LogError($"{TargetScenePath}에서 {OriginalObjectName} 오브젝트를 찾지 못했습니다.");
                return;
            }

            ConfigurePufferfish(pufferfish);
            pufferfish.name = PrefabObjectName;

            string currentPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(pufferfish);
            if (currentPrefabPath != TargetPrefabPath)
            {
                pufferfish = PrefabUtility.SaveAsPrefabAssetAndConnect(
                    pufferfish,
                    TargetPrefabPath,
                    InteractionMode.AutomatedAction);
            }

            if (pufferfish == null)
            {
                Debug.LogError($"복어 프리팹을 {TargetPrefabPath}에 저장하지 못했습니다.");
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = pufferfish;
            Debug.Log("InGame_4 복어를 프리팹으로 만들고 거북이 전용 게임오버 Trigger를 연결했습니다.");
        }

        private static void ConfigurePufferfish(GameObject pufferfish)
        {
            BoxCollider2D boxCollider = pufferfish.GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = pufferfish.AddComponent<BoxCollider2D>();
            }

            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(1.05f, 0.95f);

            if (pufferfish.GetComponent<PufferfishHazard>() == null)
            {
                pufferfish.AddComponent<PufferfishHazard>();
            }
        }

        private static GameObject FindRootObject(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == objectName)
                {
                    return root;
                }
            }

            return null;
        }
    }
}
