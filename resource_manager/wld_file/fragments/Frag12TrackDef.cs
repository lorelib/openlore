using System.IO;
using Godot;
using Godot.Collections;
using OpenLore.resource_manager.wld_file.data_types;
using OpenLore.resource_manager.wld_file.helpers;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag12TrackDef : WldFragment
{
    [Export] public int Flags;
    [Export] public bool IsAssigned;
    [Export] public Array<BoneTransform> Frames;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);

        Reader = new BinaryReader(new MemoryStream(data));
        Name = wld.GetName(Reader.ReadInt32());

        Flags = Reader.ReadInt32();

        // Flags are always 8 when dealing with object animations
        if (Flags != 8)
        {
        }

        BitAnalyzer bitAnalyzer = new(Flags);

        var isS3dTrack2 = bitAnalyzer.IsBitSet(3);

        var frameCount = Reader.ReadInt32();

        Frames = [];

        if (isS3dTrack2)
            for (var i = 0; i < frameCount; ++i)
            {
                var rotW = Reader.ReadInt16();
                var rotX = Reader.ReadInt16();
                var rotY = Reader.ReadInt16();
                var rotZ = Reader.ReadInt16();
                var shiftX = Reader.ReadInt16();
                var shiftY = Reader.ReadInt16();
                var shiftZ = Reader.ReadInt16();
                var shiftDenominator = Reader.ReadInt16();

                var frameTransform = new BoneTransform();

                if (shiftDenominator != 0)
                {
                    var x = shiftX / 256f;
                    var y = shiftY / 256f;
                    var z = shiftZ / 256f;

                    frameTransform.Scale = shiftDenominator / 256f;
                    frameTransform.Translation = new Vector3(x, y, z);
                }
                else
                {
                    frameTransform.Translation = Vector3.Zero;
                }

                frameTransform.Rotation = new Quaternion(rotX, rotY, rotZ, rotW).Normalized();
                Frames.Add(frameTransform);
            }
        else
            for (var i = 0; i < frameCount; ++i)
            {
                var shiftDenominator = Reader.ReadSingle();
                var shiftX = Reader.ReadSingle();
                var shiftY = Reader.ReadSingle();
                var shiftZ = Reader.ReadSingle();
                var rotW = Reader.ReadSingle();
                var rotX = Reader.ReadSingle();
                var rotY = Reader.ReadSingle();
                var rotZ = Reader.ReadSingle();

                var frameTransform = new BoneTransform
                {
                    Scale = shiftDenominator,
                    Translation = new Vector3(shiftX, shiftY, shiftZ),
                    Rotation = new Quaternion(rotX, rotY, rotZ, rotW).Normalized()
                };

                Frames.Add(frameTransform);
            }
    }
}