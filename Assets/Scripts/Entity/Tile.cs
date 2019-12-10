using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile-00", menuName = "My Assets/Tile")]
public class Tile : ScriptableObject
{
    public string json;

    public Sprite sprite;
    public Rect rect; //Texture rect of the sprite 
    public Vector2 transform;
    public bool enabled = true;

    //CircleCollider circleCollider;
    //PolygonCollider polygonCollider; 
}
