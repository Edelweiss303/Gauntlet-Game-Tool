using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

public class PlayerEditor
{
    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

    VisualElement levelRootElement;


    public void OnOpen(VisualElement root)
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Player/PlayerEditor.uss");
        root.styleSheets.Add(styleSheet);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Player/PlayerEditor.uxml");
        levelRootElement = visualTree.CloneTree();
        root.Add(levelRootElement);


        var mapObjectField = levelRootElement.Q<ObjectField>("playerObjectField");
        mapObjectField.objectType = typeof(Map);

    }

}