using System.Collections.Generic;
using Photon.Deterministic;

namespace Quantum.EQS
{
    [System.Serializable]
    public unsafe class EnvQueryGenerator : AssetObject
    {
        public enum ProjectionType
        {
            None,
            Navigation,
            Trace
        }

        public ProjectionType projectionType = ProjectionType.Navigation;
        public LayerMask projectionLayerMask;
        
        public virtual List<EnvQueryItem> GenerateItems(Frame frame, EnvQuery* envQuery, int numTests, Transform3D centerOfItems)
        {
            return null;
        }

        public virtual int CreateBroadphaseQueryForItems(Frame frame, EnvQueryCached* envQuery, int numTests, Transform3D centerOfItems)
        {
            return 0;
        }

        public virtual void GenerateItemsFromBroadphaseQueries(Frame frame, EnvQueryCached* envQuery, int numTests, Transform3D centerOfItems)
        {
            
        }
        
        protected FPVector3 GetPosition(Frame frame, EnvQueryContext context, EnvQuery* envQuery)
        {
            switch (context)
            {
                case EnvQueryContext.Querier:
                    return frame.Unsafe.GetPointer<Transform3D>(envQuery->querier)->Position;
                case EnvQueryContext.Target:
                    return envQuery->target.Position;
            }
            return default;
        }
        
        protected FPVector3 GetPosition(Frame frame, EnvQueryContext context, EnvQueryCached* envQuery)
        {
            switch (context)
            {
                case EnvQueryContext.Querier:
                    return frame.Unsafe.GetPointer<Transform3D>(envQuery->querier)->Position;
                case EnvQueryContext.Target:
                    return envQuery->target.Position;
            }
            return default;
        }
    }
}
