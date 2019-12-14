using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Map : ScriptableObject
{
    public string Name;

    [SerializeField]
    public List<Tile> tiles;
}
