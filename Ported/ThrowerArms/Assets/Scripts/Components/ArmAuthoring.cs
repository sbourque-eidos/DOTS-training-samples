
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public class ArmAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	const float armBoneThickness = 0.1f;
	const float armBendStrength = 0.0f;

	public float armBoneLength = 1.0f;
	public float reachDuration = 1.0f;
	public float windupDuration = 1.0f;
	public float throwDuration = 1.0f;

	public Material material;
	public Mesh boneMesh;
   
	static readonly float3 k_Right = new float3(1.0f, 0.0f, 0.0f);

	void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		ComponentType[] componentTypes = new ComponentType[]
        {
		    typeof(JointEntity),
		    typeof(SkeletonReference),
		    typeof(Translation)
        };

		ComponentType[] jointTypes = new ComponentType[]
		{
	        typeof(Translation),
	        typeof(Rotation),
	        typeof(Parent)
		};

		const int segmentCount = 3;

		BlobAssetReference<Skeleton> skeletonRef = SkeletonAssetBuilder.BuildSkeleton(armBoneLength);
		var skeletonRefComponent = new SkeletonReference { skeleton = skeletonRef };

		var comps = new ComponentTypes(componentTypes);
		dstManager.AddComponents(entity, comps);

		dstManager.AddComponentData(entity, skeletonRefComponent);

		var joints = new NativeArray<Entity>(segmentCount + 1, Allocator.Temp);
		for (int jointIndex = 0; jointIndex < segmentCount+1; jointIndex++)
		{
			Entity newJoint = dstManager.CreateEntity(jointTypes);
			joints[jointIndex] = newJoint;
		}

		var buffer = dstManager.AddBuffer<JointEntity>(entity);
		for (int jointIndex = 0; jointIndex < segmentCount + 1; jointIndex++)
		{
			buffer.Add(new JointEntity { Value = joints[jointIndex] });
		}
	}
}
