using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : ScriptableObject
{
    public Sprite sprite;
    public int moveSpeed;
    public Vector2 position;
    public Vector2 scale = new Vector2(1.0f, 1.0f);
    public bool enabled = true;
    public bool destroyOnUnload = true;
    public int layer;
    public Weapon weapon;
    public struct RigidBody {

    };
    CircleCollider circleCollider;
    PolygonCollider polygonCollider; 
}
