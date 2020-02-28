using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class UISystem : SystemBase
{
    private EntityQuery m_cylindersQuery;
    private EntityQuery m_ballQuery;
    protected override void OnCreate()
    {
        base.OnCreate();

        var uiDataEntity = EntityManager.CreateEntity(typeof(UIData));

        m_cylindersQuery = GetEntityQuery(typeof(CanTag));
        m_ballQuery = GetEntityQuery(typeof(BallTag));

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        var uiDataEntity = GetSingletonEntity<UIData>();
        EntityManager.DestroyEntity(uiDataEntity);
    }

    protected override void OnUpdate()
    {
        var uiData = GetSingleton<UIData>();
        var canCount = m_cylindersQuery.CalculateEntityCount();
        var ballCount = m_ballQuery.CalculateEntityCount();

        var ballHits = DetectCollisionSystem.Contacts.Count;

        uiData.canCount.CopyFrom(canCount.ToString());
        uiData.ballCount.CopyFrom(ballCount.ToString());

        var uiDataEntity = GetSingletonEntity<UIData>();
        EntityManager.SetComponentData<UIData>(uiDataEntity, uiData);
    }
}
