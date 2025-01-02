using Godot;
using OpenLore.resource_manager.wld_file.helpers;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag04SimpleSpriteDef : WldFragment
{
    [Export] public int Flags;
    [Export] public bool SkipFrames;
    [Export] public bool Unknown;
    [Export] public bool Animated;
    [Export] public bool Sleep;
    [Export] public bool CurrentFrameFlag;
    [Export] public int AnimationDelayMs;
    [Export] public uint CurrentFrame;
    [Export] public int BitmapCount;
    [Export] public Godot.Collections.Array<Frag03BmInfo> BitmapNames;


    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());

        Flags = Reader.ReadInt32();
        var bitAnalyzer = new BitAnalyzer(Flags);
        SkipFrames = bitAnalyzer.IsBitSet(1);
        Unknown = bitAnalyzer.IsBitSet(2);
        Animated = bitAnalyzer.IsBitSet(3);
        Sleep = bitAnalyzer.IsBitSet(4);
        CurrentFrameFlag = bitAnalyzer.IsBitSet(5);
        BitmapCount = Reader.ReadInt32();

        if (CurrentFrameFlag) CurrentFrame = Reader.ReadUInt32();
        if (Animated && Sleep) AnimationDelayMs = Reader.ReadInt32();

        BitmapNames = [];
        for (var i = 0; i < BitmapCount; ++i) BitmapNames.Add(wld.GetFragment(Reader.ReadInt32()) as Frag03BmInfo);
    }
}