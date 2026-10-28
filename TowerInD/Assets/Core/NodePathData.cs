using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodePathNode
{
    public int id;
    public Vector3Int position;
}

[Serializable]
public class NodePathConnection
{
    public int id;
    public int from;
    public int to;
    public bool horizontalFirst = true;
    public List<Vector3Int> cells = new List<Vector3Int>();
}

[Serializable]
public class NodePathGround
{
    public Vector3Int position;
    public int type;
    public bool placeable;
}

[Serializable]
public class NodePathGraph
{
    public List<NodePathNode> nodes = new List<NodePathNode>();
    public List<NodePathConnection> connections = new List<NodePathConnection>();
    public int nextId;

    // copy before an edit so an invalid drag can be cancelled
    public NodePathGraph Copy()
    {
        NodePathGraph copy = new NodePathGraph();
        copy.nextId = nextId;

        if (nodes == null)
            copy.nodes = null;
        else
        {
            foreach (NodePathNode node in nodes)
            {
                if (node == null)
                    copy.nodes.Add(null);
                else
                    copy.nodes.Add(new NodePathNode { id = node.id, position = node.position });
            }
        }

        if (connections == null)
            copy.connections = null;
        else
        {
            foreach (NodePathConnection connection in connections)
            {
                if (connection == null)
                {
                    copy.connections.Add(null);
                    continue;
                }

                NodePathConnection newConnection = new NodePathConnection();
                newConnection.id = connection.id;
                newConnection.from = connection.from;
                newConnection.to = connection.to;
                newConnection.horizontalFirst = connection.horizontalFirst;
                if (connection.cells != null)
                    newConnection.cells = new List<Vector3Int>(connection.cells);
                copy.connections.Add(newConnection);
            }
        }

        return copy;
    }

    public NodePathNode FindNode(int id)
    {
        if (nodes != null)
        {
            foreach (NodePathNode node in nodes)
            {
                if (node != null && node.id == id)
                    return node;
            }
        }
        return null;
    }

    public NodePathNode NodeAt(Vector3Int position)
    {
        if (nodes != null)
        {
            foreach (NodePathNode node in nodes)
            {
                if (node != null && node.position == position)
                    return node;
            }
        }
        return null;
    }

    public NodePathConnection FindConnection(int id)
    {
        if (connections != null)
        {
            foreach (NodePathConnection connection in connections)
            {
                if (connection != null && connection.id == id)
                    return connection;
            }
        }
        return null;
    }

    public NodePathNode AddNode(Vector3Int position)
    {
        NodePathNode node = NodeAt(position);
        if (node != null)
            return node;

        node = new NodePathNode();
        node.id = GetNextId();
        node.position = position;
        nodes.Add(node);
        return node;
    }

    public NodePathConnection AddConnection(int from, int to, bool horizontalFirst)
    {
        NodePathConnection connection = new NodePathConnection();
        connection.id = GetNextId();
        connection.from = from;
        connection.to = to;
        connection.horizontalFirst = horizontalFirst;
        connections.Add(connection);
        RebuildConnection(connection);
        return connection;
    }

    private int GetNextId()
    {
        // use a wider temporary so corrupted saves cannot wrap IDs negative
        if (nextId < 0)
            throw new InvalidOperationException("The next path ID cannot be negative.");
        long availableId = nextId;

        // also handles a saved nextId that predates a newly added node
        foreach (NodePathNode node in nodes)
        {
            if (node != null && node.id >= availableId)
                availableId = (long)node.id + 1;
        }
        foreach (NodePathConnection connection in connections)
        {
            if (connection != null && connection.id >= availableId)
                availableId = (long)connection.id + 1;
        }
        if (availableId >= int.MaxValue - 1)
            throw new InvalidOperationException("The path has no available IDs left.");

        nextId = (int)availableId + 1;
        return (int)availableId;
    }

    // the node positions are the source; cached cells are always regenerated
    public void RebuildCells()
    {
        if (connections == null)
            return;
        foreach (NodePathConnection connection in connections)
        {
            if (connection != null)
                RebuildConnection(connection);
        }
    }

    public Vector3Int GetBend(NodePathConnection connection)
    {
        NodePathNode from = FindNode(connection.from);
        NodePathNode to = FindNode(connection.to);
        if (from == null || to == null)
            return Vector3Int.zero;

        if (connection.horizontalFirst)
            return new Vector3Int(to.position.x, from.position.y, 0);
        return new Vector3Int(from.position.x, to.position.y, 0);
    }

