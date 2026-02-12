using System.Collections.Generic;
using System.Linq;
using Quantum.Profiling;

namespace Quantum.EQS
{
    public static unsafe class EnvQueryHelper
    {
        public static bool TryExecuteQueryImmediate(Frame frame, EnvQuery* envQuery, out List<EnvQueryItem> itemsSorted)
        {
            itemsSorted = null;
            if (!frame.TryFindAsset(envQuery->queryAsset, out var qAsset)) return false;

            HostProfiler.Start("EQS Immediate");
            {
                var queryItems = qAsset.generator.GenerateItems(frame, envQuery, qAsset.tests.Count, envQuery->CenterOfItems);
                if (queryItems == null) return false;

                // Reset Scores
                foreach (var t in queryItems)
                {
                    t.Score = 0;
                }

                HostProfiler.Start("Tests");
                // Run Tests
                for (int currentTest = 0; currentTest < qAsset.tests.Count; currentTest++)
                {
                    qAsset.tests[currentTest].RunTest(frame, envQuery, currentTest, queryItems);
                    qAsset.tests[currentTest].NormalizeItemScores(frame, currentTest, queryItems);
                }
                HostProfiler.End();

                // Finalize scores
                NormalizeScore(frame, envQuery, queryItems);
                itemsSorted = queryItems.OrderByDescending(x => x.Score).ToList();
                DrawItems(frame, envQuery, itemsSorted);
            }
            HostProfiler.End();
            return true;
        }
        
        private static void NormalizeScore(Frame frame, EnvQuery* envQuery, List<EnvQueryItem> queryItemsList)
        {
            if (queryItemsList.Count <= 0) return;

            var maxScore = queryItemsList[0].Score;
            var minScore = queryItemsList[0].Score;

            for (var index = 0; index < queryItemsList.Count; index++)
            {
                var item = queryItemsList[index];
                if (!item.IsValid) continue;
                if (item.Score > maxScore)
                {
                    maxScore = item.Score;
                }

                if (item.Score < minScore)
                {
                    minScore = item.Score;
                }
            }

            if(maxScore != minScore)
            {
                for (var index = 0; index < queryItemsList.Count; index++)
                {
                    if(!queryItemsList[index].IsValid) continue;
                    queryItemsList[index].Score = (queryItemsList[index].Score - minScore) / (maxScore - minScore);
                }
            }
        }
        
        public static void DrawItems(Frame frame, EnvQuery* envQuery, List<EnvQueryItem> queryItemsList)
        {
            /*
            foreach (var item in queryItemsList)
            {
                var c = Color.HSVToRGB((item.Score/2).AsFloat, 1, 1);
                using (Drawing.Draw.WithDuration(4))
                {
                    if (item.IsValid)
                    {
                        using (Drawing.Draw.WithColor(c))
                        {
                            var wp = item.GetWorldPosition();
                            Vector3 p = new Vector3(wp.X.AsFloat, wp.Y.AsFloat, wp.Z.AsFloat);
                            Drawing.Draw.WireSphere(p, 0.25f);
                            Drawing.Draw.Label2D((p + Vector3.up * 0.2f), item.Score.AsFloat.ToString("F2"),
                                20, Color.white);
                        }
                    }
                    else
                    {
                        using (Drawing.Draw.WithColor(Color.blue))
                        {
                            var wp = item.GetWorldPosition();
                            Vector3 p = new Vector3(wp.X.AsFloat, wp.Y.AsFloat, wp.Z.AsFloat);
                            Drawing.Draw.WireSphere(p, 0.2f);
                        }
                    }
                }
            }*/
        }
    }
}
