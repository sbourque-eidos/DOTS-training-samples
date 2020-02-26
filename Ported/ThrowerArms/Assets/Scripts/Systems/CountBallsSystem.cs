using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(SpawnConstantlySystem))]
public class CountBallsSystem : SystemBase
{
    EntityQuery m_BallQuery;
    EntityQuery m_CounterQuery;

    protected override void OnCreate()
    {
        m_BallQuery = GetEntityQuery(ComponentType.ReadOnly<BallTag>());

        RequireForUpdate(m_CounterQuery);
    }

    protected override void OnUpdate()
    {
        int ballCount = m_BallQuery.CalculateEntityCount();

        Entities.WithAll<BallCounter>()
            .WithStoreEntityQueryInField(ref m_CounterQuery)
            .ForEach((ref CurrentCount count) =>
        {
            count.Value = ballCount;
        }).ScheduleParallel();
    }
}
