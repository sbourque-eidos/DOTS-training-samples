using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class ConstantSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject Prefab;
    public GameObject SpawnArea;
    public GameObject DeathPlane;
    public float Speed;
    public int TargetCount;

    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var bounds = SpawnArea.GetComponent<Renderer>().bounds;

        dstManager.AddComponentData(entity, new ConstantSpawner
        {
            MinArea = bounds.min,
            MaxArea = bounds.max,
            InitialVelocity = new float3(Speed, 0.0f, 0.0f),
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            TargetCount = TargetCount
        });
    }
    
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }
}
