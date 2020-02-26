
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateAfter(typeof(DetectCollisionSystem))]
public class ProcessCollisionSystem : JobComponentSystem
{
    [BurstCompile]
    struct ProcessCollisionJob : IJob
    {
        public NativeQueue<ContactPoint> Contacts;
        public ComponentDataFromEntity<Velocity> Velocities;

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
                float massB = 1.0f;
                float restitution = 0.9f;

                float mult = restitution / (massA + massB);
                float massDiff = massB - massA;

                velBComp.Value = mult * (2.0f * massA * velA + massDiff * velB);
                velAComp.Value = mult * (2.0f * massB * velB - massDiff * velA);

                Velocities[contact.EntityA] = velAComp;
                Velocities[contact.EntityB] = velBComp;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var velocities = GetComponentDataFromEntity<Velocity>(false);
        var job = new ProcessCollisionJob
        {
            Contacts = DetectCollisionSystem.Contacts,
            Velocities = velocities
        };

        return job.Schedule(inputDependencies);
    }
}