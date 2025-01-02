using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenLore.resource_manager.wld_file.data_types;
using OpenLore.resource_manager.wld_file.helpers;
using Animation = OpenLore.resource_manager.wld_file.data_types.Animation;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag10HierarchicalSpriteDef : WldFragment
{
    //public List<OldMesh> SecondaryAlternateMeshes = new List<OldMesh>();

    [Export] private bool _hasBuiltData;

    [Export] public Godot.Collections.Dictionary<string, Animation> Animations = [];
    [Export] public Godot.Collections.Dictionary<int, string> BoneMapping = [];
    [Export] public Godot.Collections.Dictionary<int, string> BoneMappingClean = [];
    [Export] public float BoundingRadius;
    [Export] public Array<Frag36DmSpriteDef2> NewMeshes = [];
    [Export] public Godot.Collections.Dictionary<int, Frag36DmSpriteDef2> NewMeshesByBone = [];

    //public List<OldMesh> AlternateMeshes {
    //    get; private set;
    //}
    [Export] public Array<SkeletonBone> Skeleton;

    //private PolyhedronReference _fragment18Reference;

    [Export] public string ModelBase;
    [Export] public bool IsAssigned;
    [Export] public Godot.Collections.Dictionary<string, SkeletonBone> SkeletonPieceDictionary;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);

        Skeleton = [];
        NewMeshes = [];
        //AlternateMeshes = new List<OldMesh>();
        SkeletonPieceDictionary = [];

        Name = wld.GetName(Reader.ReadInt32());
        ModelBase = FragmentNameCleaner.CleanName(this);

        // Always 2 when used in main zone, and object files.
        // This means, it has a bounding radius
        // Some differences in character + model archives
        // Confirmed
        var flags = Reader.ReadInt32();

        var ba = new BitAnalyzer(flags);

        var hasUnknownParams = ba.IsBitSet(0);
        var hasBoundingRadius = ba.IsBitSet(1);
        var hasMeshReferences = ba.IsBitSet(9);

        // Number of bones in the skeleton
        var boneCount = Reader.ReadInt32();

        // Fragment 18 reference
        var fragment18Reference = Reader.ReadInt32() - 1;

        //if (fragment18Reference > 0) {
        //    _fragment18Reference = fragments[fragment18Reference] as PolyhedronReference;
        //}

        // Three sequential DWORDs
        // This will never be hit for object animations.
        if (hasUnknownParams) Reader.BaseStream.Position += 3 * sizeof(int);

        // This is the sphere radius checked against the frustum to cull this object
        if (hasBoundingRadius) BoundingRadius = Reader.ReadSingle();

        for (var i = 0; i < boneCount; ++i)
        {
            var boneName = wld.GetName(Reader.ReadInt32());

            // Always 0 for object bones
            // Confirmed
            var boneFlags = Reader.ReadInt32();

            // Reference to a bone track
            // Confirmed - is never a bad reference
            var track = wld.GetFragment(Reader.ReadInt32()) as Frag13Track;
            AddPoseTrack(track, boneName);

            var pieceNew = new SkeletonBone
            {
                Index = i,
                Track = track,
                Name = boneName
            };

            pieceNew.Track!.IsPoseAnimation = true;
            pieceNew.AnimationTracks = [];

            BoneMappingClean[i] = Animation.CleanBoneAndStripBase(boneName, ModelBase);
            BoneMapping[i] = boneName;

            if (pieceNew.Track == null) GD.PrintErr("Unable to link track reference!");

            var meshReference = Reader.ReadInt32();
            // var meshName = wld.GetName(meshReference);
            var meshBoneRef = wld.GetFragment(meshReference) as Frag2DDmSprite;

            if (meshBoneRef?.NewMesh != null)
            {
                // GD.Print($"Found bone {boneName}({i}) with Mesh {meshBoneRef.NewMesh.Name} and ref {meshReference}");
                meshBoneRef.NewMesh.AttachedBoneId = i;
                pieceNew.NewMesh = meshBoneRef.NewMesh;
                NewMeshesByBone.Add(i, meshBoneRef.NewMesh);
                meshBoneRef.NewMesh.IsHandled = true;
            }

            //if (pieceNew.MeshReference == null) {
            //    pieceNew.ParticleCloud = fragments[meshReferenceIndex] as ParticleCloud;
            //}

            if (pieceNew.Name == "root") pieceNew.Name = FragmentNameCleaner.CleanName(meshBoneRef.NewMesh);

            var childCount = Reader.ReadInt32();

            pieceNew.Children = [];
            for (var j = 0; j < childCount; ++j) pieceNew.Children.Add(Reader.ReadInt32());

            Skeleton.Add(pieceNew);

            if (pieceNew.Name != "") SkeletonPieceDictionary.TryAdd(pieceNew.Name, pieceNew);
        }

        // Read in mesh references
        // All meshes will have vertex bone assignments
        if (hasMeshReferences)
        {
            var size2 = Reader.ReadInt32();

            for (var i = 0; i < size2; ++i)
            {
                var meshRef = wld.GetFragment(Reader.ReadInt32()) as Frag2DDmSprite;

                if (meshRef?.NewMesh != null)
                    if (NewMeshes.All(x => x.Name != meshRef.NewMesh.Name))
                    {
                        NewMeshes.Add(meshRef.NewMesh);
                        meshRef.NewMesh.IsHandled = true;
                    }

                //if (meshRef?.OldMesh != null) {
                //    if (AlternateMeshes.All(x => x.Name != meshRef.OldMesh.Name)) {
                //        AlternateMeshes.Add(meshRef.OldMesh);
                //    }
                //}
            }
            
            // ReSharper disable once CollectionNeverQueried.Local
            List<int> unknown = [];
            for (var i = 0; i < size2; ++i) unknown.Add(Reader.ReadInt32());
        }
        NewMeshes = [.. NewMeshes.OrderBy(x => x.Name)];
    }

    public void BuildSkeletonData(bool stripModelBase)
    {
        if (_hasBuiltData) return;

        BuildSkeletonTreeData(0, Skeleton, null, string.Empty,
            string.Empty, string.Empty, stripModelBase);
        _hasBuiltData = true;
    }

    private void AddPoseTrack(Frag13Track track, string pieceName)
    {
        if (!Animations.ContainsKey("pos")) Animations["pos"] = new Animation();

        Animations["pos"].AddTrack(track, pieceName, Animation.CleanBoneName(pieceName),
            Animation.CleanBoneAndStripBase(pieceName, ModelBase));
        track.TrackDefFragment.IsAssigned = true;
        track.IsProcessed = true;
        track.IsPoseAnimation = true;
    }


    private void BuildSkeletonTreeData(int index, Array<SkeletonBone> treeNodes, SkeletonBone parent,
        string runningName, string runningNameCleaned, string runningIndex, bool stripModelBase)
    {
        var bone = treeNodes[index];
        bone.Parent = parent;
        bone.CleanedName = CleanBoneName(bone.Name, stripModelBase);
        BoneMappingClean[index] = bone.CleanedName;

        if (bone.Name != string.Empty) runningIndex += bone.Index + "/";

        runningName += bone.Name;
        runningNameCleaned += bone.CleanedName;

        bone.FullPath = runningName;
        bone.CleanedFullPath = runningNameCleaned;

        if (bone.Children.Count == 0) return;

        runningName += "/";
        runningNameCleaned += "/";

        foreach (var childNode in bone.Children)
            BuildSkeletonTreeData(childNode, treeNodes, bone, runningName, runningNameCleaned, runningIndex,
                stripModelBase);
    }

    private string CleanBoneName(string nodeName, bool stripModelBase)
    {
        nodeName = nodeName.Replace("_DAG", "");
        nodeName = nodeName.ToLower();
        if (stripModelBase) nodeName = nodeName.Replace(ModelBase, string.Empty);

        nodeName += nodeName.Length == 0 ? "root" : string.Empty;
        return nodeName;
    }
}