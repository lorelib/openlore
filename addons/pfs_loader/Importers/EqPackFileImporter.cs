using System;
using Godot;
using Godot.Collections;

namespace OpenLore.addons.pfs_loader.Importers;

[Tool]
[GlobalClass]
public partial class EqPackFileImporter : EditorImportPlugin
{
    public override string _GetImporterName()
    {
        return "pfs.import.plugin";
    }

    public override string _GetVisibleName()
    {
        return "PFS Archive Import";
    }

    public override string[] _GetRecognizedExtensions()
    {
        return ["s3d", "eqg"];
    }

    public override string _GetSaveExtension()
    {
        return "tres";
    }

    public override string _GetResourceType()
    {
        return "PfsArchive";
    }

    public override int _GetPresetCount()
    {
        return 1;
    }

    public override string _GetPresetName(int presetIndex)
    {
        return "Default";
    }

    public override float _GetPriority()
    {
        return 1.0F;
    }

    public override int _GetImportOrder()
    {
        return 1;
    }

    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex)
    {
        return [];
    }

    public override Error _Import(string sourceFile, string savePath, Dictionary options,
        Array<string> platformVariants, Array<string> genFiles)
    {
        try
        {
            GD.Print($"PfsArchive: Finished post-processing archive: {sourceFile}");
            var destFile = $"{savePath}.{_GetSaveExtension()}";
            // var archive = PfsParser.Load(sourceFile);
            // var result = ResourceSaver.Save(archive, destFile);
            GD.Print($"PfsArchive: Finished writing archive: {sourceFile} to {destFile}");
            // return result;
        }
        catch (Exception e)
        {
            GD.PrintErr(e);
        }
        return Error.Failed;
    }
}