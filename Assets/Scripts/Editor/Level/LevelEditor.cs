using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Collections.Generic;
using SimpleJSON;


public class LevelEditor
{
    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

    VisualElement levelRootElement;

    ListView layerList;
    string[] layers = {"Environment", "Props", "Enemies", "Player"};
    int currentLayer;

    int rowCount = 40;
    int columnCount = 40;
    int cellSize = 16;

    //list of 2D arrays for each layer where
    // 0 = environment
    // 1 = props
    // 2 = enemies
    // 3 = player
    List<List<List<Tile>>> mapLayers;
    bool paintFlag = false;

    //Arrays for the paint palette
    int paletteRowCount = 10;
    int paletteColumnCount = 10;
    int paletteCellSize = 64;
    List<List<bool>> palette; //List of bools to check which stamp was selected [THIS IS SLOW. DO THIS DIFFERENTLY]
    List<List<Tile>> tiles; //List of all tile prefabs 

    Texture2D texture;
    int selectionRow;
    int selectionColumn;
    Color selectionColor = new Color(0.5f, 0.9f, 0.5f, 0.25f);
    Tile selection;
    bool eraser = false;
    bool circleCollider = false;
    bool squareCollider = false;
    Texture2D colliderTexture;

    Object[] loadedAssets;
    int tileColumn;
    int tileRow;

