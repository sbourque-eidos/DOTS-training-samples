
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(DetectCollisionSystem))]
public class ProcessCollisionSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    
    [BurstCompile]
    struct ProcessCollisionJob : IJob
    {
        public NativeQueue<ContactPoint> Contacts;
        public ComponentDataFromEntity<Velocity> Velocities;
        public ComponentDataFromEntity<Mass> Masses;
        [ReadOnly] public ComponentDataFromEntity<Translation> Positions;

        public EntityCommandBuffer EntityCommandBuffer;
        const float hollywoodFactor = 5.0f;

        public void Execute()
        {
            while(Contacts.Count > 0)
            {
                ContactPoint contact = Contacts.Dequeue();

                Velocity velAComp = Velocities[contact.EntityA];
                Velocity velBComp = Velocities[contact.EntityB];

                float3 velA = velAComp.Value;
                float3 velB = velBComp.Value;

                // TODO: take mass into account
                float massA = Masses[contact.EntityA].Value;
                float massB = Masses[contact.EntityB].Value;
                float restitution = 0.9f;

                float mult = restitution / (massA + massB);
                float massDiff = massB - massA;

                velBComp.Value = mult * (2.0f * massA * velA + massDiff * velB);
                velAComp.Value = mult * (2.0f * massB * velB - massDiff * velA);

                Velocities[contact.EntityA] = velAComp;
                Velocities[contact.EntityB] = velBComp;

                float3 arm = Positions[contact.EntityB].Value - contact.Position;
                float3 torque = math.cross(velA, arm);

                float3 angVel = hollywoodFactor * torque;
                var angularVelocity = new AngularVelocity { Value = angVel };

                EntityCommandBuffer.AddComponent<FreeFalling>(contact.EntityB);
                EntityCommandBuffer.AddComponent(contact.EntityB, angularVelocity);
                EntityCommandBuffer.RemoveComponent<Target>(contact.EntityA);
            }
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer();

        var contactsQueue = DetectCollisionSystem.Contacts;

        JobHandle handle = JobHandle.CombineDependencies(inputDependencies, DetectCollisionSystem.Handle);
        var uiJobHandle = Entities.WithReadOnly(contactsQueue)
            .ForEach((ref UIData uiData) =>
        {
            uiData.ballHits += contactsQueue.Count;
        }).Schedule(handle);

        var velocities = GetComponentDataFromEntity<Velocity>(false);
        var masses = GetComponentDataFromEntity<Mass>(false);

        var positions = GetComponentDataFromEntity<Translation>(true);
        var job = new ProcessCollisionJob
        {
            Contacts = contactsQueue,
            Velocities = velocities,
            Masses = masses,
            Positions = positions,
            EntityCommandBuffer = ecb
        };

        handle = job.Schedule(uiJobHandle);

        m_EntityCommandBufferSystem.AddJobHandleForProducer(handle);

        return handle;
    }
}
