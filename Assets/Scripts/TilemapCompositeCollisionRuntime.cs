using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Hanseithon.Gameplay
{
    /// <summary>
    /// Ensures every solid tilemap uses one static composite collider in builds too.
    /// Scene assets are left untouched so ongoing level edits are preserved.
    /// </summary>
    internal static class TilemapCompositeCollisionRuntime
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneCallback()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ConfigureInitialScene()
        {
            ConfigureScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            ConfigureScene(scene);
        }

        private static void ConfigureScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                TilemapCollider2D[] tilemapColliders =
                    roots[rootIndex].GetComponentsInChildren<TilemapCollider2D>(true);

                for (int colliderIndex = 0;
                     colliderIndex < tilemapColliders.Length;
                     colliderIndex++)
                {
                    TilemapCollider2D tilemapCollider = tilemapColliders[colliderIndex];
                    if (!tilemapCollider.enabled || tilemapCollider.isTrigger)
                    {
                        continue;
                    }

                    GameObject tilemapObject = tilemapCollider.gameObject;
                    Rigidbody2D body = tilemapObject.GetComponent<Rigidbody2D>();
                    if (body == null)
                    {
                        body = tilemapObject.AddComponent<Rigidbody2D>();
                    }

                    body.bodyType = RigidbodyType2D.Static;
                    body.simulated = true;

                    CompositeCollider2D composite =
                        tilemapObject.GetComponent<CompositeCollider2D>();
                    if (composite == null)
                    {
                        composite = tilemapObject.AddComponent<CompositeCollider2D>();
                    }

                    composite.isTrigger = false;
                    composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
                    tilemapCollider.compositeOperation =
                        Collider2D.CompositeOperation.Merge;
                    tilemapCollider.ProcessTilemapChanges();
                }
            }

            Physics2D.SyncTransforms();
        }
    }
}
