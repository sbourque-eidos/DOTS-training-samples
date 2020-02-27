
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public class ArmData : MonoBehaviour, IConvertGameObjectToEntity
{
	public float armBoneLength;
	public float armBoneThickness;
	public float armBendStrength;
	public float maxReachLength;
	public float reachDuration;
	public float maxHandSpeed;
	[Range(0f,1f)]
	public float grabTimerSmooth;
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

	Rock intendedRock;
	Rock heldRock;
	TinCan targetCan;

	Vector3[] armChain;
	Vector3[][] fingerChains;
	Vector3[] thumbChain;
	Matrix4x4[] matrices;
	Vector3 handTarget;

	Vector3 handForward;
	Vector3 handUp;
	Vector3 handRight;
	Matrix4x4 handMatrix;

	Vector3 grabHandTarget;
	Vector3 lastIntendedRockPos;
	float lastIntendedRockSize;
	Vector3 windupHandTarget;
	Vector3 heldRockOffset;
	Vector3 aimVector;
	float reachTimer;
	float windupTimer;
	float throwTimer;
	float timeOffset;
   
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
