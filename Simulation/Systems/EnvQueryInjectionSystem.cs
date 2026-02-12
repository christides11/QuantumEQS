using UnityEngine;

namespace Quantum.EQS
{
    public unsafe class EnvQueryInjectionSystem : SystemMainThreadFilter<EnvQueryInjectionSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public EnvQueryCached* envQuery;
        }


        public override void Update(Frame f, ref Filter filter)
        {
            if (filter.envQuery->stage == 0)
            {
                EnvQueryHelperBroadphase.StageOne(f, filter.envQuery);
            }else if (filter.envQuery->stage == 1)
            {
                EnvQueryHelperBroadphase.StageThree(f, filter.envQuery);
            }
        }
    }
}
