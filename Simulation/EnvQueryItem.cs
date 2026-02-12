using Photon.Deterministic;

namespace Quantum.EQS
{
    public unsafe class EnvQueryItem
    {
        public FP Score;
        public bool IsValid;
        public FP[] TestResults;
        
        public FPVector3 centerOfItems;
        public FPVector3 location;
        public FPVector3 navLocation;
        
        public EnvQueryItem(int numOfTests, FPVector3 location, FPVector3 centerOfItems)
        {
            Score = 0;
            IsValid = true;
            TestResults = new FP[numOfTests];
            this.centerOfItems = centerOfItems;
            this.location = location;
            navLocation = location;
        }

        public FPVector3 GetWorldPosition()
        {
            return centerOfItems + navLocation;
        }

        public void UpdateTraceProjection(Frame frame, FP projectionUpDown, FP postProjectionVerticalOffset, LayerMask projectionLayerMask)
        {
            if (!IsValid) return;
            
            var worldPosition = centerOfItems + location;

            var downResult = frame.Physics3D.Raycast(worldPosition, FPVector3.Down, projectionUpDown, projectionLayerMask,
                QueryOptions.HitStatics | QueryOptions.HitDynamics | QueryOptions.ComputeDetailedInfo | QueryOptions.DetectOverlapsAtCastOrigin);
            var upResult = frame.Physics3D.Raycast(worldPosition, FPVector3.Up, projectionUpDown, projectionLayerMask,
                QueryOptions.HitStatics | QueryOptions.HitDynamics | QueryOptions.ComputeDetailedInfo | QueryOptions.DetectOverlapsAtCastOrigin);
            
            if (downResult.HasValue && !upResult.HasValue)
            {
                IsValid = true;
                navLocation = downResult.Value.Point - centerOfItems;
                navLocation += FPVector3.Up * postProjectionVerticalOffset;
                return;
            }else if (!downResult.HasValue && upResult.HasValue)
            {
                IsValid = true;
                navLocation = upResult.Value.Point - centerOfItems;
                navLocation += FPVector3.Up * postProjectionVerticalOffset;
                return;
            }else if (downResult.HasValue)
            {
                IsValid = true;
                if (FPVector3.DistanceSquared(downResult.Value.Point, location) <
                    FPVector3.DistanceSquared(upResult.Value.Point, location))
                {
                    navLocation = downResult.Value.Point - centerOfItems;
                }
                else
                {
                    navLocation = upResult.Value.Point - centerOfItems;
                }
                navLocation += FPVector3.Up * postProjectionVerticalOffset;
                return;
            }

            IsValid = false;
            navLocation = location;
        }
        
        public void UpdateNavMeshProjection(Frame frame, FP projectionRadius, FP postProjectionVerticalOffset)
        {
            if (!IsValid) return;

            var worldPosition = centerOfItems + location;
            var r = frame.Map.GetNavMesh("NavMeshData").FindClosestTriangle(frame, worldPosition, projectionRadius, NavMeshRegionMask.Default, out int triangle, out FPVector3 closestPosition);
            
            FP diff = (closestPosition.X - worldPosition.X) * (closestPosition.X - worldPosition.X)
                      + (closestPosition.Z - worldPosition.Z) * (closestPosition.Z - worldPosition.Z);
            
            if(r && diff < FP.SmallestNonZero)
            {
                IsValid = true;
                navLocation = closestPosition - centerOfItems;
                navLocation += FPVector3.Up * postProjectionVerticalOffset;
            }
            else
            {
                IsValid = false;
                navLocation = location;
            }
        }
    }
}