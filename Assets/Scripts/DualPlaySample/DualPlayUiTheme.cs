using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hanseithon.UI
{
    /// <summary>
    /// Existing Sprite-00012 pixel frame and NeoDunggeunmo font based UI helpers.
    /// It never replaces unrelated scene images or creates a separate visual theme.
    /// </summary>
    public static class DualPlayUiTheme
    {
        public const float VirtualWidth = 1280f;
        public const float VirtualHeight = 720f;

        private static readonly Color Ink = new Color32(45, 45, 45, 255);
        private static readonly Color Paper = new Color32(244, 241, 236, 255);
        private static readonly Color MutedInk = new Color32(92, 92, 92, 255);

        private static Sprite pixelFrame;
        private static Font uiFont;
        private static Texture2D frameTexture;
        private static GUIStyle panelStyle;
        private static GUIStyle titleStyle;
        private static GUIStyle headerStyle;
        private static GUIStyle subtitleStyle;
        private static GUIStyle labelStyle;
        private static GUIStyle centeredLabelStyle;
        private static GUIStyle captionStyle;
        private static GUIStyle buttonStyle;
        private static GUIStyle secondaryButtonStyle;
        private static GUIStyle textFieldStyle;
        private static GUIStyle statusStyle;
        private static GUIStyle hintStyle;

        public static GUIStyle PanelStyle { get { EnsureStyles(); return panelStyle; } }
        public static GUIStyle TitleStyle { get { EnsureStyles(); return titleStyle; } }
        public static GUIStyle HeaderStyle { get { EnsureStyles(); return headerStyle; } }
        public static GUIStyle SubtitleStyle { get { EnsureStyles(); return subtitleStyle; } }
        public static GUIStyle LabelStyle { get { EnsureStyles(); return labelStyle; } }
        public static GUIStyle CenteredLabelStyle { get { EnsureStyles(); return centeredLabelStyle; } }
        public static GUIStyle CaptionStyle { get { EnsureStyles(); return captionStyle; } }
        public static GUIStyle ButtonStyle { get { EnsureStyles(); return buttonStyle; } }
        public static GUIStyle SecondaryButtonStyle { get { EnsureStyles(); return secondaryButtonStyle; } }
        public static GUIStyle TextFieldStyle { get { EnsureStyles(); return textFieldStyle; } }
        public static GUIStyle StatusStyle { get { EnsureStyles(); return statusStyle; } }
        public static GUIStyle HintStyle { get { EnsureStyles(); return hintStyle; } }

        public static void UseAssets(Sprite frame, Font font)
        {
            bool needsRebuild = panelStyle == null || pixelFrame != frame || uiFont != font;
            pixelFrame = frame;
            uiFont = font;

            if (needsRebuild)
            {
                BuildStyles();
            }
        }

        public static Matrix4x4 BeginCanvas(bool drawBackdrop)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;

            if (drawBackdrop)
            {
                Color previousColor = GUI.color;
                GUI.color = new Color32(25, 27, 31, 255);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = previousColor;
            }

            float scale = Mathf.Min(Screen.width / VirtualWidth, Screen.height / VirtualHeight);
            scale = Mathf.Max(0.01f, scale);
            float offsetX = (Screen.width - VirtualWidth * scale) * 0.5f;
            float offsetY = (Screen.height - VirtualHeight * scale) * 0.5f;
            GUI.matrix = Matrix4x4.TRS(
                new Vector3(offsetX, offsetY, 0f),
                Quaternion.identity,
                new Vector3(scale, scale, 1f));
            return previousMatrix;
        }

        public static void EndCanvas(Matrix4x4 previousMatrix)
        {
            GUI.matrix = previousMatrix;
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
            GUI.enabled = true;
        }

        public static Rect CenteredPanel(float width, float height)
        {
            return new Rect(
                (VirtualWidth - width) * 0.5f,
                (VirtualHeight - height) * 0.5f,
                width,
                height);
        }

        public static void DrawSprite(Rect area, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
            {
                return;
            }

            Rect textureRect = sprite.textureRect;
            Rect uv = new Rect(
                textureRect.x / sprite.texture.width,
                textureRect.y / sprite.texture.height,
                textureRect.width / sprite.texture.width,
                textureRect.height / sprite.texture.height);

            float spriteAspect = textureRect.width / textureRect.height;
            float areaAspect = area.width / area.height;
            Rect drawRect = area;
            if (areaAspect > spriteAspect)
            {
                drawRect.width = area.height * spriteAspect;
                drawRect.x += (area.width - drawRect.width) * 0.5f;
            }
            else
            {
                drawRect.height = area.width / spriteAspect;
                drawRect.y += (area.height - drawRect.height) * 0.5f;
            }

            GUI.DrawTextureWithTexCoords(drawRect, sprite.texture, uv, true);
        }

        public static void StyleSceneButtons(Sprite frame, TMP_FontAsset font)
        {
            if (frame == null)
            {
                return;
            }

            Button[] buttons = Object.FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                Image image = button.targetGraphic as Image;
                if (image == null)
                {
                    image = button.GetComponent<Image>();
                }

                if (image != null)
                {
                    image.sprite = frame;
                    image.type = Image.Type.Sliced;
                    image.color = Color.white;
                    image.pixelsPerUnitMultiplier = 1f;
                }

                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color32(230, 230, 230, 255);
                colors.pressedColor = new Color32(185, 185, 185, 255);
                colors.selectedColor = new Color32(220, 220, 220, 255);
                colors.disabledColor = new Color32(120, 120, 120, 150);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.05f;
                button.colors = colors;

                TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
                for (int labelIndex = 0; labelIndex < labels.Length; labelIndex++)
                {
                    labels[labelIndex].color = Ink;
                    if (font != null)
                    {
                        labels[labelIndex].font = font;
                    }
                }
            }
        }

        private static void BuildStyles()
        {
            frameTexture = pixelFrame != null ? ExtractSpriteTexture(pixelFrame) : Texture2D.whiteTexture;

            panelStyle = new GUIStyle
            {
                normal = { background = frameTexture },
                border = new RectOffset(8, 8, 8, 8),
                padding = new RectOffset(34, 34, 28, 28)
            };

            titleStyle = CreateLabelStyle(44, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            headerStyle = CreateLabelStyle(30, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            subtitleStyle = CreateLabelStyle(18, MutedInk, TextAnchor.MiddleCenter, FontStyle.Normal);
            labelStyle = CreateLabelStyle(18, Ink, TextAnchor.MiddleLeft, FontStyle.Normal);
            centeredLabelStyle = CreateLabelStyle(18, Ink, TextAnchor.MiddleCenter, FontStyle.Normal);
            captionStyle = CreateLabelStyle(14, MutedInk, TextAnchor.MiddleCenter, FontStyle.Normal);

            buttonStyle = new GUIStyle
            {
                normal = { background = frameTexture, textColor = Ink },
                hover = { background = frameTexture, textColor = Color.black },
                active = { background = frameTexture, textColor = MutedInk },
                focused = { background = frameTexture, textColor = Color.black },
                alignment = TextAnchor.MiddleCenter,
                font = uiFont,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                border = new RectOffset(8, 8, 8, 8),
                padding = new RectOffset(16, 16, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };
            secondaryButtonStyle = new GUIStyle(buttonStyle);

            textFieldStyle = new GUIStyle
            {
                normal = { background = frameTexture, textColor = Ink },
                focused = { background = frameTexture, textColor = Color.black },
                alignment = TextAnchor.MiddleLeft,
                font = uiFont,
                fontSize = 20,
                border = new RectOffset(8, 8, 8, 8),
                padding = new RectOffset(16, 16, 10, 10)
            };

            statusStyle = CreateLabelStyle(16, Ink, TextAnchor.MiddleCenter, FontStyle.Normal);
            statusStyle.normal.background = frameTexture;
            statusStyle.border = new RectOffset(8, 8, 8, 8);
            statusStyle.padding = new RectOffset(14, 14, 8, 8);

            hintStyle = new GUIStyle(statusStyle)
            {
                fontStyle = FontStyle.Bold
            };
        }

        private static void EnsureStyles()
        {
            if (panelStyle == null)
            {
                BuildStyles();
            }
        }

        private static GUIStyle CreateLabelStyle(
            int fontSize,
            Color color,
            TextAnchor alignment,
            FontStyle fontStyle)
        {
            return new GUIStyle
            {
                font = uiFont,
                fontSize = fontSize,
                fontStyle = fontStyle,
                alignment = alignment,
                wordWrap = true,
                normal = { textColor = color }
            };
        }

        private static Texture2D ExtractSpriteTexture(Sprite sprite)
        {
            Rect rect = sprite.textureRect;
            int width = Mathf.Max(1, Mathf.RoundToInt(rect.width));
            int height = Mathf.Max(1, Mathf.RoundToInt(rect.height));
            RenderTexture renderTexture = RenderTexture.GetTemporary(
                width,
                height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            renderTexture.filterMode = FilterMode.Point;

            Vector2 scale = new Vector2(
                rect.width / sprite.texture.width,
                rect.height / sprite.texture.height);
            Vector2 offset = new Vector2(
                rect.x / sprite.texture.width,
                rect.y / sprite.texture.height);
            Graphics.Blit(sprite.texture, renderTexture, scale, offset);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Sprite-00012 UI Frame Copy",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            result.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
            result.Apply(false, false);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
            return result;
        }
    }
}
