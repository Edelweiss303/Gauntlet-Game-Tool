using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile-00", menuName = "My Assets/Tile")]
public class Tile : ScriptableObject
{
    public Sprite sprite;
    public Rect rect; //Texture rect of the sprite 
    public Vector2 position;
    public Vector2 scale = new Vector2(1.0f, 1.0f);
    public bool enabled = true;
    public bool destroyOnUnload = true;
    public int layer;

    public bool isTrigger;
    public int radius = 0;
    public int boxHeight = 0;
    public int boxWidth = 0;

}
