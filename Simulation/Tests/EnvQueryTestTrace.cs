using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum.EQS
{
    public unsafe class EnvQueryTestTrace : EnvQueryTest
    {
        public enum TraceType
        {
            Visible,
            Invisible
        }

        public enum TraceHitType
        {
            Hit,
            NoHit
        }

        public enum ShapeType
        {
            Line,
            Sphere,
            Box,
            Capsule
        }


        public LayerMask layerMask;
        public QueryOptions queryOptions = QueryOptions.HitAll;
        public TraceType traceType = TraceType.Visible;
        public ShapeType traceShape;
        public TraceHitType hitType;

        public EnvQueryContext traceFrom;
        public EnvQueryContext traceTo;

        public FP fromHeightOffset;
        public FP toHeightOffset;

        public override void RunTest(Frame frame, EnvQuery* envQuery, int currentTest, List<EnvQueryItem> envQueryItems)
        {
            if (IsActive)
            {
                for (var index = 0; index < envQueryItems.Count; index++)
                {
                    var item = envQueryItems[index];

                    var fromPos = GetPosition(frame, traceFrom, envQuery, ref item) + FPVector3.Up * fromHeightOffset;
                    var toPos = GetPosition(frame, traceTo, envQuery, ref item) + FPVector3.Up * toHeightOffset;

                    FPVector3 dir = (toPos - fromPos);
                    var raycastHit = frame.Physics3D.Raycast(fromPos, dir.Normalized, dir.Magnitude, layerMask, queryOptions);

                    var raycastHitTarget = raycastHit.HasValue && raycastHit.Value.IsDynamic &&
                                           raycastHit.Value.Entity == GetEntity(frame, traceTo, envQuery);
                    

                    if (raycastHit.HasValue && raycastHit.Value.IsDynamic &&
                        raycastHit.Value.Entity == GetEntity(frame, traceTo, envQuery))
                    {
                        if(testPurpose == EnvTestPurpose.FilterOnly) break;
                        
                        if (traceType == TraceType.Visible)
                        {
                            item.TestResults[currentTest] = (FP)1;
                        }else if (traceType == TraceType.Invisible)
                        {
                            item.TestResults[currentTest] = -(FP)1;
                        }
                    }
                    else
                    {
                        item.TestResults[currentTest] = 0;
                        switch (testPurpose)
                        {
                            case EnvTestPurpose.FilterOnly:
                            case EnvTestPurpose.FilterAndScore:
                                item.IsValid = false;
                                break;
                        }
                    }
                }
            }else
            {
                foreach (var item in envQueryItems)
                {
                    item.TestResults[currentTest] = 0;
                }
            }
        }

        public override void StartInjectionTest(Frame frame, EnvQueryCached* envQuery, int currentTest, ref int queryCurrentIndex)
        {
            var envQueryItems = frame.ResolveList(envQuery->envQueryItems);
            var broadphaseQueries = frame.ResolveList(envQuery->broadphaseQueries);
            
            if (IsActive)
            {
                for (var index = 0; index < envQueryItems.Count; index++)
                {
                    var item = envQueryItems.GetPointer(index);

                    var fromPos = GetPosition(frame, traceFrom, envQuery, item) + FPVector3.Up * fromHeightOffset;
                    var toPos = GetPosition(frame, traceTo, envQuery, item) + FPVector3.Up * toHeightOffset;

                    FPVector3 dir = (toPos - fromPos);
                    var raycastQueryRef = frame.Physics3D.AddRaycastQuery(fromPos, dir.Normalized, dir.Magnitude, false, layerMask, queryOptions);
                    broadphaseQueries.Add(raycastQueryRef);

                    queryCurrentIndex += 1;
                }
            }
            else
            {
                for (var index = 0; index < envQueryItems.Count; index++)
                {
                    var item = envQueryItems.GetPointer(index);
                    var testResults = frame.ResolveList(item->TestResults);
                    testResults[currentTest] = 0;
                }
            }
        }

        public override void FinalizeInjectionTest(Frame frame, EnvQueryCached* envQuery, int currentTest, ref int queryCurrentIndex)
        {
            var envQueryItems = frame.ResolveList(envQuery->envQueryItems);
            var broadphaseQueries = frame.ResolveList(envQuery->broadphaseQueries);
            
            if (!IsActive) return;
            
            for (var index = 0; index < envQueryItems.Count; index++)
            {
                var item = envQueryItems.GetPointer(index);
                var testResults = frame.ResolveList(item->TestResults);
                var raycastResult = frame.Physics3D.GetQueryHits(broadphaseQueries[queryCurrentIndex]);
                
                var raycastHitTarget = raycastResult.Count == 1 && raycastResult[0].IsDynamic &&
                                       raycastResult[0].Entity == GetEntity(frame, traceTo, envQuery);
                
                if (raycastHitTarget)
                {
                    if(testPurpose == EnvTestPurpose.FilterOnly) break;
                        
                    if (traceType == TraceType.Visible)
                    {
                        testResults[currentTest] = (FP)1;
                    }else if (traceType == TraceType.Invisible)
                    {
                        testResults[currentTest] = -(FP)1;
                    }
                }
                else
                {
                    testResults[currentTest] = 0;
                    switch (testPurpose)
                    {
                        case EnvTestPurpose.FilterOnly:
                        case EnvTestPurpose.FilterAndScore:
                            item->IsValid = false;
                            break;
                    }
                }
                
                queryCurrentIndex += 1;
            }
        }

        private FPVector3 GetPosition(Frame frame, EnvQueryContext context, EnvQuery* envQuery, ref EnvQueryItem item)
        {
            switch (context)
            {
                case EnvQueryContext.Querier:
                    return frame.Unsafe.GetPointer<Transform3D>(envQuery->querier)->Position;
                case EnvQueryContext.Item:
                    return item.GetWorldPosition();
                case EnvQueryContext.Target:
                    return envQuery->target.Position;
            }
            return default;
        }

        private EntityRef GetEntity(Frame frame, EnvQueryContext context, EnvQuery* envQuery)
        {
            switch (context)
            {
                case EnvQueryContext.Querier:
                    return envQuery->querier;
                case EnvQueryContext.Target:
                    return envQuery->targetEntity;
            }
            
            return default;
        }
        
        private FPVector3 GetPosition(Frame frame, EnvQueryContext context, EnvQueryCached* envQuery, EnvQueryItemCached* item)
        {
            switch (context)
            {
                case EnvQueryContext.Querier:
                    return frame.Unsafe.GetPointer<Transform3D>(envQuery->querier)->Position;
                case EnvQueryContext.Item:
                    return item->location;
                case EnvQueryContext.Target:
                    return envQuery->target.Position;
            }
            return default;
        }

        private EntityRef GetEntity(Frame frame, EnvQueryContext context, EnvQueryCached* envQuery)
        {
            switch (context)
            {
                case EnvQueryContext.Querier:
                    return envQuery->querier;
                case EnvQueryContext.Target:
                    return envQuery->targetEntity;
            }
            
            return default;
        }
    }
}