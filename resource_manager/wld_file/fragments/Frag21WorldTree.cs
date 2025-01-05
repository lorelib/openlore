using System.Collections.Generic;
using Godot;
using Godot.Collections;
using OpenLore.resource_manager.interfaces;
using OpenLore.resource_manager.wld_file.data_types;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag21WorldTree : WldFragment, IIntoGodotZone
{
    [Export] public Array<BspNode> Nodes;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        var nodeCount = Reader.ReadInt32();
        Nodes = [];

        for (var i = 0; i < nodeCount; ++i)
            Nodes.Add(new BspNode
            {
                NormalX = Reader.ReadSingle(),
                NormalY = Reader.ReadSingle(),
                NormalZ = Reader.ReadSingle(),
                SplitDistance = Reader.ReadSingle(),
                RegionId = Reader.ReadInt32(),
                LeftNode = Reader.ReadInt32() - 1,
                RightNode = Reader.ReadInt32() - 1
            });
    }

    public void LinkBspRegions(List<Frag22Region> fragments)
    {
        foreach (var node in Nodes)
        {
            if (node.RegionId == 0) continue;

            node.Region = fragments[node.RegionId - 1];
        }
    }

    public Node3D ToGodotZone()
    {
        var zone = new Node3D();

        var queue = new Queue<int>();
        queue.Enqueue(0);
        while (queue.TryDequeue(out var index))
        {
            var node = Nodes[index];
            if (node.RegionId != 0)
            {
                var mesh = node.Region.Mesh;
                if (mesh != null)
                {
                    var arrayMesh = mesh.ToGodotMesh();
                    var inst = new MeshInstance3D { Name = mesh.Name, Mesh = arrayMesh };
                    zone.AddChild(inst);
                }
            }

            if (node.LeftNode != -1)
            {
                queue.Enqueue(node.LeftNode);
            }

            if (node.RightNode != -1)
            {
                queue.Enqueue(node.RightNode);
            }
        }

        return zone;
    }
}