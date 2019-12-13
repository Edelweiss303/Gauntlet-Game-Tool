using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO; 
using SimpleJSON;


public class AssetEditor
{
    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

    VisualElement assetRootElement;
    Object selection;
    string fileName;

    public void OnOpen(VisualElement root)
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Assets/AssetEditor.uss");
        root.styleSheets.Add(styleSheet);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Assets/AssetEditor.uxml");
        assetRootElement = visualTree.CloneTree();
        root.Add(assetRootElement);

        var assetField = assetRootElement.Q<ObjectField>("assetField");
        assetField.objectType = typeof(ScriptableObject);
        assetField.RegisterCallback<ChangeEvent<Object>>(GetObjectField);


        var exportButton = assetRootElement.Q<Button>("exportButton");
        exportButton.RegisterCallback<MouseUpEvent>(ExportAsset);   
    }

    void GetObjectField(ChangeEvent<Object> evt)
    {
        selection = evt.newValue;
    }

    void ExportAsset(MouseUpEvent evt)
    {
        ScriptableObject scObj = selection as ScriptableObject;
        string type = scObj.GetType().ToString();
        
        switch (type)
        {
            case "Map":
                ExportLevel(scObj);
                break;

            case "Player":
                break;

            case "Enemy":
                break;

        }
    }

    void ExportLevel(ScriptableObject scObj)
    {
        Map map = scObj as Map;
        JSONObject mapJSON = new JSONObject(); //Main JSON Object

        JSONArray resources = new JSONArray(); //List of .meta files
        resources.Add("../Assets/Images/Dungeon.png.meta");
        resources.Add("../Assets/Images/Witch.png.meta");

        JSONArray gameObjects = new JSONArray(); // GameObjects in level
        for (int i = 0; i < map.tiles.Count; i++)
        {
            Tile tile = map.tiles[i]; //get reference to current tile for readability 
            JSONObject gameObject = new JSONObject(); //each gameObject

            gameObject.Add("enabled", tile.enabled);
            gameObject.Add("destroyOnUnload", tile.destroyOnUnload);


            JSONArray components = new JSONArray();

            #region create transform component
            JSONObject transform = new JSONObject();
            JSONObject position = new JSONObject();
            JSONObject scale = new JSONObject();

            position.Add("X", tile.position.x * 1.0f); //have to convert to floats
            position.Add("Y", tile.position.y * 1.0f);

            scale.Add("X", tile.scale.x * 1.0f);
            scale.Add("Y", tile.scale.y * 1.0f);

            transform.Add("class", "Transform");
            transform.Add("Position", position);
            transform.Add("Scale", scale);
            #endregion

            #region create sprite component
            JSONObject sprite = new JSONObject();
            JSONObject texture = new JSONObject();
            JSONObject dimensions = new JSONObject();

            texture.Add("textureAssetGUID", "5651edc5-61a2-4e2d-8c52-fb04ca66564d");

            dimensions.Add("Left", tile.sprite.rect.x );
            dimensions.Add("Top", (640 - tile.sprite.rect.y) - 64);
            dimensions.Add("Width", 64);
            dimensions.Add("Height", 64);

            sprite.Add("class", "Sprite");
            sprite.Add("enabled", true);
            sprite.Add("Texture", texture);
            sprite.Add("Dimensions", dimensions);
            sprite.Add("layer", tile.layer);
            #endregion

            //Add components to components array
            components.Add(transform);
            components.Add(sprite);

            //Add components array to gameObject
            gameObject.Add("Components", components);

            //Add gameObject to gameObjects array
            gameObjects.Add(gameObject);
            
        }

        mapJSON.Add("resources", resources);
        //Add gameObjects array to the map JSON 
        mapJSON.Add("GameObjects", gameObjects);

        var fileNameField = assetRootElement.Q<TextField>("fileNameField");

        if (fileNameField.text == "")
        {
            Debug.Log("Level must have a name!");
        }
        else
        {
            File.WriteAllText("Assets/Resources/JSON/" + fileNameField.text + ".json", mapJSON.ToString());
            Debug.Log("Level successfully exported!");
        }
    }

}