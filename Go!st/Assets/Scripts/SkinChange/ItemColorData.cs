using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemColorData
{
    public ItemNameSlot itemName;

    [Serializable]
    public class ColorSpritePair
    {
        public ItemColorChangeSlotColor color;
        public Sprite sprite;
    }

    [SerializeField]
    private List<ColorSpritePair> colorSpriteList;

    private Dictionary<ItemColorChangeSlotColor, Sprite> spriteDict;

    public void Initialize()
    {
        spriteDict = new Dictionary<ItemColorChangeSlotColor, Sprite>();

        foreach (var pair in colorSpriteList)
        {
            if (!spriteDict.ContainsKey(pair.color))
            {
                spriteDict.Add(pair.color, pair.sprite);
            }
        }
    }

    public Sprite GetSprite(ItemColorChangeSlotColor color)
    {
        spriteDict.TryGetValue(color, out var sprite);
        return sprite;
    }
}