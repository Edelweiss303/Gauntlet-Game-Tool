using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileList : List<Tile> { }
public class Map : ScriptableObject
{
    public string Name;
    public Texture2D texture;
    public TileList tiles;
}
