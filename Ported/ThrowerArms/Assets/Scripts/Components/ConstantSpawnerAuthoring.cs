using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class ConstantSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject Prefab;
    public GameObject SpawnArea;
    public GameObject DeathPlane;

    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var bounds = SpawnArea.GetComponent<Renderer>().bounds;

        dstManager.AddComponentData(entity, new ConstantSpawner
        {
            MinArea = bounds.min,
            MaxArea = bounds.max,
            Prefab = conversionSystem.GetPrimaryEntity(Prefab)
        });
    }
    
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }
}
