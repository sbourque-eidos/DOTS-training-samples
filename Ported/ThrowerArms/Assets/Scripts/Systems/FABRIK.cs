using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

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

        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<Rotation> Rotations;

        public void Execute(DynamicBuffer<JointEntity> joints, ref SkeletonReference skeletonRef, ref Translation position, ref IKTarget target)
        {
            ref Skeleton skeleton = ref skeletonRef.skeleton.Value;
            ref BlobArray<float3> restPos = ref skeleton.Transforms;
            IKChain chain = skeleton.Chains[(int)Skeleton.ChainIndex.Arm];

            int length = chain.EndIndex - chain.StartIndex;
            var chainCopy = new NativeArray<float3>(length+1, Allocator.Temp);
            for (int i = 1; i < length; i++)
            {
                float3 newPos = restPos[chain.StartIndex + i];
                chainCopy[i] = newPos;
            }

            float3 localTarget = target.Target - position.Value;

            FABRIKSolver.Solve(chainCopy, k_BoneLength, float3.zero, localTarget, float3.zero);

            quaternion parentLocalRotation = quaternion.identity;
            for (int i = 0; i < length-1; i++)
            {
                float3 pos = chainCopy[i];
                float3 child = chainCopy[i + 1];

                float3 objectSpaceDirection = child - pos;

                quaternion objectSpaceRotation = Quaternion.FromToRotation(math.up(), objectSpaceDirection);
                quaternion localRotation = math.mul(math.inverse(parentLocalRotation), objectSpaceRotation);

                Entity jointEntity = joints[i].Value;
                Rotations[jointEntity] = new Rotation { Value = localRotation };

                parentLocalRotation = localRotation;
            }

            chainCopy.Dispose();
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var rotations = GetComponentDataFromEntity<Rotation>(false);

        var job = new FABRIKJob
        {
            Rotations = rotations
        };

        return job.Schedule(this, inputDependencies);
    }
}