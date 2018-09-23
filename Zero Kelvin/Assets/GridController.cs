using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour {
    const int width = 48;
    const int size = width * width;
    const int n_connections = (width-1) * width *2;
    NodeController nodePrefab;
    readonly NodeController[] nodeGrid = new NodeController[size];
    readonly NodeConnection[] connections = new NodeConnection[n_connections];

    // Use this for initialization
    void Start () {
        nodePrefab = transform.GetComponentInChildren<NodeController>();
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        nodePrefab.enabled = true;
        nodePrefab.GetComponent<BoxCollider2D>().enabled = true;
        for (int i = 0; i < size; ++i)
        {
            nodeGrid[i] = Instantiate(nodePrefab, transform);
            nodeGrid[i].transform.localPosition = new Vector3(
                i % width - (width - 1) / 2f, 
                i / width - (width - 1) / 2f, 0);
            nodeGrid[i].name = "Node (" + i % width + ", " + i / width + ")";
            nodeGrid[i].delta_pressure = 0.1f;
        }
        nodeGrid[0].delta_pressure = 1f;
        int j = 0;
        for (int y = 0; y < width; ++y)
        {
            for (int x = 0; x < width - 1; ++x)
            {
                connections[j++] = new NodeConnection(nodeGrid[x + y * width], nodeGrid[(x + 1) + y * width], new Vector2(1, 0));
                connections[j++] = new NodeConnection(nodeGrid[y + x * width], nodeGrid[y + (1 + x) * width], new Vector2(0, 1));
            }
        }
        transform.Translate(new Vector3(0, 0, width - 4));
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        Simulate_C();
    }

    void Simulate_A()
    {
        // Satisfy first corner case (only exchange pressure, not wind)
        int x0 = width - 1;
        int y0 = width - 1;
        NodeController corner_node0 = nodeGrid[x0 + y0 * width];
        NodeController left_x_node = nodeGrid[(x0 - 1) + y0 * width];
        NodeController down_y_node = nodeGrid[x0 + (y0 - 1) * width];
        //Adjust wind based on pressure difference
        left_x_node.delta_wind.x += (left_x_node.Pressure - corner_node0.Pressure) / 5f;
        down_y_node.delta_wind.y += (down_y_node.Pressure - corner_node0.Pressure) / 5f;
        // Move pressure between nodes
        left_x_node.delta_pressure -= left_x_node.Wind.x;
        down_y_node.delta_pressure -= down_y_node.Wind.y;
        corner_node0.delta_pressure += left_x_node.Wind.x + down_y_node.Wind.y;

        // Satisfy second corner case (exchange pressure, and only specific wind)
        int x1 = width - 2;
        int y1 = width - 2;
        NodeController corner_node1 = nodeGrid[x1 + y1 * width];
        NodeController right_x_node = nodeGrid[(x1 + 1) + y1 * width];
        NodeController up_y_node = nodeGrid[x1 + (y1 + 1) * width];
        //Adjust wind based on pressure difference
        corner_node1.delta_wind += new Vector2(
            corner_node1.Pressure - right_x_node.Pressure,
            corner_node1.Pressure - up_y_node.Pressure) / 5f;
        // Move pressure between nodes
        right_x_node.delta_pressure += corner_node1.Wind.x;
        up_y_node.delta_pressure += corner_node1.Wind.y;
        corner_node1.delta_pressure -= corner_node1.Wind.x + corner_node1.Wind.y;
        // Move wind between nodes
        Vector2 delta_wind = new Vector2(
            (corner_node1.Wind.x - up_y_node.Wind.x),
            (corner_node1.Wind.y - right_x_node.Wind.y)) / 4f;
        up_y_node.delta_wind.x += delta_wind.x;     // Exchange x wind with the node above
        right_x_node.delta_wind.y += delta_wind.y;  // Exchange y wind with the node to the right.
        corner_node1.delta_wind -= delta_wind;

        // Satisfy right edge case (only interact vertically on the right edge)
        for (int y = 0; y < width - 2; ++y)
        {
            int x = width - 1;
            NodeController middle_node = nodeGrid[x + y * width];
            NodeController y_node = nodeGrid[x + (y + 1) * width];
            //Adjust wind based on pressure difference
            middle_node.delta_wind.y += (middle_node.Pressure - y_node.Pressure) / 5f;
            // Move pressure between nodes
            y_node.delta_pressure += middle_node.Wind.y;
            middle_node.delta_pressure -= middle_node.Wind.y;
            // Move wind between nodes
            float delta_wind_y = (middle_node.Wind.y - y_node.Wind.y) / 4f;
            y_node.delta_wind.y += delta_wind_y;
            middle_node.delta_wind.y -= delta_wind_y;
        }
        // Satisfy top edge case (only interact horizontally on the top edge)
        for (int x = 0; x < width - 2; ++x)
        {
            int y = width - 1;
            NodeController middle_node = nodeGrid[x + y * width];
            NodeController x_node = nodeGrid[(x + 1) + y * width];
            //Adjust wind based on pressure difference
            middle_node.delta_wind.x += (middle_node.Pressure - x_node.Pressure) / 5f;
            // Move pressure between nodes
            x_node.delta_pressure += middle_node.Wind.x;
            middle_node.delta_pressure -= middle_node.Wind.x;
            // Move wind between nodes
            float delta_wind_x = (middle_node.Wind.x - x_node.Wind.x) / 4f;
            x_node.delta_wind.x += delta_wind_x;
            middle_node.delta_wind.x -= delta_wind_x;
        }
        // Satisfy second right edge case (don't transfer x-wind horizontally on the second right edge)
        for (int y = 0; y < width - 2; ++y)
        {
            int x = width - 2;
            NodeController middle_node = nodeGrid[x + y * width];   // Middle node -> x + y * width
            NodeController y_node = nodeGrid[x + (y + 1) * width];  // Node above -> x + (y+1) * width
            NodeController x_node = nodeGrid[(x + 1) + y * width];  // Right node -> (x+1) + y * width
            //Adjust wind based on pressure difference
            middle_node.delta_wind += new Vector2(
                middle_node.Pressure - x_node.Pressure,
                middle_node.Pressure - y_node.Pressure) / 5f;
            // Move pressure between nodes
            x_node.delta_pressure += middle_node.Wind.x;
            y_node.delta_pressure += middle_node.Wind.y;
            middle_node.delta_pressure -= middle_node.Wind.x + middle_node.Wind.y;
            // Move wind between nodes
            Vector2 delta_wind_x = new Vector2(0f, (middle_node.Wind.y - x_node.Wind.y) / 4f);
            Vector2 delta_wind_y = (middle_node.Wind - y_node.Wind) / 4f;
            x_node.delta_wind += delta_wind_x;
            y_node.delta_wind += delta_wind_y;
            middle_node.delta_wind -= delta_wind_x + delta_wind_y;
        }
        // Satisfy second top edge case (don't transfer y-wind vertically on the second top edge)
        for (int x = 0; x < width - 2; ++x)
        {
            int y = width - 2;
            NodeController middle_node = nodeGrid[x + y * width];   // Middle node -> x + y * width
            NodeController y_node = nodeGrid[x + (y + 1) * width];  // Node above -> x + (y+1) * width
            NodeController x_node = nodeGrid[(x + 1) + y * width];  // Right node -> (x+1) + y * width
            //Adjust wind based on pressure difference
            middle_node.delta_wind += new Vector2(
                middle_node.Pressure - x_node.Pressure,
                middle_node.Pressure - y_node.Pressure) / 5f;
            // Move pressure between nodes
            x_node.delta_pressure += middle_node.Wind.x;
            y_node.delta_pressure += middle_node.Wind.y;
            middle_node.delta_pressure -= middle_node.Wind.x + middle_node.Wind.y;
            // Move wind between nodes
            Vector2 delta_wind_x = (middle_node.Wind - x_node.Wind) / 4f;
            Vector2 delta_wind_y = new Vector2((middle_node.Wind.x - y_node.Wind.x) / 4f, 0f);
            x_node.delta_wind += delta_wind_x;
            y_node.delta_wind += delta_wind_y;
            middle_node.delta_wind -= delta_wind_x + delta_wind_y;
        }
        // Perform the rest of the grid's interactions
        for (int y = 0; y < width - 2; ++y)
        {
            for (int x = 0; x < width - 2; ++x)
            {
                NodeController middle_node = nodeGrid[x + y * width];   // Middle node -> x + y * width
                NodeController y_node = nodeGrid[x + (y + 1) * width];  // Node above -> x + (y+1) * width
                NodeController x_node = nodeGrid[(x + 1) + y * width];  // Right node -> (x+1) + y * width
                //Adjust wind based on pressure difference
                middle_node.delta_wind += new Vector2(
                    middle_node.Pressure - x_node.Pressure,
                    middle_node.Pressure - y_node.Pressure) / 5f;
                // Move pressure between nodes
                x_node.delta_pressure += middle_node.Wind.x;
                y_node.delta_pressure += middle_node.Wind.y;
                middle_node.delta_pressure -= middle_node.Wind.x + middle_node.Wind.y;
                // Move wind between nodes
                Vector2 delta_wind_x = (middle_node.Wind - x_node.Wind) / 4f;
                Vector2 delta_wind_y = (middle_node.Wind - y_node.Wind) / 4f;
                x_node.delta_wind += delta_wind_x;
                y_node.delta_wind += delta_wind_y;
                middle_node.delta_wind -= delta_wind_x + delta_wind_y;
            }
        }
    }

    void Simulate_B()
    {
        foreach(NodeConnection connection in connections)
        {
            // Sum of the existing wind vectors
            Vector2 existing_wind = (connection.n0.Wind + connection.n1.Wind);
            //Average the wind between the connected tiles
            float average_wind = Vector2.Dot(existing_wind, connection.normal) / 2f;
            if(average_wind != 0f)
            {
                // Move pressure between nodes
                connection.n0.delta_pressure -= average_wind;
                connection.n1.delta_pressure += average_wind;
                // Move wind between nodes
                if (average_wind > 0f)
                {
                    Vector2 wind_moved_from_n0 = connection.n0.Wind * (average_wind / connection.n0.Pressure);
                    connection.n0.delta_wind -= wind_moved_from_n0;
                    connection.n1.delta_wind += wind_moved_from_n0;
                }
                else
                {
                    Vector2 wind_moved_from_n1 = connection.n1.Wind * (average_wind / connection.n1.Pressure);
                    connection.n0.delta_wind += wind_moved_from_n1;
                    connection.n1.delta_wind -= wind_moved_from_n1;
                }
            }
            //Adjust wind based on pressure difference
            float delta_pressure = connection.n0.Pressure - connection.n1.Pressure;
            connection.n0.delta_wind += connection.normal * (delta_pressure / 10f);
            connection.n1.delta_wind += connection.normal * (delta_pressure / 10f);
        }
    }

    void Simulate_C()
    {
        foreach (NodeConnection connection in connections)
        {
            //Adjust wind based on pressure difference
            connection.delta_wind += (connection.n0.Pressure - connection.n1.Pressure) / 5f;
            // Move pressure between nodes
            float delta_pressure = connection.wind;
            connection.n0.delta_pressure -= delta_pressure;
            connection.n1.delta_pressure += delta_pressure;
            // Move wind between nodes
            Vector2 delta_wind = (connection.n0.Wind - connection.n1.Wind) / 4f;
            connection.n0.delta_wind -= delta_wind;
            connection.n1.delta_wind += delta_wind;
        }
        foreach (NodeConnection connection in connections)
        {
            connection.wind += connection.delta_wind;
            connection.delta_wind = 0f;
        }
    }
}
