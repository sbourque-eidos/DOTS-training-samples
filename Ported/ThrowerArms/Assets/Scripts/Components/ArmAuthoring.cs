
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ArmAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
	public float armBoneLength;
	public float armBoneThickness;
	public float armBendStrength;
	public float maxReachLength;
	public float maxHandSpeed;
	[Space(10)]
	public float[] fingerBoneLengths;
	public float[] fingerThicknesses;
	public float fingerXOffset;
	public float fingerSpacing;
	public float fingerBendStrength;
	[Space(10)]
	public float thumbBoneLength;
	public float thumbThickness;
	public float thumbBendStrength;
	public float thumbXOffset;
	[Space(10)]
	public GameObject bonePrefab;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
		referencedPrefabs.Add(bonePrefab);
    }

    void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		ComponentType[] armComponentTypes = new ComponentType[]
        {
		    typeof(JointEntity),
		    typeof(SkeletonReference)
        };

		const int segmentCount = 3;

		BlobAssetReference<Skeleton> skeletonRef = SkeletonAssetBuilder.BuildSkeleton();
		var skeletonRefComponent = new SkeletonReference { skeleton = skeletonRef };

		var comps = new ComponentTypes(armComponentTypes);
		dstManager.AddComponents(entity, comps);

		dstManager.AddComponentData(entity, skeletonRefComponent);

		var parent = entity;

		var joints = new NativeArray<Entity>(segmentCount + 1, Allocator.Temp);
		for (int jointIndex = 0; jointIndex < segmentCount+1; jointIndex++)
		{
			var newJoint = dstManager.Instantiate(conversionSystem.GetPrimaryEntity(bonePrefab));

			joints[jointIndex] = newJoint;

			dstManager.AddComponentData(newJoint, new Parent { Value = parent });
			dstManager.AddComponentData(newJoint, new LocalToParent { Value = float4x4.identity });
			dstManager.AddComponentData(newJoint, new NonUniformScale { Value = new float3(1.0f) });

			parent = newJoint;
		}

		var buffer = dstManager.AddBuffer<JointEntity>(entity);
		for (int jointIndex = 0; jointIndex < segmentCount + 1; jointIndex++)
		{
			buffer.Add(new JointEntity { Value = joints[jointIndex] });
		}

		joints.Dispose();
	}
}
