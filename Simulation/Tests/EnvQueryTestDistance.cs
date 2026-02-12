using System.Collections.Generic;
using Photon.Deterministic;

namespace Quantum.EQS
{
    public unsafe class EnvQueryTestDistance : EnvQueryTest
    {
        public EnvQueryContext distanceTo = EnvQueryContext.Querier;
        [DrawIf(nameof(testPurpose), (int)EnvTestPurpose.ScoreOnly, CompareOperator.NotEqual)]
        public FP distanceBeforeFilteredOut = 0;
        
        public override void RunTest(Frame frame, EnvQuery* envQuery, int currentTest, List<EnvQueryItem> envQueryItems)
        {
            if (IsActive)
            {
                for (var index = 0; index < envQueryItems.Count; index++)
                {
                    var item = envQueryItems[index];

                    var hasPoint = TryGetDistanceToPoint(frame, envQuery, ref item, out var pos);
                    if (!hasPoint)
                    {
                        item.TestResults[currentTest] = 0;
                        continue;
                    }
                    
                    item.TestResults[currentTest] =
                        FPVector3.Distance(pos, item.GetWorldPosition());
                }
            }
            else
            {
                for (var index = 0; index < envQueryItems.Count; index++)
                {
                    var item = envQueryItems[index];

                    item.TestResults[currentTest] = 0;
                }
            }
        }

        public override void FinalizeInjectionTest(Frame frame, EnvQueryCached* envQuery, int currentTest, ref int queryCurrentIndex)
        {
            var envQueryItems = frame.ResolveList(envQuery->envQueryItems);
            
            if (IsActive)
            {
                for (var index = 0; index < envQueryItems.Count; index++)
                {
                    var item = envQueryItems.GetPointer(index);
                    var testResults = frame.ResolveList(item->TestResults);
                    
                    var hasPoint = TryGetDistanceToPoint(frame, envQuery, item, out var pos);
                    if (!hasPoint)
                    {
                        testResults[currentTest] = 0;
                        continue;
                    }

                    var dist = FPVector3.Distance(pos, item->location);
                    
                    switch (testPurpose)
                    {
                        case EnvTestPurpose.FilterOnly:
                            if (dist >= distanceBeforeFilteredOut)
                            {
                                item->IsValid = false;
                                break;
                            }
                            break;
                        case EnvTestPurpose.ScoreOnly:
                            testResults[currentTest] = dist;
                            break;
                        case EnvTestPurpose.FilterAndScore:
                            if (dist >= distanceBeforeFilteredOut)
                            {
                                item->IsValid = false;
                                break;
                            }
                            testResults[currentTest] = dist;
                            break;
                    }
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

        private bool TryGetDistanceToPoint(Frame frame, EnvQuery* envQuery, ref EnvQueryItem currentItem, out FPVector3 position)
        {
            switch (distanceTo)
            {
                case EnvQueryContext.Item:
                    position = currentItem.GetWorldPosition();
                    return true;
                case EnvQueryContext.Querier:
                    var querierTransform = frame.Unsafe.GetPointer<Transform3D>(envQuery->querier);
                    position = querierTransform->Position;
                    return true;
            }

            position = FPVector3.Zero;
            return false;
        }
        
        private bool TryGetDistanceToPoint(Frame frame, EnvQueryCached* envQuery, EnvQueryItemCached* currentItem, out FPVector3 position)
        {
            switch (distanceTo)
            {
                case EnvQueryContext.Item:
                    position = currentItem->location;
                    return true;
                case EnvQueryContext.Querier:
                    var querierTransform = frame.Unsafe.GetPointer<Transform3D>(envQuery->querier);
                    position = querierTransform->Position;
                    return true;
            }

            position = FPVector3.Zero;
            return false;
        }
    }
}