    void drawGrid()
    {
        DrawLevel();

        for (int r = 1; r < rowCount + 1; r++)
        {
            EditorGUI.DrawRect(new Rect(new Vector2(0, r * cellSize), new Vector2(columnCount * cellSize, 1)), selectionColor);
        }
        for (int c = 1; c < columnCount + 1; c++)
        {
            EditorGUI.DrawRect(new Rect(new Vector2(c * cellSize, 0), new Vector2(1, rowCount * cellSize)), selectionColor);
        }
    }
    void drawSpriteSheet()
    {
        DrawPalette();
        for (int r = 0; r < palette.Count; r++)
        {
            for (int c = 0; c < palette[r].Count; c++)
            {
                if (palette[r][c])
                {
                    EditorGUI.DrawRect(new Rect(c * paletteCellSize, r * paletteCellSize, paletteCellSize, paletteCellSize), selectionColor);
                }
            }
        }
    }
    public void OnOpen(VisualElement root)
    {
        //load sprite sheet for colliders
        colliderTexture = Resources.Load<Texture2D>("colliders");

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Level/LevelEditor.uss");
        root.styleSheets.Add(styleSheet);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Level/LevelEditor.uxml");
        levelRootElement = visualTree.CloneTree();
        root.Add(levelRootElement);

        var mapObjectField = levelRootElement.Q<ObjectField>("mapObjectField");
        mapObjectField.objectType = typeof(Map);
        mapObjectField.RegisterCallback<ChangeEvent<Object>>(LoadMap);


        var textureField = levelRootElement.Q<ObjectField>("textureField");
        textureField.objectType = typeof(Texture2D);

        var circleColliderButton = levelRootElement.Q<Button>("circleColliderButton");
        var squareColliderButton = levelRootElement.Q<Button>("squareColliderButton");
        var eraserButton = levelRootElement.Q<Button>("eraserButton");
        var saveButton = levelRootElement.Q<Button>("saveButton");

        layerList = levelRootElement.Q<ListView>("layerList");
        layerList.itemsSource = layers;
        layerList.makeItem = () => new Label();
        layerList.bindItem = (e, i) => (e as Label).text = layers[i].ToString();
        layerList.itemHeight = 20;
        layerList.onSelectionChanged += objects => SelectLayer();
        currentLayer = 0;

        palette = new List<List<bool>>(paletteRowCount);
        tiles = new List<List<Tile>>(paletteRowCount);
        for (int r = 0; r < paletteRowCount; r++)
        {
            palette.Add(new List<bool>(paletteColumnCount));
            tiles.Add(new List<Tile>(paletteColumnCount));
            for (int c = 0; c < paletteColumnCount; c++)
            {
                palette[r].Add(false);
                tiles[r].Add(Tile.CreateInstance<Tile>());
            }
        }

        mapLayers = new List<List<List<Tile>>>(layers.Length);
        for (int i = 0; i < layers.Length; i++)
        {
            mapLayers.Add(new List<List<Tile>>(rowCount));
            for (int r = 0; r < rowCount; r++)
            {
                mapLayers[i].Add(new List<Tile>(columnCount));
                for (int c = 0; c < columnCount; c++)
                {
                    mapLayers[i][r].Add(Tile.CreateInstance<Tile>());
                    EditorUtility.SetDirty(mapLayers[i][r][c]);
                }
            }
        }
        LoadPalette();

        var mapGridContainer = levelRootElement.Q<IMGUIContainer>("mapGridContainer");
        var paletteGridContainer = levelRootElement.Q<IMGUIContainer>("palletteGridContainer");

        mapGridContainer.RegisterCallback<MouseMoveEvent>(MapMouseGridCallBack);
        mapGridContainer.RegisterCallback<MouseDownEvent>(SetPaintFlagTrue);
        mapGridContainer.RegisterCallback<MouseUpEvent>(SetPaintFlagFalse);
        mapGridContainer.RegisterCallback<MouseLeaveEvent>(SetPaintFlagFalse);
        mapGridContainer.onGUIHandler = drawGrid;

        paletteGridContainer.RegisterCallback<MouseDownEvent>(PaletteMouseGridCallBack);
        paletteGridContainer.onGUIHandler = drawSpriteSheet;

        textureField.RegisterCallback<ChangeEvent<Object>>(TextureFieldCallBack);

        circleColliderButton.RegisterCallback<MouseUpEvent>(circleColliderCallBack);
        squareColliderButton.RegisterCallback<MouseUpEvent>(squareColliderCallBack);
        eraserButton.RegisterCallback<MouseUpEvent>(eraserCallBack);
        saveButton.RegisterCallback<MouseUpEvent>(SaveButtonCallBack);
        
    }
    void SetPaintFlagTrue(MouseDownEvent evt)
    {
        paintFlag = true;
        if (paintFlag)
        {
            int rCell = (int)evt.localMousePosition.y / cellSize;
            int cCell = (int)evt.localMousePosition.x / cellSize;
            if (eraser) //erase if the eraser is selected
            {
                Erase(rCell, cCell);
            }
            else if (circleCollider)
            {
                CreateCircleCollider(rCell, cCell);
            }
            else if (squareCollider)
            {
                CreateSquareCollider(rCell, cCell);
            }
            else
            {
                Paint(rCell, cCell);
            }
            MainEditor.getWindow().Repaint();
        }
    }
    void SetPaintFlagFalse(MouseUpEvent evt)
    {
        paintFlag = false;
    }
    void SetPaintFlagFalse(MouseLeaveEvent evt)
    {
        paintFlag = false;
    }
    void MapMouseGridCallBack(MouseMoveEvent evt)
    {
        if (paintFlag)
        {
            int rCell = (int)evt.localMousePosition.y / cellSize;
            int cCell = (int)evt.localMousePosition.x / cellSize;

            if (eraser) //erase if the eraser is selected
            {
                Erase(rCell, cCell);
            }
            else if (circleCollider)
            {
                CreateCircleCollider(rCell, cCell);
            }
            else if (squareCollider)
            {
                CreateSquareCollider(rCell, cCell);
            }
            else
            {
                Paint(rCell, cCell);
            }
            MainEditor.getWindow().Repaint();
        }

    }
    void PaletteMouseGridCallBack(MouseDownEvent evt)
    {  
        //unselect the other buttons
        eraser = false;
        circleCollider = false;
        squareCollider = false;
        //Clear the other button backgrounds
        var eraserButton = levelRootElement.Q<Button>("eraserButton");
        eraserButton.style.backgroundColor = Color.clear;
        var squareColliderButton = levelRootElement.Q<Button>("squareColliderButton");
        squareColliderButton.style.backgroundColor = Color.clear;
        var circleColliderButton = levelRootElement.Q<Button>("circleColliderButton");
        circleColliderButton.style.backgroundColor = Color.clear;

        int rCell = (int)evt.localMousePosition.y / paletteCellSize;
        int cCell = (int)evt.localMousePosition.x / paletteCellSize;

        if (rCell < paletteRowCount && cCell < paletteColumnCount)
        {
            palette[selectionRow][selectionColumn] = false;
            palette[rCell][cCell] = !palette[rCell][cCell];
            selectionRow = rCell;
            selectionColumn = cCell;
            MainEditor.getWindow().Repaint();
        }
    }
    void SelectLayer()
    {
        currentLayer = layerList.selectedIndex + 1;
    }
    void LoadPalette()
    {
        loadedAssets = Resources.LoadAll("Tiles", typeof(Tile));
        foreach (Object obj in loadedAssets)
        {
            Tile tile = obj as Tile;
            int xPos = (int)(tile.rect.y * (1.0f / 64.0f));
            int yPos = (int)(tile.rect.x * (1.0f / 64.0f));
            tiles[xPos][yPos] = tile;
        }
    }
    void LoadMap(ChangeEvent<Object> evt)
    {
        Map map = evt.newValue as Map;

        foreach (Tile tile in map.tiles)
        {
            int rCell = (int)(tile.position.y / 64);
            int cCell = (int)(tile.position.x / 64);

            mapLayers[tile.layer][rCell][cCell] = tile;
            EditorUtility.SetDirty(mapLayers[tile.layer][rCell][cCell]);
        }
    }
    void DrawPalette()
    {
        for (int r = 0; r < palette.Count; r++)
        {
            for (int c = 0; c < palette[r].Count; c++)
            {
                if(tiles[r][c].sprite != null)
                {
                    Rect textureRect = tiles[r][c].rect;

                    textureRect.x *= 0.0015625f;
                    textureRect.y *= 0.0015625f;
                    textureRect.width *= 0.0015625f;
                    textureRect.height *= 0.0015625f;

                    GUI.DrawTextureWithTexCoords(tiles[r][c].rect, tiles[r][c].sprite.texture, textureRect);
                }

                if (palette[r][c])
                {
                    selection = tiles[r][c];
                    EditorGUI.DrawRect(new Rect(c * paletteCellSize, r * paletteCellSize, paletteCellSize, paletteCellSize), selectionColor);
                }
            }
        }
    }
    void DrawLevel()
    {
        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < columnCount; c++)
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    DrawTile(mapLayers[i], r, c);
                }
            }
        }
    }
    void DrawTile(List<List<Tile>> cells, int r, int c)
    {
        Rect destRect = new Rect(c * cellSize, r * cellSize, cellSize, cellSize);

        if (cells[r][c].sprite != null)
        {
            Rect textureRect = cells[r][c].rect;

            //dividing rect by 1/640;
            textureRect.x *= 0.0015625f;
            textureRect.y *= 0.0015625f;
            textureRect.width *= 0.0015625f;
            textureRect.height *= 0.0015625f;

            GUI.DrawTextureWithTexCoords(destRect, cells[r][c].sprite.texture, textureRect);
        }
        //draw the colliders if there is one
        if (cells[r][c].radius != 0)
        {
            if (cells[r][c].isTrigger)
            {
                GUI.DrawTextureWithTexCoords(destRect, colliderTexture, new Rect(0.5f, 0.0f, 0.5f, 0.5f));
            }
            else
            {
                GUI.DrawTextureWithTexCoords(destRect, colliderTexture, new Rect(0.0f, 0.0f, 0.5f, 0.5f));
            }
        }
        else if (cells[r][c].boxHeight != 0)
        {
            if (cells[r][c].isTrigger)
            {
                GUI.DrawTextureWithTexCoords(destRect, colliderTexture, new Rect(0.5f, 0.5f, 0.5f, 0.5f));
            }
            else
            {
                GUI.DrawTextureWithTexCoords(destRect, colliderTexture, new Rect(0.0f, 0.5f, 0.5f, 0.5f));
            }
        }

    }
    void TextureFieldCallBack(ChangeEvent<Object> obj)
    {
        texture = obj.newValue as Texture2D;
        Sprite[] sprites = Resources.LoadAll<Sprite>(texture.name);

        foreach(Sprite s in sprites)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = s;
            tile.rect = s.textureRect;
            EditorUtility.SetDirty(tile);
            AssetDatabase.CreateAsset(tile, "Assets/Resources/Tiles/" + s.name + ".asset");
            AssetDatabase.SaveAssets();
        }
    }
    void circleColliderCallBack(MouseUpEvent evt)
    {
        //clear other buttons/selections
        palette[selectionRow][selectionColumn] = false;
        squareCollider = false;
        var eraserButton = levelRootElement.Q<Button>("eraserButton");
        eraserButton.style.backgroundColor = Color.clear;
        var squareColliderButton = levelRootElement.Q<Button>("squareColliderButton");
        squareColliderButton.style.backgroundColor = Color.clear;

        //Highlight the button so we know it's selected
        var circleColliderButton = levelRootElement.Q<Button>("circleColliderButton");
        circleColliderButton.style.backgroundColor = selectionColor;

        circleCollider = true;
    }
    void squareColliderCallBack(MouseUpEvent evt)
    {
        //clear other buttons/selections
        palette[selectionRow][selectionColumn] = false;
        circleCollider = false;
        var eraserButton = levelRootElement.Q<Button>("eraserButton");
        eraserButton.style.backgroundColor = Color.clear;
        var circleColliderButton = levelRootElement.Q<Button>("circleColliderButton");
        circleColliderButton.style.backgroundColor = Color.clear;

        //Highlight the button so we know it's selected
        var squareColliderButton = levelRootElement.Q<Button>("squareColliderButton");
        squareColliderButton.style.backgroundColor = selectionColor;

        squareCollider = true;
    }
    void eraserCallBack(MouseUpEvent evt)
    {
        //clear other buttons/selections
        palette[selectionRow][selectionColumn] = false;
        var squareColliderButton = levelRootElement.Q<Button>("squareColliderButton");
        squareColliderButton.style.backgroundColor = Color.clear;
        var circleColliderButton = levelRootElement.Q<Button>("circleColliderButton");
        circleColliderButton.style.backgroundColor = Color.clear;

        var eraserButton = levelRootElement.Q<Button>("eraserButton");
        eraserButton.style.backgroundColor = selectionColor;
        eraser = true;
    }
    void SaveButtonCallBack(MouseUpEvent evt)
    {
        var mapNameField = levelRootElement.Q<TextField>("mapNameField");

        if (mapNameField.text == "")
        {
            Debug.Log("Map must have a name!");
        }
        else
        {
            
            Map map = ScriptableObject.CreateInstance<Map>();
            EditorUtility.SetDirty(map);

            map.name = mapNameField.text;
            EditorUtility.SetDirty(map);

            map.tiles = new List<Tile>();
            EditorUtility.SetDirty(map);

            for (int i = 0; i < layers.Length; i++)
            {
                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < columnCount; c++)
                    {
                        if(mapLayers[i][r][c].sprite != null)
                        {
                            mapLayers[i][r][c].layer = i;
                            EditorUtility.SetDirty(mapLayers[i][r][c]);

                            map.tiles.Add(mapLayers[i][r][c]);
                            EditorUtility.SetDirty(map);
                        }
                        if(mapLayers[i][r][c].radius != 0 || mapLayers[i][r][c].boxHeight != 0)
                        {
                            mapLayers[i][r][c].layer = i;
                            EditorUtility.SetDirty(mapLayers[i][r][c]);

                            map.tiles.Add(mapLayers[i][r][c]);
                            EditorUtility.SetDirty(map);
                        }
                    }
                }
            }

            
            AssetDatabase.CreateAsset(map, "Assets/Resources/Maps/" + map.name + ".asset");
            EditorUtility.SetDirty(map);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Level Object exported successfully!");
        }
    }
    void Paint(int rCell, int cCell)
    {
        if (rCell < rowCount && cCell < columnCount && selection != null)
        {
            Vector2 position = new Vector2(cCell * 64, rCell * 64);
            //Draw on selected layer
            switch (currentLayer)
            {
                case 1:
                    mapLayers[0][rCell][cCell].sprite = selection.sprite;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);

                    mapLayers[0][rCell][cCell].rect = selection.sprite.rect;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);

                    mapLayers[0][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);
                    break;

                case 2:
                    mapLayers[1][rCell][cCell].sprite = selection.sprite;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);

                    mapLayers[1][rCell][cCell].rect = selection.sprite.rect;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);

                    mapLayers[1][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);
                    break;

                case 3:
                    mapLayers[2][rCell][cCell].sprite = selection.sprite;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);

                    mapLayers[2][rCell][cCell].rect = selection.sprite.rect;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);

                    mapLayers[2][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);
                    break;

                case 4:
                    mapLayers[3][rCell][cCell].sprite = selection.sprite;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);

                    mapLayers[3][rCell][cCell].rect = selection.sprite.rect;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);

                    mapLayers[3][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);
                    break;
            }
        }
    }
    void Erase(int rCell, int cCell)
    {
        if (rCell < rowCount && cCell < columnCount && selection != null)
        {
            //replace painted tile with blank tile to erase
            Tile tile = Tile.CreateInstance<Tile>();
            switch (currentLayer)
            {
                case 1:
                    mapLayers[0][rCell][cCell] = tile;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);
                    break;                       
                                                 
                case 2:                          
                    mapLayers[1][rCell][cCell] = tile;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);
                    break;                       
                                                 
                case 3:                          
                    mapLayers[2][rCell][cCell] = tile;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);
                    break;                       
                                                 
                case 4:                          
                    mapLayers[3][rCell][cCell] = tile;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);
                    break;
            }
        }
    }
    void CreateSquareCollider(int rCell, int cCell)
    {
        if (rCell < rowCount && cCell < columnCount && selection != null)
        {
            Vector2 position = new Vector2(cCell * 64, rCell * 64);
            var colliderTriggerToggle = levelRootElement.Q<Toggle>("colliderTriggerToggle");
            switch (currentLayer)
            {
                case 1:
                    mapLayers[0][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);

                    mapLayers[0][rCell][cCell].radius = 0;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);

                    mapLayers[0][rCell][cCell].boxHeight = 64;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);

                    mapLayers[0][rCell][cCell].boxWidth = 64;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);

                    mapLayers[0][rCell][cCell].isTrigger = colliderTriggerToggle.value;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);
                    break;

                case 2:
                    mapLayers[1][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);

                    mapLayers[1][rCell][cCell].radius = 0;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);

                    mapLayers[1][rCell][cCell].boxHeight = 64;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);

                    mapLayers[1][rCell][cCell].boxWidth = 64;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);

                    mapLayers[1][rCell][cCell].isTrigger = colliderTriggerToggle.value;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);
                    break;

                case 3:
                    mapLayers[2][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);

                    mapLayers[2][rCell][cCell].radius = 0;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);

                    mapLayers[2][rCell][cCell].boxHeight = 64;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);

                    mapLayers[2][rCell][cCell].boxWidth = 64;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);

                    mapLayers[2][rCell][cCell].isTrigger = colliderTriggerToggle.value;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);
                    break;

                case 4:
                    mapLayers[3][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);

                    mapLayers[3][rCell][cCell].radius = 0;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);

                    mapLayers[3][rCell][cCell].boxHeight = 64;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);

                    mapLayers[3][rCell][cCell].boxWidth = 64;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);

                    mapLayers[3][rCell][cCell].isTrigger = colliderTriggerToggle.value;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);
                    break;
            }
        }
    }
    void CreateCircleCollider(int rCell, int cCell)
    {
        if (rCell < rowCount && cCell < columnCount && selection != null)
        {
            Vector2 position = new Vector2(cCell * 64, rCell * 64);
            var colliderTriggerToggle = levelRootElement.Q<Toggle>("colliderTriggerToggle");
            switch (currentLayer)
            {
                case 1:
                    mapLayers[0][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);

                    mapLayers[0][rCell][cCell].boxHeight = 0;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);

                    mapLayers[0][rCell][cCell].boxWidth = 0;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);

                    mapLayers[0][rCell][cCell].radius = 32;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);

                    mapLayers[0][rCell][cCell].isTrigger = colliderTriggerToggle.value;
                    EditorUtility.SetDirty(mapLayers[0][rCell][cCell]);
                    break;

                case 2:
                    mapLayers[1][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);

                    mapLayers[1][rCell][cCell].boxHeight = 0;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);

                    mapLayers[1][rCell][cCell].boxWidth = 0;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);

                    mapLayers[1][rCell][cCell].radius = 32;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);

                    mapLayers[1][rCell][cCell].isTrigger = colliderTriggerToggle.value;
                    EditorUtility.SetDirty(mapLayers[1][rCell][cCell]);
                    break;

                case 3:
                    mapLayers[2][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);

                    mapLayers[2][rCell][cCell].boxHeight = 0;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);

                    mapLayers[2][rCell][cCell].boxWidth = 0;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);

                    mapLayers[2][rCell][cCell].radius = 32;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);

                    mapLayers[2][rCell][cCell].isTrigger = colliderTriggerToggle.value;
                    EditorUtility.SetDirty(mapLayers[2][rCell][cCell]);
                    break;

                case 4:
                    mapLayers[3][rCell][cCell].position = position;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);

                    mapLayers[3][rCell][cCell].boxHeight = 0;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);

                    mapLayers[3][rCell][cCell].boxWidth = 0;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);

                    mapLayers[3][rCell][cCell].radius = 32;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);

                    mapLayers[3][rCell][cCell].isTrigger = colliderTriggerToggle.value;
                    EditorUtility.SetDirty(mapLayers[3][rCell][cCell]);
                    break;
            }
        }
    }
}

