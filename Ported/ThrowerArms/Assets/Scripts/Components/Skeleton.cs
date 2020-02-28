
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct JointEntity : IBufferElementData
{
    public Entity Value;
}

public struct IKTarget : IComponentData
{
    public float3 Target;
}

public struct IKChain
{
    public int StartIndex;
    public int EndIndex;
}

public struct Skeleton
{
    public enum ChainIndex : int
    {
        Arm = 0,
        Count = 1
    }

    public BlobArray<IKChain> Chains;
    public BlobArray<float3> Transforms;
}

public struct SkeletonReference : IComponentData
{
    public BlobAssetReference<Skeleton> skeleton;
}

public static class SkeletonAssetBuilder
{
    public static BlobAssetReference<Skeleton> BuildSkeleton(float boneLength)
    {
        var blobBuilder = new BlobBuilder(Allocator.Temp);
        ref var skeleton = ref blobBuilder.ConstructRoot<Skeleton>();

        var nodearray = blobBuilder.Allocate(ref skeleton.Transforms, 3);

        nodearray[0] = new float3(0.0f, boneLength, 0.0f);
        nodearray[1] = new float3(0.0f, boneLength, 0.0f);
        nodearray[2] = new float3(0.0f, boneLength, 0.0f);

        return blobBuilder.CreateBlobAssetReference<Skeleton>(Allocator.Persistent);
    }
}
