using UnityEngine;

public static class BattleUiSkinRenderer
{
    private static readonly Rect FullUv = new Rect(0f, 0f, 1f, 1f);

    public static bool DrawSkinnedPanel(Rect rect, BattleUiSkinDefinition.GraphicSlot slot, Color fallbackColor)
    {
        if (TryDrawGraphic(rect, slot))
        {
            return true;
        }

        DrawSolidRect(rect, fallbackColor);
        return false;
    }

    public static bool DrawSkinnedButtonBackground(Rect rect, BattleUiSkinButtonState state, BattleUiSkinDefinition skin, Color fallbackColor)
    {
        BattleUiSkinDefinition.GraphicSlot slot = skin != null ? skin.GetCommandButtonSlot(state) : null;
        return DrawSkinnedPanel(rect, slot, fallbackColor);
    }

    public static bool DrawSkinnedAccent(Rect rect, BattleUiSkinDefinition.GraphicSlot slot, Color fallbackColor)
    {
        return DrawSkinnedPanel(rect, slot, fallbackColor);
    }

    public static bool TryDrawGraphic(Rect rect, BattleUiSkinDefinition.GraphicSlot slot)
    {
        if (slot == null || !slot.HasAssignedGraphic)
        {
            return false;
        }

        Event current = Event.current;
        if (current != null && current.type != EventType.Repaint)
        {
            return true;
        }

        Texture texture = ResolveTexture(slot);
        if (texture == null)
        {
            return false;
        }

        Rect drawRect = slot.PreserveAspect
            ? FitRectPreserveAspect(rect, ResolveGraphicSize(slot, texture))
            : rect;

        Color previousColor = GUI.color;
        GUI.color = slot.Tint;
        if (slot.Sprite != null)
        {
            GUI.DrawTextureWithTexCoords(drawRect, texture, GetSpriteUv(slot.Sprite), true);
        }
        else
        {
            GUI.DrawTexture(drawRect, texture, ScaleMode.StretchToFill, true);
        }

        GUI.color = previousColor;
        return true;
    }

    public static void DrawSprite(Rect rect, Sprite sprite, Color tint)
    {
        if (sprite == null)
        {
            return;
        }

        Event current = Event.current;
        if (current != null && current.type != EventType.Repaint)
        {
            return;
        }

        Texture texture = sprite.texture;
        if (texture == null)
        {
            return;
        }

        Color previousColor = GUI.color;
        GUI.color = tint;
        GUI.DrawTextureWithTexCoords(rect, texture, GetSpriteUv(sprite), true);
        GUI.color = previousColor;
    }

    private static void DrawSolidRect(Rect rect, Color color)
    {
        Event current = Event.current;
        if (current != null && current.type != EventType.Repaint)
        {
            return;
        }

        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private static Texture ResolveTexture(BattleUiSkinDefinition.GraphicSlot slot)
    {
        if (slot == null)
        {
            return null;
        }

        if (slot.Sprite != null)
        {
            return slot.Sprite.texture;
        }

        return slot.Texture;
    }

    private static Vector2 ResolveGraphicSize(BattleUiSkinDefinition.GraphicSlot slot, Texture texture)
    {
        if (slot != null && slot.Sprite != null)
        {
            Rect textureRect = slot.Sprite.textureRect;
            return new Vector2(Mathf.Max(1f, textureRect.width), Mathf.Max(1f, textureRect.height));
        }

        return texture != null
            ? new Vector2(Mathf.Max(1f, texture.width), Mathf.Max(1f, texture.height))
            : Vector2.one;
    }

    private static Rect FitRectPreserveAspect(Rect targetRect, Vector2 size)
    {
        if (size.x <= 0f || size.y <= 0f)
        {
            return targetRect;
        }

        float scale = Mathf.Min(targetRect.width / size.x, targetRect.height / size.y);
        float width = size.x * scale;
        float height = size.y * scale;
        return new Rect(
            targetRect.x + ((targetRect.width - width) * 0.5f),
            targetRect.y + ((targetRect.height - height) * 0.5f),
            width,
            height);
    }

    private static Rect GetSpriteUv(Sprite sprite)
    {
        if (sprite == null || sprite.texture == null)
        {
            return FullUv;
        }

        Rect textureRect = sprite.textureRect;
        Texture texture = sprite.texture;
        if (texture.width <= 0f || texture.height <= 0f)
        {
            return FullUv;
        }

        return new Rect(
            textureRect.x / texture.width,
            textureRect.y / texture.height,
            textureRect.width / texture.width,
            textureRect.height / texture.height);
    }
}
