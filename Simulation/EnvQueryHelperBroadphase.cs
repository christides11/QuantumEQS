using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.EQS
{
    public static unsafe class EnvQueryHelperBroadphase
    {
        public static bool StageOne(Frame frame, EnvQueryCached* envQuery)
        {
            if (!frame.TryFindAsset(envQuery->queryAsset, out var eqsAsset)) return false;
            
            // Initialize lists
            var envQueryItems = frame.ResolveList(envQuery->envQueryItems);
            var broadphaseQueries = frame.ResolveList(envQuery->broadphaseQueries);
            CleanupEnvQueryItems(frame, envQuery, envQueryItems);
            envQueryItems.Clear();
            broadphaseQueries.Clear();
            
            // Request Create Points
            var broadphaseQueryCount = eqsAsset.generator.CreateBroadphaseQueryForItems(frame, envQuery, eqsAsset.tests.Count, envQuery->CenterOfItems);
            return true;
        }

        public static bool StageTwo(Frame frame, EnvQueryCached* envQuery)
        {
            if (!frame.TryFindAsset(envQuery->queryAsset, out var eqsAsset)) return false;

            // Initialize test lists
            var broadphaseQueries = frame.ResolveList(envQuery->broadphaseQueries);
            var envQueryItems = frame.ResolveList(envQuery->envQueryItems);
            for (int i = 0; i < envQueryItems.Count; i++)
            {
                envQueryItems.GetPointer(i)->TestResults = frame.AllocateList<FP>(eqsAsset.tests.Count);
                var testResults = frame.ResolveList(envQueryItems.GetPointer(i)->TestResults);

                for (int w = 0; w < eqsAsset.tests.Count; w++)
                {
                    testResults.Add(0);
                }
            }
            
            // Finalize points
            eqsAsset.generator.GenerateItemsFromBroadphaseQueries(frame, envQuery, eqsAsset.tests.Count, envQuery->CenterOfItems);
            broadphaseQueries.Clear();
            return true;
        }

        public static bool StageThree(Frame frame, EnvQueryCached* envQuery)
        {
            if (!frame.TryFindAsset(envQuery->queryAsset, out var eqsAsset)) return false;
            
            // Start tests
            int bqIndex = 0;
            for (int currentTest = 0; currentTest < eqsAsset.tests.Count; currentTest++)
            {
                eqsAsset.tests[currentTest].StartInjectionTest(frame, envQuery, currentTest, ref bqIndex);
            }
            
            return true;
        }

        public static bool StageFour(Frame frame, EnvQueryCached* envQuery)
        {
            if (!frame.TryFindAsset(envQuery->queryAsset, out var eqsAsset)) return false;
            
            var broadphaseQueries = frame.ResolveList(envQuery->broadphaseQueries);
            var envQueryItems = frame.ResolveList(envQuery->envQueryItems);
            
            // Finalize tests
            int bqIndex = 0;
            for (int currentTest = 0; currentTest < eqsAsset.tests.Count; currentTest++)
            {
                eqsAsset.tests[currentTest].FinalizeInjectionTest(frame, envQuery, currentTest, ref bqIndex);
                eqsAsset.tests[currentTest].NormalizeItemScores(frame, currentTest, envQuery);
            }
            broadphaseQueries.Clear();
            
            // Score
            //NormalizeScore(frame, envQuery);
            SortItemsByScore(frame, envQuery);
            return true;
        }

        public static bool TryGetBestPosition(Frame frame, EnvQueryCached* envQuery, out FPVector3 position)
        {
            position = default;

            var items = frame.ResolveList(envQuery->envQueryItems);

            if (items.Count == 0) return false;
            else if (items.Count == 1)
            {
                position = items.GetPointer(0)->location;
                return true;
            }

            FP bestScore = items.GetPointer(0)->Score;
            int bestScoreIndex = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (items.GetPointer(i)->Score <= bestScore) continue;
                bestScore = items.GetPointer(i)->Score;
                bestScoreIndex = i;
            }

            position = items.GetPointer(bestScoreIndex)->location;
            return true;
        }

        public static void DrawItems(Frame frame, EnvQueryCached* envQuery)
        {
            /*
            var envQueryItems = frame.ResolveList(envQuery->envQueryItems);

            for (var index = 0; index < envQueryItems.Count; index++)
            {
                var item = envQueryItems.GetPointer(index);
                var c = Color.HSVToRGB((item->Score / 2).AsFloat, 1, 1);
                using (Drawing.Draw.WithDuration(1.5f/60.0f))
                {
                    if (item->IsValid)
                    {
                        using (Drawing.Draw.WithColor(c))
                        {
                            var wp = item->location;
                            Vector3 p = new Vector3(wp.X.AsFloat, wp.Y.AsFloat, wp.Z.AsFloat);
                            Drawing.Draw.WireSphere(p, 0.25f);
                            Drawing.Draw.Label2D((p + Vector3.up * 0.2f), item->Score.AsFloat.ToString("F2"),
                                30, Color.white);
                        }
                    }
                    else
                    {
                        using (Drawing.Draw.WithColor(Color.blue))
                        {
                            var wp = item->location;
                            Vector3 p = new Vector3(wp.X.AsFloat, wp.Y.AsFloat, wp.Z.AsFloat);
                            Drawing.Draw.WireSphere(p, 0.2f);
                            Drawing.Draw.Label2D((p + Vector3.up * 0.2f), "INVALID",
                                15, Color.white);
                        }
                    }
                }
            }*/
        }
        
        private static void NormalizeScore(Frame frame, EnvQueryCached* envQuery)
        {
            var queryItemsList = frame.ResolveList(envQuery->envQueryItems);

            if(queryItemsList.Count < 1)
            {
                return;
            }

            var maxScore = queryItemsList[0].Score;
            var minScore = queryItemsList[0].Score;

            for (var index = 0; index < queryItemsList.Count; index++)
            {
                var item = queryItemsList.GetPointer(index);
                if (!item->IsValid) continue;
                if (item->Score > maxScore)
                {
                    maxScore = item->Score;
                }

                if (item->Score < minScore)
                {
                    minScore = item->Score;
                }
            }

            if(maxScore != minScore)
            {
                for (var index = 0; index < queryItemsList.Count; index++)
                {
                    if (!queryItemsList.GetPointer(index)->IsValid) continue;
                    queryItemsList.GetPointer(index)->Score = (queryItemsList[index].Score - minScore) / (maxScore - minScore);
                }
            }
        }
        
        private static void SortItemsByScore(Frame frame, EnvQueryCached* envQuery)
        {
            
        }
        
        private static void CleanupEnvQueryItems(Frame frame, EnvQueryCached* envQuery, QList<EnvQueryItemCached> envQueryItems)
        {
            for (int i = 0; i < envQueryItems.Count; i++)
            {
                frame.TryFreeList(envQueryItems.GetPointer(i)->TestResults);
            }
        }
    }
}
