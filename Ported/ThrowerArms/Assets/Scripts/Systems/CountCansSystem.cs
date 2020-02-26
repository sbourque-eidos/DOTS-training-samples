using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(SpawnConstantlySystem))]
public class CountCansSystem : SystemBase
{
    EntityQuery m_CanQuery;
    EntityQuery m_CounterQuery;

    protected override void OnCreate()
    {
        m_CanQuery = GetEntityQuery(ComponentType.ReadOnly<CanTag>());

        RequireForUpdate(m_CounterQuery);
    }

    protected override void OnUpdate()
    {
        int ballCount = m_CanQuery.CalculateEntityCount();

        Entities.WithAll<CanCounter>()
            .WithStoreEntityQueryInField(ref m_CounterQuery)
            .ForEach((ref CurrentCount count) =>
        {
            count.Value = ballCount;
        }).ScheduleParallel();
    }
}
