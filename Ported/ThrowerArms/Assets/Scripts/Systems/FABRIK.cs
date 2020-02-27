using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public static class FABRIKSolver
{
    //handForward = (armChain.Last(0) - armChain.Last(1)).normalized;
    //handUp = Vector3.Cross(handForward, transform.right).normalized;
    // bend = handUp * someValue

    public static void Solve(
        NativeArray<float3> chain,
        float boneLength,
        float3 root,
        float3 target,
        float3 bendHint)
    {
        chain[chain.Length - 1] = target;
        for (int i = chain.Length - 2; i >= 0; i--)
        {
            chain[i] += bendHint;
            float3 delta = chain[i] - chain[i + 1];
            chain[i] = chain[i + 1] + math.normalize(delta) * boneLength;
        }

        chain[0] = root;
        for (int i = 1; i < chain.Length; i++)
        {
            float3 delta = chain[i] - chain[i - 1];
            chain[i] = chain[i - 1] + math.normalize(delta) * boneLength;
        }
    }
}

public class FABRIKSystem : JobComponentSystem
{
    [BurstCompile]
    struct FABRIKJob : IJobForEach_BCCC<JointEntity, SkeletonReference, Translation, IKTarget>
    {
        const float k_BoneLength = 1.0f;

        public void ComputeWorldChain(NativeArray<float3> chainCopy, ref BlobArray<float3> restPos, int startIndex, int length)
        {
            for (int i = 1; i < length; i++)
            {
                float3 newPos = chainCopy[i - 1] + restPos[startIndex + i];
                chainCopy[i] = newPos;
            }
        }

        public void Execute(DynamicBuffer<JointEntity> joints, ref SkeletonReference skeletonRef, ref Translation position, ref IKTarget target)
        {
            ref Skeleton skeleton = ref skeletonRef.skeleton.Value;
            ref BlobArray<float3> restPos = ref skeleton.Transforms;
            IKChain chain = skeleton.Chains[(int)Skeleton.ChainIndex.Arm];

            int length = chain.EndIndex - chain.StartIndex;
            var chainCopy = new NativeArray<float3>(length+1, Allocator.Temp);
            ComputeWorldChain(chainCopy, ref restPos, chain.StartIndex, length);

            FABRIKSolver.Solve(chainCopy, k_BoneLength, position.Value, target.Target, float3.zero);
            chainCopy.Dispose();
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new FABRIKJob
        {
        };

        return job.Schedule(this, inputDependencies);
    }
}