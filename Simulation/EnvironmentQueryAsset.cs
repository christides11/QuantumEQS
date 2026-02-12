using System.Collections.Generic;

namespace Quantum.EQS
{
    public unsafe class EnvironmentQueryAsset : AssetObject
    {
        public EnvQueryGenerator generator;
        public List<EnvQueryTest> tests;
        
        public EnvQuery CreateQuery(Frame frame, EntityRef querier)
        {
            if(!querier.IsValid 
               || !frame.TryGet<Transform3D>(querier, out var querierTransform)) return default;
            
            return new EnvQuery()
            {
                queryAsset = this,
                querier = querier,
                CenterOfItems = querierTransform
            };
        }
        
        public bool TryCreateCachedQuery(Frame frame, EntityRef querier, out EntityRef envQuery)
        {
            envQuery = default;

            if (!querier.IsValid || !frame.TryGet<Transform3D>(querier, out var querierTransform)) return false;

            var entityRef = frame.Create();

            frame.Add(entityRef, new EnvQueryCached()
            {
                queryAsset = this,
                querier = querier,
                CenterOfItems = querierTransform
            });

            envQuery = entityRef;
            return true;
        }
    }
}
