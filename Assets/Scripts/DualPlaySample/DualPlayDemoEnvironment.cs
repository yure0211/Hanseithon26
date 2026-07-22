using UnityEngine;

namespace Hanseithon.DualPlaySample
{
    internal static class DualPlayRuntimeSprite
    {
        private static Sprite sprite;

        public static Sprite Get()
        {
            if (sprite != null)
            {
                return sprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "DualPlayRuntimeTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            sprite.name = "DualPlayRuntimeSprite";
            return sprite;
        }
    }

    public sealed class DualPlayDemoEnvironment : MonoBehaviour
    {
        private void Awake()
        {
            CreateBlock("PlayArea", new Vector3(0f, 0f, 2f), new Vector3(11f, 7.2f, 1f), new Color(0.055f, 0.075f, 0.13f, 1f), -20);
            CreateBlock("HostZone", new Vector3(-2.75f, 0f, 1f), new Vector3(5.35f, 6.8f, 1f), new Color(0.12f, 0.25f, 0.34f, 0.55f), -15);
            CreateBlock("ClientZone", new Vector3(2.75f, 0f, 1f), new Vector3(5.35f, 6.8f, 1f), new Color(0.12f, 0.34f, 0.27f, 0.55f), -15);
            CreateBlock("CenterLine", new Vector3(0f, 0f, 0.5f), new Vector3(0.06f, 6.8f, 1f), new Color(0.65f, 0.75f, 0.9f, 0.35f), -10);
            CreateBlock("Floor", new Vector3(0f, -3.35f, 0f), new Vector3(11f, 0.18f, 1f), new Color(0.4f, 0.5f, 0.7f, 0.85f), -8);
            CreateBlock("SharedGoal", new Vector3(0f, 2.75f, 0f), new Vector3(1.4f, 0.3f, 1f), new Color(1f, 0.78f, 0.22f, 1f), -5);
        }

        private void CreateBlock(string objectName, Vector3 localPosition, Vector3 localScale, Color color, int sortingOrder)
        {
            GameObject block = new GameObject(objectName);
            block.transform.SetParent(transform, false);
            block.transform.localPosition = localPosition;
            block.transform.localScale = localScale;

            SpriteRenderer spriteRenderer = block.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = DualPlayRuntimeSprite.Get();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }
}