    private void RebuildConnection(NodePathConnection connection)
    {
        connection.cells = new List<Vector3Int>();
        NodePathNode from = FindNode(connection.from);
        NodePathNode to = FindNode(connection.to);
        if (from == null || to == null)
            return;

        connection.cells.Add(from.position);
        Vector3Int bend = GetBend(connection);
        AddStraightCells(connection.cells, from.position, bend);
        AddStraightCells(connection.cells, bend, to.position);
    }

    private void AddStraightCells(List<Vector3Int> cells, Vector3Int from, Vector3Int to)
    {
        Vector3Int current = from;
        int stepX = Math.Sign(to.x - from.x);
        int stepY = Math.Sign(to.y - from.y);

        while (current.x != to.x || current.y != to.y)
        {
            current = new Vector3Int(current.x + stepX, current.y + stepY, 0);
            cells.Add(current);
        }
    }

    // promote a cell (including an automatic bend) into an editable junction
    public NodePathNode SplitConnection(int connectionId, Vector3Int position)
    {
        NodePathConnection connection = FindConnection(connectionId);
        if (connection == null)
            return null;

        RebuildConnection(connection);
        int index = connection.cells.IndexOf(position);
        if (index < 0)
            return null;
        if (index == 0)
            return FindNode(connection.from);
        if (index == connection.cells.Count - 1)
            return FindNode(connection.to);

        // the first step of each half preserves its original bend direction
        bool firstHorizontal = connection.cells[0].y == connection.cells[1].y;
        bool secondHorizontal = connection.cells[index].y == connection.cells[index + 1].y;
        NodePathNode node = AddNode(position);
        connections.Remove(connection);
        AddConnection(connection.from, node.id, firstHorizontal);
        AddConnection(node.id, connection.to, secondHorizontal);
        return node;
    }

