using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle
{
    public Vector3 position;
    public float density;
    public float pressure;
    public Vector3 velocity;
    public List<int> neighboors;
    public float timeout;

    public Particle(Vector3 position)
    {
        this.position = position;
        neighboors = new List<int>();
    }
}
