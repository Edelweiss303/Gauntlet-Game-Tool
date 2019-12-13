using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

public class WeaponEditor
{
    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

    VisualElement tileRootElement;
    Sprite selectedSprite;
    Rect textureRect;

    int displayWidth = 128;
    int displayHeight = 128;

    public void OnOpen(VisualElement root)
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Weapon/WeaponEditor.uss");
        root.styleSheets.Add(styleSheet);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Weapon/WeaponEditor.uxml");
        tileRootElement = visualTree.CloneTree();
        root.Add(tileRootElement);

        var tileSpriteField = tileRootElement.Q<ObjectField>("tileSpriteField");
        tileSpriteField.objectType = typeof(Sprite);

        var spriteDisplay = tileRootElement.Q<IMGUIContainer>("spriteDisplay");
        spriteDisplay.onGUIHandler = DrawSprite;

        var saveButton = tileRootElement.Q<Button>("saveButton");

        saveButton.RegisterCallback<MouseUpEvent>(CreateTile);
        tileSpriteField.RegisterCallback <ChangeEvent<Object>>(SpriteFieldCallBack);

    }

    private void SpriteFieldCallBack(ChangeEvent<Object> evt)
    {
        selectedSprite = evt.newValue as Sprite;
    }

    private void DrawSprite()
    {
        if (selectedSprite != null)
        {
            textureRect = selectedSprite.textureRect;

            textureRect.x *= 0.0015625f;
            textureRect.y *= 0.0015625f;
            textureRect.width *= 0.0015625f;
            textureRect.height *= 0.0015625f;

            GUI.DrawTextureWithTexCoords(new Rect(0, 0, displayWidth, displayHeight), selectedSprite.texture, textureRect);
        }
    }

    private void CreateTile(MouseUpEvent evt)
    {
        if (selectedSprite != null)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = selectedSprite;
            tile.rect = selectedSprite.textureRect;
            AssetDatabase.CreateAsset(tile, "Assets/Resources/Tiles/" + selectedSprite.name + ".asset");
            AssetDatabase.SaveAssets();
        }
    }
}