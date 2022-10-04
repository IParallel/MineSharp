﻿using MineSharp.Core.Logging;
using MineSharp.Core.Types;
using MineSharp.Data.Blocks;
using MineSharp.Pathfinding.Goals;
using MineSharp.Pathfinding.Moves;
using MineSharp.World;
using System.Reflection.Metadata;
using Priority_Queue;

namespace MineSharp.Pathfinding.Algorithm
{
    public class AStar
    {
        private static readonly Logger Logger = Logger.GetLogger();
        public const int DEFAULT_MAX_NODES = 5000;
        
        public MinecraftPlayer Player { get; set; }
        public World.World World { get; set; }
        public Movements Movements { get; set; }

        public AStar(MinecraftPlayer player, World.World world, Movements? movements = null)
        {
            this.Player = player;
            this.World = world;
            this.Movements = movements ?? new Movements();
        }

        public Node[] ComputePath(Goal goal, double? timeout = null, int maxNodes = DEFAULT_MAX_NODES)
        {
            var startTime = DateTime.Now;

            var openSet = new FastPriorityQueue<Node>(maxNodes);
            var closedSet = new HashSet<Node>();
            var nodes = new Dictionary<ulong, Node>();

            var pos = Player.Entity.Position.Floored();

            using (_ = new TemporaryBlockCache(this.World))
            {
                var startNode = GetNodeForBlock(pos, ref nodes);
                var endNode = GetNodeForBlock(goal.Target, ref nodes);

                if (!endNode.Walkable)
                {
                    throw new Exception($"Target block is not walkable");
                }

                openSet.Enqueue(startNode, startNode.fCost);

                while (openSet.Count > 0)
                {
                    if (timeout != null && (DateTime.Now - startTime).TotalMilliseconds > timeout)
                    {
                        throw new Exception($"Could not find a path after {Math.Round((DateTime.Now - startTime).TotalMilliseconds * 10) / 10}ms");
                    }

                    var node = openSet.Dequeue();
                    Logger.Debug($"Checking {node}");

                    closedSet.Add(node);

                    if (node.Position == goal.Target.Floored())
                    {
                        Logger.Debug($"Checked {nodes.Count} nodes");
                        var path = new List<Node>();
                        var currentNode = node;

                        while (currentNode != startNode)
                        {
                            path.Add(currentNode!);
                            currentNode = currentNode!.Parent!;
                        }
                        path.Add(startNode);
                        path.Reverse();
                        Logger.Debug($"Found Path with {path.Count} nodes");
                        return path.ToArray();
                    }

                    var neighbors = GetNeighbors(node, ref nodes).ToArray();

                    foreach (var neighbor in neighbors)
                    {
                        if (!neighbor.Walkable || closedSet.Contains(neighbor))
                        {
                            continue;
                        }

                        var newCost = node.gCost + (float)goal.Target.DistanceSquared(neighbor.Position);
                        if (newCost < neighbor.gCost || !openSet.Contains(node))
                        {
                            neighbor.gCost = newCost;
                            neighbor.hCost = (float)goal.Target.DistanceSquared(neighbor.Position);
                            neighbor.Parent = node;

                            if (!openSet.Contains(neighbor))
                            {
                                openSet.Enqueue(neighbor, neighbor.fCost);
                            }
                            else
                            {
                                openSet.UpdatePriority(neighbor, neighbor.fCost);
                            }
                        }
                    }
                }
            }

            throw new Exception("No path found");
        }

        private List<Node> GetNeighbors(Node node, ref Dictionary<ulong, Node> nodes)
        {
            var neighbors = new List<Node>();    

            foreach (var move in this.Movements.PossibleMoves)
            {
                var pos = node.Position.Plus(move.MoveVector);
                var neighborNode = GetNodeForBlock(pos, ref nodes);
                neighbors.Add(neighborNode);
            }

            return neighbors;
        }

        private bool IsPositionWalkable(Vector3 pos)
        {
            /*
             * Currently, a position is considered walkable,
             * when the block at the position has an empty bounding box, 
             * the block above has an empty bounding box,
             * and the block has a bounding box of type block
             * 
             * TODO: Instead of using block.BoundingBox use AABB's
             */

            var block = World.GetBlockAt(pos);
            if (block.BoundingBox == "empty")
            {
                var blockAbove = World.GetBlockAt(pos.Plus(Vector3.Up));
                var blockBelow = World.GetBlockAt(pos.Plus(Vector3.Down));
                if (blockAbove.BoundingBox == "empty" && blockBelow.BoundingBox == "block")
                {
                    return true;
                }
            }

            return false;
        }

        private Node GetNodeForBlock(Vector3 pos, ref Dictionary<ulong, Node> nodes)
        {
            pos = pos.Floored();
            if (nodes.TryGetValue(((Position)pos).ToULong(), out var node))
            {
                return node;
            }

            var walkable = IsPositionWalkable(pos);

            var newNode = new Node(pos, walkable, 0, 0);
            nodes.Add(((Position)pos).ToULong(), newNode);
            return newNode;
        }
    }
}
