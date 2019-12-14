using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonCollider
{
    public bool enabled = true;
    public int boxHeight;
    public int boxWidth;
    public bool trigger = false;  

    public int fixtureDensity = 0;
    public int fixtureFriction = 0;
    public int fixtureRestitution = 0;
}
