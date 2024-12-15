using System;
using Godot;

namespace EQGodot.addons.pfs_loader.Importers;

[GlobalClass]
[Tool]
public partial class EqPackFileResourceLoader : ResourceFormatLoader
{
    public override string[] _GetRecognizedExtensions()
    {
        // GD.Print("_GetRecognizedExtensions");
        return ["s3d", "eqg"];
    }

    public override string _GetResourceScriptClass(string path)
    {
        // GD.Print($"_GetResourceScriptClass({path})");
        return "PfsArchive.cs";
    }

    public override string _GetResourceType(string path)
    {
        // GD.Print($"_GetResourceType({path})");
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file.GetError() != Error.Ok)
        {
            GD.PrintErr("Failed to open: ", path);
            throw new Exception($"Failed to open {path}");
        }

        file.BigEndian = false;

        var directoryOffset = file.Get32();
        var magicNumber = file.Get32();
        return magicNumber is 0x20534650 or 0x50465320 ? "PfsArchive" : "";
    }

    public override bool _HandlesType(StringName type)
    {
        // GD.Print($"_HandlesType({type})");
        return type == "PfsArchive";
    }

    public override Variant _Load(string path, string originalPath, bool useSubThreads, int cacheMode)
    {
        // GD.Print($"_Load({path}, {originalPath}, {useSubThreads}, {cacheMode})");
        // return PackFileParser.Load(path);
        return false;
    }
}