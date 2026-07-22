using Hanseithon.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hanseithon.EditorTools
{
    [InitializeOnLoad]
    internal static class TurtleCarrySkillSceneSetup
    {
        private const string TargetScenePath = "Assets/Scenes/InGame_skilldev.unity";
        private const string TurtleObjectName = "Turtle";
        private const string BoxObjectName = "TurtleCarryBox";

        static TurtleCarrySkillSceneSetup()
        {
            EditorApplication.delayCall += InstallIntoCurrentScene;
        }

        [MenuItem("Tools/Hanseithon/Setup Turtle Carry Skill")]
        private static void InstallIntoCurrentScene()
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

            bool changed = false;
            GameObject turtle = FindRootObject(scene, TurtleObjectName);
            if (turtle == null)
            {
                Debug.LogError($"{TurtleObjectName} 오브젝트를 {TargetScenePath}에서 찾지 못했습니다.");
                return;
            }

            if (turtle.GetComponent<TurtleCarrySkill>() == null)
            {
                Undo.AddComponent<TurtleCarrySkill>(turtle);
                changed = true;
            }

            if (!turtle.activeSelf)
            {
                Undo.RecordObject(turtle, "Enable Turtle For Skill Test");
                turtle.SetActive(true);
                changed = true;
            }

            GameObject carryBox = FindRootObject(scene, BoxObjectName);
            if (carryBox == null)
            {
                carryBox = CreateCarryBox();
                SceneManager.MoveGameObjectToScene(carryBox, scene);
                Undo.RegisterCreatedObjectUndo(carryBox, "Create Turtle Carry Box");
                changed = true;
            }

            changed |= EnsureCarryBoxConfiguration(carryBox);

            if (!changed)
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = carryBox;
            Debug.Log("거북이 운반 스킬과 운반 상자를 InGame_skilldev 씬에 배치했습니다.");
        }

        private static GameObject CreateCarryBox()
        {
            GameObject carryBox = new GameObject(BoxObjectName);
            carryBox.transform.position = new Vector3(1.2f, -0.06f, 0f);
            carryBox.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            SpriteRenderer spriteRenderer = carryBox.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            spriteRenderer.color = new Color(0.55f, 0.32f, 0.14f, 1f);
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = Vector2.one;
            spriteRenderer.sortingOrder = 2;

            Rigidbody2D body = carryBox.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.linearDamping = 3f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            BoxCollider2D boxCollider = carryBox.AddComponent<BoxCollider2D>();
            boxCollider.size = Vector2.one;

            carryBox.AddComponent<TurtleCarryableBox>();
            return carryBox;
        }

        private static bool EnsureCarryBoxConfiguration(GameObject carryBox)
        {
            SpriteRenderer spriteRenderer = carryBox.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null ||
                (spriteRenderer.drawMode == SpriteDrawMode.Sliced && spriteRenderer.size == Vector2.one))
            {
                return false;
            }

            Undo.RecordObject(spriteRenderer, "Match Carry Box Visual To Collider");
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = Vector2.one;
            return true;
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
