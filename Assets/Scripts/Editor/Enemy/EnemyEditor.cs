using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

public class EnemyEditor
{
    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

    VisualElement enemyRootElement;

    public void OnOpen(VisualElement root)
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Enemy/EnemyEditor.uss");
        root.styleSheets.Add(styleSheet);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Enemy/EnemyEditor.uxml");
        enemyRootElement = visualTree.CloneTree();
        root.Add(enemyRootElement);

        var mapListView = enemyRootElement.Q<ListView>("enemyListView");

        var mapObjectField = enemyRootElement.Q<ObjectField>("enemyObjectField");
        mapObjectField.objectType = typeof(Map);
    }
}