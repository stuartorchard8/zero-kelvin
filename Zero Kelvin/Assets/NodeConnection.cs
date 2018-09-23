using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeConnection {
    public NodeController n0, n1;
    public Vector2 normal;
    public float wind = 0f;
    public float delta_wind = 0f;

    public NodeConnection(NodeController n0, NodeController n1, Vector2 normal)
    {
        this.n0 = n0;
        this.n1 = n1;
        this.normal = normal;
        n0.AddConnection(this);
    }
}
