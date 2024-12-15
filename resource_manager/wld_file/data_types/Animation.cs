using System.Collections.Generic;
using Godot;
using OpenLore.resource_manager.wld_file.fragments;

namespace OpenLore.resource_manager.wld_file.data_types;

// Lantern Extractor class
[GlobalClass]
public partial class Animation : Resource
{
    public string AnimModelBase;
    public int FrameCount;
    public Dictionary<string, Frag13Track> Tracks;
    public Dictionary<string, Frag13Track> TracksCleaned;
    public Dictionary<string, Frag13Track> TracksCleanedStripped;

    public Animation()
    {
        Tracks = [];
        TracksCleaned = [];
        TracksCleanedStripped = [];
    }

    public int AnimationTimeMs;

    public static string CleanBoneName(string boneName)
    {
        if (string.IsNullOrEmpty(boneName)) return boneName;

        var cleanedName = CleanBoneNameDag(boneName);
        return cleanedName.Length == 0 ? "root" : cleanedName;
    }

    public static string CleanBoneAndStripBase(string boneName, string modelBase)
    {
        var cleanedName = CleanBoneNameDag(boneName);

        if (cleanedName.StartsWith(modelBase)) cleanedName = cleanedName.Substring(modelBase.Length);

        return cleanedName.Length == 0 ? "root" : cleanedName;
    }

    public static string CleanBoneNameDag(string boneName)
    {
        if (string.IsNullOrEmpty(boneName)) return boneName;

        return boneName.ToLower().Replace("_dag", string.Empty);
    }

    public void AddTrack(Frag13Track track, string pieceName, string cleanedName, string cleanStrippedName)
    {
        // Prevent overwriting tracks
        // Drachnid edge case
        if (Tracks.ContainsKey(pieceName)) return;

        Tracks[pieceName] = track;
        TracksCleaned[cleanedName] = track;
        TracksCleanedStripped[cleanStrippedName] = track;

        if (string.IsNullOrEmpty(AnimModelBase) &&
            !string.IsNullOrEmpty(track.ModelName))
            AnimModelBase = track.ModelName;

        if (track.TrackDefFragment.Frames.Count > FrameCount) FrameCount = track.TrackDefFragment.Frames.Count;

        var totalTime = track.TrackDefFragment.Frames.Count * track.FrameMs;

        if (totalTime > AnimationTimeMs) AnimationTimeMs = totalTime;
    }
}