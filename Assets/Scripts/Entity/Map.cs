using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Map-00", menuName = "My Assets/Map")]
[Serializable]
public class Map : ScriptableObject
{
    public string Name;
    public Texture2D texture;
    public List<List<Tile>> environment;
    public List<List<Tile>> props;
    public List<List<Tile>> enemies;
    public List<List<Tile>> player;
}
