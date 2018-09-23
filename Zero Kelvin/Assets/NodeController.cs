using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour {
    private bool mouseDown = false;
    public MeshRenderer meshRenderer;
    private float pressure = 0.0f;
    public float Pressure
    {
        get
        {
            return pressure;
        }
        private set
        {
            pressure = value;
        }
    }
    public float delta_pressure = 0.0f;

    private List<NodeConnection> connections = new List<NodeConnection>();

    public void AddConnection(NodeConnection connection)
    {
        connections.Add(connection);
    }

    public Vector2 Wind
    {
        get
        {
            Vector2 wind = new Vector2();
            foreach (NodeConnection connection in connections)
            {
                wind += connection.wind * connection.normal;
            }
            return wind;
        }
    }
    public Vector2 delta_wind = new Vector2();
    Color colour;

    void Start()
    {
        colour = new Color(
            meshRenderer.material.color.r, 
            meshRenderer.material.color.g, 
            meshRenderer.material.color.b, 
            meshRenderer.material.color.a);
    }

    // Update is called once per frame
    public void FixedUpdate() {
        Pressure += delta_pressure;
        delta_pressure = 0;
        foreach(NodeConnection connection in connections)
        {
            float wind_change = Vector2.Dot(delta_wind, connection.normal);
            connection.wind += wind_change;
        }
        delta_wind.x = 0;
        delta_wind.y = 0;
        if (mouseDown) {
            Pressure += 1.0f;
        }
    }
    public void Update()
    {
        colour.a = Pressure;
        
        meshRenderer.material.color = Pressure >= 0f ? colour : new Color(1f,0,0,1f);
    }

    void OnMouseDown()
    {
        mouseDown = true;
    }

    void OnMouseUp()
    {
        mouseDown = false;
    }
}
