using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class AssetEditor
{
    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

    VisualElement assetRootElement;
    public void OnOpen(VisualElement root)
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Assets/AssetEditor.uss");
        root.styleSheets.Add(styleSheet);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Assets/AssetEditor.uxml");
        assetRootElement = visualTree.CloneTree();
        root.Add(assetRootElement);

        var spriteSheetField = assetRootElement.Q<ObjectField>("spriteSheetField");
        spriteSheetField.objectType = typeof(Texture2D);
    }
}