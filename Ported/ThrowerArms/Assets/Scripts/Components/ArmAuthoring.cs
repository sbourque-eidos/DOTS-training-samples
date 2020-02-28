
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public class ArmAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	public float armBoneLength;
	public float armBoneThickness;
	public float armBendStrength;
	public float maxReachLength;
	public float reachDuration;
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
	public float windupDuration;
	public float throwDuration;
	public AnimationCurve throwCurve;
	public float baseThrowSpeed;
	public float targetXRange;
	[Space(10)]
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

		BlobAssetReference<Skeleton> skeletonRef = SkeletonAssetBuilder.BuildSkeleton();
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
