using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class UniformSpawnAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject Prefab;
    public int Count;
    public int Length;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var forward = transform.forward;
        var pos = transform.position;
        dstManager.AddComponentData(entity, new UniformSpawner()
        {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            Count = Count,
            Min = -0.5f * Length * forward + pos,
            Max =  0.5f * Length * forward + pos
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }
}