    // drafts may be disconnected; finishing requires one entry and valid exits
    public bool Validate(int width, int height, bool complete, out string message)
    {
        message = "";
        if (width <= 0 || height <= 0)
        {
            message = "The map has not been created yet.";
            return false;
        }

        Dictionary<int, List<NodePathConnection>> outgoing;
        Dictionary<int, int> incoming;
        if (!GetLinks(out outgoing, out incoming, out message))
            return false;

        Dictionary<Vector3Int, int> nodePositions = new Dictionary<Vector3Int, int>();
        foreach (NodePathNode node in nodes)
        {
            Vector3Int position = node.position;
            if (position.z != 0 || position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
            {
                message = "Keep every node inside the map.";
                return false;
            }
            if (nodePositions.ContainsKey(position))
            {
                message = "Two separate nodes cannot occupy the same tile. Draw a connection to join them.";
                return false;
            }
            nodePositions.Add(position, node.id);
        }

        RebuildCells();
        Dictionary<Vector3Int, int> usedCells = new Dictionary<Vector3Int, int>();
        foreach (NodePathConnection connection in connections)
        {
            for (int i = 0; i < connection.cells.Count; i++)
            {
                Vector3Int cell = connection.cells[i];
                int endpoint = -1;
                if (i == 0)
                    endpoint = connection.from;
                else if (i == connection.cells.Count - 1)
                    endpoint = connection.to;

                int nodeId;
                if (nodePositions.TryGetValue(cell, out nodeId) && endpoint != nodeId)
                {
                    message = "A path crosses a node. Join that node with separate connections.";
                    return false;
                }

                int previousEndpoint;
                if (usedCells.TryGetValue(cell, out previousEndpoint))
                {
                    if (endpoint < 0 || previousEndpoint != endpoint)
                    {
                        message = "Paths can only meet at a shared node. Move the bend or join the paths there.";
                        return false;
                    }
                }
                else
                    usedCells.Add(cell, endpoint);
            }
        }

        if (!CheckForCycles(outgoing, incoming))
        {
            message = "That connection would make a loop. Paths must lead toward an exit.";
            return false;
        }

        if (!complete)
            return true;
        if (nodes.Count < 2 || connections.Count == 0)
        {
            message = "Draw a path before finishing.";
            return false;
        }

        int startCount = 0;
        foreach (NodePathNode node in nodes)
        {
            if (incoming[node.id] == 0)
            {
                startCount++;
                if (node.position.x != 0)
                {
                    message = "The starting node must be on the left edge.";
                    return false;
                }
            }
            if (outgoing[node.id].Count == 0 && node.position.x != width - 1)
            {
                message = "Every unfinished branch must reach the right edge or join another path.";
                return false;
            }
        }

        // In a finite DAG, exactly one source means all nodes descend from it.
        if (startCount != 1)
        {
            message = "Connect all paths to one starting node on the left edge.";
            return false;
        }
        return true;
    }

    private bool GetLinks(out Dictionary<int, List<NodePathConnection>> outgoing,
        out Dictionary<int, int> incoming, out string message)
    {
        outgoing = new Dictionary<int, List<NodePathConnection>>();
        incoming = new Dictionary<int, int>();
        message = "";

        if (nodes == null || connections == null)
        {
            message = "The saved path data is missing its node or connection list.";
            return false;
        }
        if (nextId < 0 || nextId == int.MaxValue)
        {
            message = "The saved path has an invalid next ID.";
            return false;
        }

        HashSet<int> ids = new HashSet<int>();
        foreach (NodePathNode node in nodes)
        {
            if (node == null || node.id < 0 || node.id >= int.MaxValue - 1 || !ids.Add(node.id))
            {
                message = "The path contains a missing node or a repeated node ID.";
                return false;
            }
            outgoing.Add(node.id, new List<NodePathConnection>());
            incoming.Add(node.id, 0);
        }

        HashSet<string> pairs = new HashSet<string>();
        foreach (NodePathConnection connection in connections)
        {
            if (connection == null || connection.id < 0 || connection.id >= int.MaxValue - 1 || !ids.Add(connection.id))
            {
                message = "The path contains a missing connection or a repeated connection ID.";
                return false;
            }
            if (!outgoing.ContainsKey(connection.from) || !outgoing.ContainsKey(connection.to))
            {
                message = "A connection refers to a node that does not exist.";
                return false;
            }
            if (connection.from == connection.to)
            {
                message = "A node cannot connect to itself.";
                return false;
            }
            if (!pairs.Add(connection.from + ":" + connection.to))
            {
                message = "These nodes already have a connection in that direction.";
                return false;
            }
            outgoing[connection.from].Add(connection);
            incoming[connection.to]++;
        }
        return true;
    }

    private bool CheckForCycles(Dictionary<int, List<NodePathConnection>> outgoing,
        Dictionary<int, int> incoming)
    {
        Dictionary<int, int> remaining = new Dictionary<int, int>(incoming);
        Queue<int> ready = new Queue<int>();
        foreach (NodePathNode node in nodes)
        {
            if (remaining[node.id] == 0)
                ready.Enqueue(node.id);
        }

        int visited = 0;
        while (ready.Count > 0)
        {
            int id = ready.Dequeue();
            visited++;
            foreach (NodePathConnection connection in outgoing[id])
            {
                remaining[connection.to]--;
                if (remaining[connection.to] == 0)
                    ready.Enqueue(connection.to);
            }
        }
        return visited == nodes.Count;
    }

    // enumerate every directed entry-to-exit choice, including split-then-merge
    public bool BuildRoutes(out List<List<Vector3Int>> routes, out string message, int maxRoutes = 2048)
    {
        routes = new List<List<Vector3Int>>();
        message = "";
        Dictionary<int, List<NodePathConnection>> outgoing;
        Dictionary<int, int> incoming;
        if (!GetLinks(out outgoing, out incoming, out message))
            return false;
        if (!CheckForCycles(outgoing, incoming))
        {
            message = "Routes cannot be generated while the path contains a loop.";
            return false;
        }
        if (connections.Count == 0 || maxRoutes <= 0)
        {
            message = "Draw a path before generating routes.";
            return false;
        }

        NodePathNode start = null;
        foreach (NodePathNode node in nodes)
        {
            if (incoming[node.id] == 0)
            {
                if (start != null)
                {
                    message = "Routes need one connected starting node.";
                    return false;
                }
                start = node;
            }
        }

        if (start == null)
        {
            message = "The path has no starting node.";
            return false;
        }

        RebuildCells();
        List<Vector3Int> path = new List<Vector3Int>();
        path.Add(start.position);
        if (!AddRoutes(start.id, outgoing, path, routes, maxRoutes))
        {
            routes.Clear();
            message = "This layout creates more than " + maxRoutes + " routes. Reduce the number of splits.";
            return false;
        }
        return true;
    }

    private bool AddRoutes(int id, Dictionary<int, List<NodePathConnection>> outgoing,
        List<Vector3Int> path, List<List<Vector3Int>> routes, int maxRoutes)
    {
        if (outgoing[id].Count == 0)
        {
            if (routes.Count >= maxRoutes)
                return false;
            routes.Add(new List<Vector3Int>(path));
            return true;
        }

        foreach (NodePathConnection connection in outgoing[id])
        {
            int originalCount = path.Count;
            // skip the first cell: the preceding connection already added it
            for (int i = 1; i < connection.cells.Count; i++)
                path.Add(connection.cells[i]);

            if (!AddRoutes(connection.to, outgoing, path, routes, maxRoutes))
                return false;
            path.RemoveRange(originalCount, path.Count - originalCount);
        }
        return true;
    }
}
