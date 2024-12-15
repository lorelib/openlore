using System;
using System.Text.RegularExpressions;
using EQGodot.resource_manager.wld_file.fragments;
using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.godot_resources;

public partial class ActorSkeletonPath : Resource
{
    [GeneratedRegex(@"^[A-Z][0-9][0-9]")]
    private static partial Regex AnimatedTrackRegex();

    [Export] public string AnimationName;
    [Export] public int DefFlags;
    [Export] public int Flags;
    [Export] public string ActorName;
    [Export] public string Name;
    [Export] public string BoneName;
    [Export] public int FrameMs;

    [Export] public Array<Vector3> Translation;
    [Export] public Array<Quaternion> Rotation;
    [Export] public Array<float> Scale;

    public static ActorSkeletonPath FromFrag13Track(Frag13Track track)
    {
        string animationName = null;
        string actorName = null;
        string boneName = null;
        if (AnimatedTrackRegex().IsMatch(track.Name))
        {
            animationName = track.Name.Substr(0, 3).ToLower();
            actorName = track.Name.Substr(3, 3).ToLower();
            boneName = track.Name[6..].ToLower().Replace("_track", "");
            if (boneName == "") boneName = "root";
        }

        var result = new ActorSkeletonPath
        {
            Name = track.Name.ToLower(),
            AnimationName = animationName,
            ActorName = actorName,
            BoneName = boneName,
            FrameMs = track.FrameMs,
            Flags = track.Flags,
            DefFlags = track.TrackDefFragment.Flags,
            Translation = [],
            Rotation = [],
            Scale = []
        };

        foreach (var frame in track.TrackDefFragment.Frames)
        {
            result.Translation.Add(frame.Translation);
            result.Rotation.Add(frame.Rotation);
            result.Scale.Add(frame.Scale);
        }

        return result;
    }

    public void ApplyToAnimation(Animation animation)
    {
        var bonePath = new NodePath($":{BoneName}");
        var posIdx = animation.AddTrack(Animation.TrackType.Position3D);
        animation.TrackSetPath(posIdx, bonePath);

        var rotIdx = animation.AddTrack(Animation.TrackType.Rotation3D);
        animation.TrackSetPath(rotIdx, bonePath);
        animation.TrackSetInterpolationType(
            rotIdx,
            Animation.InterpolationType.LinearAngle
        );
        for (var frame = 0; frame < Translation.Count; frame++)
        {
            animation.PositionTrackInsertKey(
                posIdx,
                frame * 0.001f * FrameMs,
                Translation[frame]
            );
            animation.RotationTrackInsertKey(
                rotIdx,
                frame * 0.001f * FrameMs,
                Rotation[frame]
            );
        }

        animation.Length = Math.Max(
            animation.Length,
            Translation.Count * 0.001f * FrameMs
        );
    }
}