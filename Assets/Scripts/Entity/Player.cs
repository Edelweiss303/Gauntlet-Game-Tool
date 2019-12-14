using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : ScriptableObject
{
    public Sprite sprite;
    public int moveSpeed;
    public Vector2 position = new Vector2(100.0f, 100.0f); //Default Transform
    public Vector2 scale = new Vector2(1.0f, 1.0f); //Default scale
    public bool enabled = true;
    public bool destroyOnUnload = true;

    public int layer = 3; //Players are always drawn on layer 3

    public Weapon weapon;

    public int bodyType; //RigidBody currently only has one element 
    //public RigidBody rigidBody;

    //Circle Collider
    public int radius;
    public bool trigger = false; //Default to false

    public int fixtureDensity = 0;
    public int fixtureFriction = 0;
    public int fixtureRestitution = 0;
}
