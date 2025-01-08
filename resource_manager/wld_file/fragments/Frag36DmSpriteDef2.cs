using System;
using Godot;
using OpenLore.resource_manager.interfaces;
using OpenLore.resource_manager.wld_file.data_types;
using Array = Godot.Collections.Array;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class adapted for Godot
[GlobalClass]
public partial class Frag36DmSpriteDef2 : WldFragment, IIntoGodotMesh
{
    [Export] public int Flags;
    [Export] public Frag31MaterialPalette MaterialPalette;
    [Export] public int AnimatedVerticesReference;
    [Export] public Vector3 Centre;
    [Export] public float MaxDistance;
    [Export] public Vector3 MinPosition;
    [Export] public Vector3 MaxPosition;

    [Export] public bool IsHandled = false;
    [Export] public int AttachedBoneId = -1;

    [Export] public int StartTextureIndex;
    [Export] public Vector3[] Vertices;
    [Export] public Vector3[] Normals;
    [Export] public Godot.Collections.Array<Polygon> Indices;
    [Export] public Color[] Colors;
    [Export] public Vector2[] TextureUvCoordinates;
    [Export] public Godot.Collections.Array<RenderGroup> MaterialGroups;
    [Export] public bool ExportSeparateCollision;
    [Export] public Godot.Collections.Array<MobVertexPiece> MobPieces;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());

        // Zone: 0x00018003, Objects: 0x00014003
        Flags = Reader.ReadInt32();

        MaterialPalette = wld.GetFragment(Reader.ReadInt32()) as Frag31MaterialPalette;
        AnimatedVerticesReference = Reader.ReadInt32();

        var unknown1 = Reader.ReadInt32();
        var unknown2 = Reader.ReadInt32();

        Centre = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());

        // 3 unknown dwords
        var unknownDword1 = Reader.ReadInt32();
        var unknownDword2 = Reader.ReadInt32();
        var unknownDword3 = Reader.ReadInt32();

        // Seems to be related to lighting models? (torches, etc.)
        if (unknownDword1 != 0 || unknownDword2 != 0 || unknownDword3 != 0)
            GD.PrintErr(
                $"WldMesh: {Name} unknown1 {unknownDword1:X} unknown2 {unknownDword2:X} unknown3 {unknownDword3:X}");

        MaxDistance = Reader.ReadSingle();
        MinPosition = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
        MaxPosition = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());

        var vertexCount = Reader.ReadInt16();
        var textureCoordinateCount = Reader.ReadInt16();
        var normalsCount = Reader.ReadInt16();
        var colorsCount = Reader.ReadInt16();
        var polygonCount = Reader.ReadInt16();
        var vertexPieceCount = Reader.ReadInt16();
        var polygonTextureCount = Reader.ReadInt16();
        var vertexTextureCount = Reader.ReadInt16();
        var meshOpCount = Reader.ReadInt16();
        var scale = 1.0f / (1 << Reader.ReadInt16());

        Vertices = new Vector3[vertexCount];
        Normals = new Vector3[normalsCount];
        Colors = new Color[colorsCount];
        TextureUvCoordinates = new Vector2[textureCoordinateCount];

        for (var i = 0; i < vertexCount; ++i)
        {
            var x = Reader.ReadInt16() * scale;
            var y = Reader.ReadInt16() * scale;
            var z = Reader.ReadInt16() * scale;
            Vertices[i] = new Vector3(x, y, z) + Centre;
        }

        for (var i = 0; i < textureCoordinateCount; ++i)
            if (wld.NewFormat)
                TextureUvCoordinates[i] = new Vector2(Reader.ReadSingle(), Reader.ReadSingle());
            else
                TextureUvCoordinates[i] = new Vector2(Reader.ReadInt16() / 256.0f, Reader.ReadInt16() / 256.0f);

        for (var i = 0; i < normalsCount; ++i)
        {
            var x = Reader.ReadSByte() / 128.0f;
            var y = Reader.ReadSByte() / 128.0f;
            var z = Reader.ReadSByte() / 128.0f;
            Normals[i] = new Vector3(x, y, z).Normalized();
        }

        for (var i = 0; i < colorsCount; ++i)
        {
            var colorBytes = BitConverter.GetBytes(Reader.ReadInt32());
            int b = colorBytes[0];
            int g = colorBytes[1];
            int r = colorBytes[2];
            int a = colorBytes[3];

            Colors[i] = new Color(r, g, b, a);
        }

        Indices = [];

        for (var i = 0; i < polygonCount; ++i)
        {
            var isSolid = Reader.ReadInt16() == 0;

            if (!isSolid) ExportSeparateCollision = true;

            Indices.Add(new Polygon
            {
                IsSolid = isSolid,
                Vertex1 = Reader.ReadInt16(),
                Vertex2 = Reader.ReadInt16(),
                Vertex3 = Reader.ReadInt16()
            });
        }

        MobPieces = [];
        var mobStart = 0;

        for (var i = 0; i < vertexPieceCount; ++i)
        {
            int count = Reader.ReadInt16();
            int index1 = Reader.ReadInt16();
            var mobVertexPiece = new MobVertexPiece
            {
                Count = count,
                Start = mobStart,
                Bone = index1
            };

            mobStart += count;

            MobPieces.Add(mobVertexPiece);
        }

        MaterialGroups = [];

        StartTextureIndex = int.MaxValue;
        var startPolygon = 0;

        for (var i = 0; i < polygonTextureCount; ++i)
        {
            var group = new RenderGroup
            {
                StartPolygon = startPolygon,
                PolygonCount = Reader.ReadUInt16(),
                MaterialIndex = Reader.ReadUInt16()
            };
            MaterialGroups.Add(group);

            startPolygon += group.PolygonCount;

            if (group.MaterialIndex < StartTextureIndex) StartTextureIndex = group.MaterialIndex;
        }

        for (var i = 0; i < vertexTextureCount; ++i) Reader.BaseStream.Position += 4;

        for (var i = 0; i < meshOpCount; ++i) Reader.BaseStream.Position += 12;

        // In some rare cases, the number of uvs does not match the number of vertices
        if (Vertices.Length == TextureUvCoordinates.Length) return;

        var difference = Vertices.Length - TextureUvCoordinates.Length;
        if (difference > 0 && TextureUvCoordinates.Length > 0)
        {
            GD.PrintErr(
                $"Name {Name} Vertices {Vertices.Length} TextureUvCoordinates {TextureUvCoordinates.Length} = {difference}");
        }

        // for (var i = 0; i < difference; ++i)
        //    TextureUvCoordinates[TextureUvCoordinates.Length + i] = new Vector2(0.0f, 0.0f);
    }

    public void ClearCollision()
    {
        foreach (var poly in Indices) poly.IsSolid = false;

        ExportSeparateCollision = true;
    }

    public ArrayMesh ToGodotMesh()
    {
        var arrays = new Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        arrays[(int)Mesh.ArrayType.Vertex] = Vertices;
        arrays[(int)Mesh.ArrayType.Normal] = Normals;

        if (Colors.Length > 0)
            arrays[(int)Mesh.ArrayType.Color] = Colors;

        if (TextureUvCoordinates.Length > 0)
            arrays[(int)Mesh.ArrayType.TexUV] = TextureUvCoordinates;

        if (AttachedBoneId >= 0)
        {
            var bones = new int[Vertices.Length * 4];
            var weights = new float[Vertices.Length * 4];
            for (var j = 0; j < Vertices.Length; j++)
            {
                bones[j * 4 + 0] = AttachedBoneId;
                bones[j * 4 + 1] = 0;
                bones[j * 4 + 2] = 0;
                bones[j * 4 + 3] = 0;
                weights[j * 4 + 0] = 1.0f;
                weights[j * 4 + 1] = 0.0f;
                weights[j * 4 + 2] = 0.0f;
                weights[j * 4 + 3] = 0.0f;
            }

            arrays[(int)Mesh.ArrayType.Bones] = bones;
            arrays[(int)Mesh.ArrayType.Weights] = weights;
        }

        if (MobPieces.Count > 0)
        {
            var bones = new int[Vertices.Length * 4];
            var weights = new float[Vertices.Length * 4];

            foreach (var t in MobPieces)
            {
                for (var j = 0; j < t.Count; j++)
                {
                    var startIndex = t.Start + j;
                    bones[startIndex * 4 + 0] = t.Bone;
                    bones[startIndex * 4 + 1] = 0;
                    bones[startIndex * 4 + 2] = 0;
                    bones[startIndex * 4 + 3] = 0;
                    weights[startIndex * 4 + 0] = 1.0f;
                    weights[startIndex * 4 + 1] = 0.0f;
                    weights[startIndex * 4 + 2] = 0.0f;
                    weights[startIndex * 4 + 3] = 0.0f;
                }
            }

            arrays[(int)Mesh.ArrayType.Bones] = bones;
            arrays[(int)Mesh.ArrayType.Weights] = weights;
        }

        var mesh = new ArrayMesh();
        for (var j = 0; j < MaterialGroups.Count; j++)
        {
            var group = MaterialGroups[j];
            var indices = new int[group.PolygonCount * 3];
            for (var i = 0; i < group.PolygonCount; i++)
            {
                indices[i * 3 + 0] = Indices[group.StartPolygon + i].Vertex1;
                indices[i * 3 + 1] = Indices[group.StartPolygon + i].Vertex2;
                indices[i * 3 + 2] = Indices[group.StartPolygon + i].Vertex3;
            }

            arrays[(int)Mesh.ArrayType.Index] = indices;

            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
            mesh.SurfaceSetMaterial(j, Wld.Materials[MaterialPalette.Materials[group.MaterialIndex].Index]);
            mesh.SurfaceSetName(j, Wld.Materials[MaterialPalette.Materials[group.MaterialIndex].Index].GetName());
        }

        mesh.ResourceName = Name;
        return mesh;
    }
}