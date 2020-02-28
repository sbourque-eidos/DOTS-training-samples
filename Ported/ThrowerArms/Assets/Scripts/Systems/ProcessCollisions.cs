
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateAfter(typeof(DetectCollisionSystem))]
public class ProcessCollisionSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    
    [BurstCompile]
    struct ProcessCollisionJob : IJob
    {
        public NativeQueue<ContactPoint> Contacts;
        public ComponentDataFromEntity<Velocity> Velocities;
        public EntityCommandBuffer EntityCommandBuffer;

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
                float massA = 1.0f;
                float massB = 0.4f;
                float restitution = 0.9f;

                float mult = restitution / (massA + massB);
                float massDiff = massB - massA;

                velBComp.Value = mult * (2.0f * massA * velA + massDiff * velB);
                velAComp.Value = mult * (2.0f * massB * velB - massDiff * velA);

                Velocities[contact.EntityA] = velAComp;
                Velocities[contact.EntityB] = velBComp;

                EntityCommandBuffer.AddComponent<FreeFalling>(contact.EntityB);
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

        var velocities = GetComponentDataFromEntity<Velocity>(false);
        var job = new ProcessCollisionJob
        {
            Contacts = DetectCollisionSystem.Contacts,
            Velocities = velocities,
            EntityCommandBuffer = ecb
        };

        JobHandle handle = JobHandle.CombineDependencies(inputDependencies, DetectCollisionSystem.Handle);
        handle = job.Schedule(handle);

        m_EntityCommandBufferSystem.AddJobHandleForProducer(handle);

        return handle;
    }
}