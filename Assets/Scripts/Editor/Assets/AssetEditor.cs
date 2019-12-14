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
                ExportPlayer(scObj);
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
        resources.Add("../Assets/Images/Explosion.png.meta");
        resources.Add("../Assets/Prefabs/projectile.json.meta");

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

            position.Add("X", tile.position.x); //have to convert to floats
            position.Add("Y", tile.position.y);
            Debug.Log(tile.position);
            scale.Add("X", tile.scale.x * 1.0f);
            scale.Add("Y", tile.scale.y * 1.0f);

            transform.Add("class", "Transform");
            transform.Add("Position", position);
            transform.Add("Scale", scale);

            components.Add(transform);
            #endregion

            #region create sprite component
            if (tile.sprite != null)
            {
                JSONObject sprite = new JSONObject();
                JSONObject texture = new JSONObject();
                JSONObject dimensions = new JSONObject();

                texture.Add("textureAssetGUID", "5651edc5-61a2-4e2d-8c52-fb04ca66564d");

                dimensions.Add("Left", tile.sprite.rect.x);
                dimensions.Add("Top", (640 - tile.sprite.rect.y) - 64);
                dimensions.Add("Width", 64);
                dimensions.Add("Height", 64);

                sprite.Add("class", "Sprite");
                sprite.Add("enabled", true);
                sprite.Add("Texture", texture);
                sprite.Add("Dimensions", dimensions);
                sprite.Add("Layer", tile.layer);

                components.Add(sprite);
            }
            #endregion

            #region create collider component

            if (map.tiles[i].radius != 0)
            {
                JSONObject collider = new JSONObject();
                collider.Add("class", "CircleCollider");
                collider.Add("enabled", true);
                collider.Add("radius", map.tiles[i].radius);
                collider.Add("trigger", map.tiles[i].isTrigger);
                components.Add(collider);
            }
            else if(map.tiles[i].boxHeight != 0)
            {
                JSONObject collider = new JSONObject();
                JSONObject box = new JSONObject();
                box.Add("height", map.tiles[i].boxHeight);
                box.Add("width", map.tiles[i].boxWidth);

                collider.Add("class", "PolygonCollider");
                collider.Add("enabled", true);
                collider.Add("box", box);
                collider.Add("trigger", map.tiles[i].isTrigger);
                components.Add(collider);
            }

            #endregion
            //Add components to components array
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

    void ExportPlayer(ScriptableObject scObj)
    {
        Player player = scObj as Player;
        JSONObject playerJSON = new JSONObject();

        JSONArray resources = new JSONArray(); //List of .meta files
        resources.Add("../Assets/Images/Witch.png.meta");

        JSONArray gameObjects = new JSONArray();
        JSONObject playerNode = new JSONObject();
        playerNode.Add("name", "Player");
        playerNode.Add("enabled", player.enabled);
        playerNode.Add("destroyOnUnload", player.destroyOnUnload);

        JSONArray components = new JSONArray();

        #region create transform component
        JSONObject transform = new JSONObject();
        JSONObject position = new JSONObject();
        JSONObject scale = new JSONObject();

        position.Add("X", player.position.x); //have to convert to floats
        position.Add("Y", player.position.y);

        scale.Add("X", (float)(player.scale.x * 1.0f));
        scale.Add("Y", (float)(player.scale.y * 1.0f));
        transform.Add("class", "Transform");
        transform.Add("Position", position);
        transform.Add("Scale", scale);
        #endregion

        #region create sprite component
        JSONObject sprite = new JSONObject();
        JSONObject texture = new JSONObject();
        JSONObject dimensions = new JSONObject();

        texture.Add("textureAssetGUID", "04127430-547d-4db3-936b-666e63665eaa");

        dimensions.Add("Left", player.sprite.rect.x);
        dimensions.Add("Top", (player.sprite.texture.height - player.sprite.rect.y) - 64);
        dimensions.Add("Width", 64);
        dimensions.Add("Height", 64);

        sprite.Add("class", "Sprite");
        sprite.Add("enabled", true);
        sprite.Add("Texture", texture);
        sprite.Add("Dimensions", dimensions);
        sprite.Add("Layer", player.layer);
        #endregion

        #region create rigidbody component
        JSONObject rigidbody = new JSONObject();
        rigidbody.Add("class", "RigidBody");
        rigidbody.Add("BodyType", player.bodyType);
        #endregion

        #region create collider component 
        JSONObject circleCollider = new JSONObject();
        circleCollider.Add("class", "CircleCollider");
        circleCollider.Add("enabled", true);
        circleCollider.Add("radius", (float)(player.radius * 1.0f));
        circleCollider.Add("trigger", player.trigger);
        #endregion

        #region create player component
        JSONObject playerComponent = new JSONObject();
        playerComponent.Add("class", "Player");
        playerComponent.Add("moveSpeed", System.Convert.ToSingle(player.moveSpeed));
        #endregion

        #region create camera component
        JSONObject camera = new JSONObject();
        camera.Add("class", "Camera");
        #endregion


        components.Add(transform);
        components.Add(sprite);
        components.Add(rigidbody);
        components.Add(circleCollider);
        components.Add(playerComponent);
        components.Add(camera);

        playerNode.Add("Components", components);
        gameObjects.Add(playerNode);

        playerJSON.Add("resources", resources);
        playerJSON.Add("GameObjects", gameObjects);

        var fileNameField = assetRootElement.Q<TextField>("fileNameField");

        if (fileNameField.text == "")
        {
            Debug.Log("Player must have a name!");
        }
        else
        {
            File.WriteAllText("Assets/Resources/JSON/" + fileNameField.text + ".json", playerJSON.ToString());
            Debug.Log("Player successfully exported!");
        }
    }
}