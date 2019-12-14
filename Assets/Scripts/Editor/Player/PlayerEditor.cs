using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

public class PlayerEditor
{
    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

    VisualElement playerRootElement;

    public Sprite selectedSprite;
    public Weapon selectedWeapon;
    Rect textureRect;
    int displayWidth = 128;
    int displayHeight = 128;
    
    public void OnOpen(VisualElement root)
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Player/PlayerEditor.uss");
        root.styleSheets.Add(styleSheet);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Player/PlayerEditor.uxml");
        playerRootElement = visualTree.CloneTree();
        root.Add(playerRootElement);

        var playerSpriteField = playerRootElement.Q<ObjectField>("playerSpriteField");
        playerSpriteField.objectType = typeof(Sprite);
        playerSpriteField.RegisterCallback<ChangeEvent<Object>>(SpriteFieldCallback);

        var playerWeaponField = playerRootElement.Q<ObjectField>("playerWeaponField");
        playerWeaponField.objectType = typeof(Weapon);
        playerWeaponField.RegisterCallback<ChangeEvent<Object>>(WeaponFieldCallback);

        var spriteDisplay = playerRootElement.Q<IMGUIContainer>("spriteDisplay");
        spriteDisplay.onGUIHandler = DrawSprite;

        var saveButton = playerRootElement.Q<Button>("saveButton");
        saveButton.RegisterCallback<MouseUpEvent>(CreatePlayer);
    }

    private void DrawSprite()
    {
        if (selectedSprite != null)
        {
            textureRect = selectedSprite.textureRect;

            textureRect.x *= 1.0f / selectedSprite.texture.width;
            textureRect.y *= 1.0f / selectedSprite.texture.height;
            textureRect.width *= 1.0f / selectedSprite.texture.width;
            textureRect.height *= 1.0f / selectedSprite.texture.height;

            GUI.DrawTextureWithTexCoords(new Rect(0, 0, displayWidth, displayHeight), selectedSprite.texture, textureRect);
        }
    }

    private void SpriteFieldCallback(ChangeEvent<Object> evt)
    {
        selectedSprite = evt.newValue as Sprite;
    }

    private void WeaponFieldCallback(ChangeEvent<Object> evt)
    {
        selectedWeapon = evt.newValue as Weapon;
    }

    private void CreatePlayer(MouseUpEvent evt)
    {
        var playerSpeedField = playerRootElement.Q<IntegerField>("playerSpeedField");
        var playerNameField = playerRootElement.Q<TextField>("playerNameField");
        var playerRigidBodyField = playerRootElement.Q<IntegerField>("playerRigidBodyField");
        var playerRadiusField = playerRootElement.Q<IntegerField>("playerRadiusField");
        var playerTriggerToggle = playerRootElement.Q<Toggle>("playerTriggerToggle");

        Player player = ScriptableObject.CreateInstance<Player>();
        player.sprite = selectedSprite;
        player.name = playerNameField.text;
        player.moveSpeed = playerSpeedField.value;
        player.bodyType = playerRigidBodyField.value;
        player.layer = 3;

        player.radius = playerRadiusField.value;
        player.trigger = playerTriggerToggle.value;

        EditorUtility.SetDirty(player);
        AssetDatabase.CreateAsset(player, "Assets/Resources/Players/" + player.name + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }
}