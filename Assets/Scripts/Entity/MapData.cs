using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="MapData-00", menuName = "My Assets/Map Data")]
public class MapData : ScriptableObject
{
    [Header("Map")]
    [Tooltip("List of all maps")]
    public List<Map> maps;
}
