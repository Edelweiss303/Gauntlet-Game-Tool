using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class MainEditor : EditorWindow, IBindable
{
    public enum EditorType
    {
        Level,
        Player,
        Enemy,
        Weapon,
        Assets
    }
    static MainEditor _window;

    VisualElement mainRootElement;

    IMGUIContainer mapGridContainer;
    ObjectField mapObjectField;

    LevelEditor levelEditor = new LevelEditor();
    PlayerEditor playerEditor = new PlayerEditor();
    EnemyEditor enemyEditor = new EnemyEditor();
    WeaponEditor weaponEditor = new WeaponEditor();
    AssetEditor assetEditor = new AssetEditor();

    Button levelButton;
    Button playerButton;
    Button enemyButton;
    Button weaponButton;
    Button assetButton;

    [Shortcut("Refresh Gauntlet Editor", KeyCode.F9)]
    [MenuItem("Tools/Gauntlet Editor")]
    public static void ShowExample()
    {
        if (_window) _window.Close();

        _window = GetWindow<MainEditor>();
        _window.titleContent = new GUIContent("Gauntlet Editor");
        _window.minSize = new Vector2(600, 750);
    }

    public static MainEditor getWindow()
    {
        return _window;
    }

    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

    public void OnEnable()
    {

        var mainStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/MainMenu.uss");
        rootVisualElement.styleSheets.Add(mainStyleSheet);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/MainMenu.uxml");
        mainRootElement = visualTree.CloneTree();
        rootVisualElement.Add(mainRootElement);

        levelButton = mainRootElement.Q<Button>("levelButton");
        levelButton.RegisterCallback<MouseUpEvent, EditorType>(MenuCallBack, EditorType.Level);

        playerButton = mainRootElement.Q<Button>("playerButton");
        playerButton.RegisterCallback<MouseUpEvent, EditorType>(MenuCallBack, EditorType.Player);

        enemyButton = mainRootElement.Q<Button>("enemyButton");
        enemyButton.RegisterCallback<MouseUpEvent, EditorType>(MenuCallBack, EditorType.Enemy);

        weaponButton = mainRootElement.Q<Button>("weaponButton");
        weaponButton.RegisterCallback<MouseUpEvent, EditorType>(MenuCallBack, EditorType.Weapon);

        assetButton = mainRootElement.Q<Button>("assetButton");
        assetButton.RegisterCallback<MouseUpEvent, EditorType>(MenuCallBack, EditorType.Assets);

        levelEditor.OnOpen(rootVisualElement);
    }


    private void MenuCallBack(MouseUpEvent evt, EditorType et)
    {
        switch (et)
        {
            case EditorType.Level:
                rootVisualElement.RemoveAt(1);
                levelEditor.OnOpen(rootVisualElement);

                levelButton.RemoveFromClassList("main-menu-button-off");
                levelButton.AddToClassList("main-menu-button-on");

                playerButton.RemoveFromClassList("main-menu-button-on");
                playerButton.AddToClassList("main-menu-button-off");

                enemyButton.RemoveFromClassList("main-menu-button-on");
                enemyButton.AddToClassList("main-menu-button-off");

                weaponButton.RemoveFromClassList("main-menu-button-on");
                weaponButton.AddToClassList("main-menu-button-off");

                assetButton.RemoveFromClassList("main-menu-button-on");
                assetButton.AddToClassList("main-menu-button-off");
                break;

            case EditorType.Player:
                rootVisualElement.RemoveAt(1);
                playerEditor.OnOpen(rootVisualElement);

                playerButton.RemoveFromClassList("main-menu-button-off");
                playerButton.AddToClassList("main-menu-button-on");

                levelButton.RemoveFromClassList("main-menu-button-on");
                levelButton.AddToClassList("main-menu-button-off");

                enemyButton.RemoveFromClassList("main-menu-button-on");
                enemyButton.AddToClassList("main-menu-button-off");

                weaponButton.RemoveFromClassList("main-menu-button-on");
                weaponButton.AddToClassList("main-menu-button-off");

                assetButton.RemoveFromClassList("main-menu-button-on");
                assetButton.AddToClassList("main-menu-button-off");
                break;

            case EditorType.Enemy:
                rootVisualElement.RemoveAt(1);
                enemyEditor.OnOpen(rootVisualElement);

                enemyButton.RemoveFromClassList("main-menu-button-off");
                enemyButton.AddToClassList("main-menu-button-on");

                playerButton.RemoveFromClassList("main-menu-button-on");
                playerButton.AddToClassList("main-menu-button-off");

                levelButton.RemoveFromClassList("main-menu-button-on");
                levelButton.AddToClassList("main-menu-button-off");

                weaponButton.RemoveFromClassList("main-menu-button-on");
                weaponButton.AddToClassList("main-menu-button-off");

                assetButton.RemoveFromClassList("main-menu-button-on");
                assetButton.AddToClassList("main-menu-button-off");
                break;

            case EditorType.Weapon:
                rootVisualElement.RemoveAt(1);
                weaponEditor.OnOpen(rootVisualElement);

                weaponButton.RemoveFromClassList("main-menu-button-off");
                weaponButton.AddToClassList("main-menu-button-on");

                assetButton.RemoveFromClassList("main-menu-button-on");
                assetButton.AddToClassList("main-menu-button-off");

                playerButton.RemoveFromClassList("main-menu-button-on");
                playerButton.AddToClassList("main-menu-button-off");

                enemyButton.RemoveFromClassList("main-menu-button-on");
                enemyButton.AddToClassList("main-menu-button-off");

                levelButton.RemoveFromClassList("main-menu-button-on");
                levelButton.AddToClassList("main-menu-button-off");
                break;

            case EditorType.Assets:
                rootVisualElement.RemoveAt(1);
                assetEditor.OnOpen(rootVisualElement);

                assetButton.RemoveFromClassList("main-menu-button-off");
                assetButton.AddToClassList("main-menu-button-on");

                playerButton.RemoveFromClassList("main-menu-button-on");
                playerButton.AddToClassList("main-menu-button-off");

                enemyButton.RemoveFromClassList("main-menu-button-on");
                enemyButton.AddToClassList("main-menu-button-off");

                weaponButton.RemoveFromClassList("main-menu-button-on");
                weaponButton.AddToClassList("main-menu-button-off");

                levelButton.RemoveFromClassList("main-menu-button-on");
                levelButton.AddToClassList("main-menu-button-off");
                break;
        }
    }


    
}
