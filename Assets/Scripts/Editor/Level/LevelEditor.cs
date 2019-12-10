using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Collections.Generic;


public class LevelEditor
{
    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

    VisualElement levelRootElement;

    ListView layerList;
    string[] layers = {"Environment", "Props", "Enemies", "Player"};
    int currentLayer;

    int rowCount = 20;
    int columnCount = 20;
    int cellSize = 32;

    //2D arrays for each layer
    List<List<Tile>> cells1;
    List<List<Tile>> cells2;
    List<List<Tile>> cells3;
    List<List<Tile>> cells4;

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

    Object[] loadedAssets;
    int tileColumn;
    int tileRow;

    void drawGrid()
    {
        DrawLevel();

        for (int r = 1; r < rowCount + 1; r++)
        {
            EditorGUI.DrawRect(new Rect(new Vector2(0, r * cellSize), new Vector2(columnCount * cellSize, 1)), Color.green);
        }
        for (int c = 1; c < columnCount + 1; c++)
        {
            EditorGUI.DrawRect(new Rect(new Vector2(c * cellSize, 0), new Vector2(1, rowCount * cellSize)), Color.green);
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
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Level/LevelEditor.uss");
        root.styleSheets.Add(styleSheet);

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Level/LevelEditor.uxml");
        levelRootElement = visualTree.CloneTree();
        root.Add(levelRootElement);

        var mapObjectField = levelRootElement.Q<ObjectField>("mapObjectField");
        mapObjectField.objectType = typeof(Map);

        var textureField = levelRootElement.Q<ObjectField>("textureField");
        textureField.objectType = typeof(Texture2D);

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

        cells1 = new List<List<Tile>>(rowCount); //Need to get this from the map;
        cells2 = new List<List<Tile>>(rowCount); 
        cells3 = new List<List<Tile>>(rowCount); 
        cells4 = new List<List<Tile>>(rowCount); 
        for (int r = 0; r < rowCount; r++)
        {
            cells1.Add(new List<Tile>(columnCount));
            cells2.Add(new List<Tile>(columnCount));
            cells3.Add(new List<Tile>(columnCount));
            cells4.Add(new List<Tile>(columnCount));
            for (int c = 0; c < columnCount; c++)
            {
                cells1[r].Add(Tile.CreateInstance<Tile>());
                cells2[r].Add(Tile.CreateInstance<Tile>());
                cells3[r].Add(Tile.CreateInstance<Tile>());
                cells4[r].Add(Tile.CreateInstance<Tile>());
            }
        }

        LoadPalette();

        var mapGridContainer = levelRootElement.Q<IMGUIContainer>("mapGridContainer");
        var paletteGridContainer = levelRootElement.Q<IMGUIContainer>("palletteGridContainer");

        mapGridContainer.RegisterCallback<MouseDownEvent>(MapMouseGridCallBack);
        mapGridContainer.onGUIHandler = drawGrid;

        paletteGridContainer.RegisterCallback<MouseDownEvent>(PaletteMouseGridCallBack);
        paletteGridContainer.onGUIHandler = drawSpriteSheet;

        textureField.RegisterCallback<ChangeEvent<Object>>(TextureFieldCallBack);

        saveButton.RegisterCallback<MouseUpEvent>(SaveButtonCallBack);
    }

    void MapMouseGridCallBack(MouseDownEvent evt)
    {

        int rCell = (int)evt.localMousePosition.y / cellSize;
        int cCell = (int)evt.localMousePosition.x / cellSize;

        if (rCell < rowCount && cCell < columnCount && selection != null)
        {
            //Draw on selected layer
            switch (currentLayer)
            {
                case 1:
                    cells1[rCell][cCell] = selection;
                    cells1[rCell][cCell].transform = new Vector2(cCell * selection.sprite.textureRect.width, rCell * selection.sprite.textureRect.height);
                    break;

                case 2:
                    cells2[rCell][cCell] = selection;
                    cells2[rCell][cCell].transform = new Vector2(cCell * selection.sprite.textureRect.width, rCell * selection.sprite.textureRect.height);
                    break;

                case 3:
                    cells3[rCell][cCell] = selection;
                    cells3[rCell][cCell].transform = new Vector2(cCell * selection.sprite.textureRect.width, rCell * selection.sprite.textureRect.height);
                    break;

                case 4:
                    cells4[rCell][cCell] = selection;
                    cells4[rCell][cCell].transform = new Vector2(cCell * selection.sprite.textureRect.width, rCell * selection.sprite.textureRect.height);
                    break;
            }
        }

        MainEditor.getWindow().Repaint();
    }

    void PaletteMouseGridCallBack(MouseDownEvent evt)
    {
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
                DrawTile(cells1, r, c);
                DrawTile(cells2, r, c);
                DrawTile(cells3, r, c);
                DrawTile(cells4, r, c);
            }
        }
    }

    void DrawTile(List<List<Tile>> cells, int r, int c)
    {
        if (cells[r][c].sprite != null)
        {
            Rect textureRect = cells[r][c].rect;
            Rect destRect = new Rect(c * cellSize, r * cellSize, cellSize, cellSize);

            textureRect.x *= 0.0015625f;
            textureRect.y *= 0.0015625f;
            textureRect.width *= 0.0015625f;
            textureRect.height *= 0.0015625f;

            GUI.DrawTextureWithTexCoords(destRect, cells[r][c].sprite.texture, textureRect);
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
            AssetDatabase.CreateAsset(tile, "Assets/Resources/Tiles/" + s.name + ".asset");
            AssetDatabase.SaveAssets();
        }
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
            map.name = mapNameField.text;
            map.texture = texture;
            map.environment = cells1;
            map.props = cells2;
            map.enemies = cells3;
            map.player = cells4;

            AssetDatabase.CreateAsset(map, "Assets/Resources/Maps/" + map.name + ".asset");
            AssetDatabase.SaveAssets();
        }
    }
}

