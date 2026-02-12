using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct EnvQueryItemCached
    {
        public EnvQueryItemCached(FPVector3 location)
        {
            Score = 0;
            IsValid = true;
            TestResults = default;
            this.location = location;
        }
        
        public void CalculateTraceProjection(Frame frame, EnvQueryCached* envQuery, FP projectionUpDown, LayerMask projectionLayerMask)
        {
            var broadphaseQueryList = frame.ResolveList(envQuery->broadphaseQueries);
            
            if (!IsValid)
            {
                return;
            }
            
            var downResult = frame.Physics3D.AddRaycastQuery(location, FPVector3.Down, projectionUpDown, true, projectionLayerMask,
                QueryOptions.HitStatics | QueryOptions.HitDynamics | QueryOptions.ComputeDetailedInfo | QueryOptions.DetectOverlapsAtCastOrigin);
            var upResult = frame.Physics3D.AddRaycastQuery(location, FPVector3.Up, projectionUpDown, true, projectionLayerMask,
                QueryOptions.HitStatics | QueryOptions.HitDynamics | QueryOptions.ComputeDetailedInfo | QueryOptions.DetectOverlapsAtCastOrigin);
            
            broadphaseQueryList.Add(downResult);
            broadphaseQueryList.Add(upResult);
        }

        public void FinalizeTraceProjection(Frame frame, EnvQueryCached* envQuery, FP postProjectionVerticalOffset, ref int queryStartIndex)
        {
            if (!IsValid) return;
            
            var broadphaseQueryList = frame.ResolveList(envQuery->broadphaseQueries);

            var downResult = frame.Physics3D.GetQueryHits(broadphaseQueryList[queryStartIndex + 0]);
            var upResult = frame.Physics3D.GetQueryHits(broadphaseQueryList[queryStartIndex + 1]);
            
            queryStartIndex += 2;
            
            if (downResult.Count > 0 && upResult.Count == 0)
            {
                location = downResult[0].Point;
                location += FPVector3.Up * postProjectionVerticalOffset;
                return;
            }else if (downResult.Count == 0 && upResult.Count > 0)
            {
                location = upResult[0].Point;
                location += FPVector3.Up * postProjectionVerticalOffset;
                return;
            }else if (downResult.Count > 0)
            {
                if (FPVector3.DistanceSquared(downResult[0].Point, location) >
                    FPVector3.DistanceSquared(upResult[0].Point, location))
                {
                    location = upResult[0].Point;
                }
                else
                {
                    location = downResult[0].Point;
                }
                
                location += FPVector3.Up * postProjectionVerticalOffset;
                return;
            }

            IsValid = false;
        }
    }
}