namespace Quantum.EQS
{
    public unsafe class EnvQuerySystem : SystemMainThreadFilter<EnvQueryInjectionSystem.Filter>, ISignalOnComponentAdded<EnvQueryCached>, ISignalOnComponentRemoved<EnvQueryCached>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public EnvQueryCached* envQuery;
        }
        
        public unsafe void OnAdded(Frame frame, EntityRef entity, EnvQueryCached* component)
        {
            component->envQueryItems = frame.AllocateList<EnvQueryItemCached>();
            component->broadphaseQueries = frame.AllocateList<PhysicsQueryRef>();
        }

        public unsafe void OnRemoved(Frame f, EntityRef entity, EnvQueryCached* component)
        {
            var eqi = f.ResolveList(component->envQueryItems);

            for (int i = 0; i < eqi.Count; i++)
            {
                f.TryFreeList(eqi[i].TestResults);
            }

            f.FreeList(component->envQueryItems);
            f.FreeList(component->broadphaseQueries);
            component->envQueryItems = default;
            component->broadphaseQueries = default;
        }

        public override void Update(Frame f, ref EnvQueryInjectionSystem.Filter filter)
        {
            if (filter.envQuery->stage == 0)
            {
                EnvQueryHelperBroadphase.StageTwo(f, filter.envQuery);
                filter.envQuery->stage = 1;
            }else if (filter.envQuery->stage == 1)
            {
                EnvQueryHelperBroadphase.StageFour(f, filter.envQuery);
                filter.envQuery->stage = 3;
            }else if (filter.envQuery->stage == 3)
            {
                EnvQueryHelperBroadphase.DrawItems(f, filter.envQuery);
            }
        }
    }
}
