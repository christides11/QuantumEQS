using System.Collections.Generic;
using Photon.Deterministic;

namespace Quantum.EQS
{
    [System.Serializable]
    public unsafe class EnvQueryTest : AssetObject
    {
        public enum EnvQueryTestScoringEquation
        {
            Linear,
            InverseLinear,
            Square,
            HalfSine,
            InverseHalfSine,
            HalfSineSquared,
            InverseHalfSineSquared,
            SigmoidLike,
            InverseSigmoidLike
        }

        public EnvTestPurpose testPurpose = EnvTestPurpose.ScoreOnly;
        public bool IsActive = true;
        public FP ScaleFactor = 1;
        public EnvQueryTestScoringEquation ScoringEquation = EnvQueryTestScoringEquation.Linear;

        public virtual void RunTest(Frame frame, EnvQuery* envQuery, int currentTest, List<EnvQueryItem> envQueryItems)
        {
        }

        public virtual void StartInjectionTest(Frame frame, EnvQueryCached* envQuery, int currentTest, ref int queryCurrentIndex)
        {
            
        }

        public virtual void FinalizeInjectionTest(Frame frame, EnvQueryCached* envQuery, int currentTest, ref int queryCurrentIndex)
        {
            
        }

        public void NormalizeItemScores(Frame frame, int currentTest, List<EnvQueryItem> envQueryItems)
        {
            if (envQueryItems.Count < 1)
            {
                return;
            }
            
            FP maxValue = envQueryItems[0].TestResults[currentTest];
            FP minValue = envQueryItems[0].TestResults[currentTest];

            foreach (EnvQueryItem item in envQueryItems)
            {
                if (item.IsValid)
                {
                    FP value = item.TestResults[currentTest];
                    if (value > maxValue)
                    {
                        maxValue = value;
                    }

                    if (value < minValue)
                    {
                        minValue = value;
                    }
                }
            }

            if (maxValue != minValue)
            {
                for (var index = 0; index < envQueryItems.Count; index++)
                {
                    var item = envQueryItems[index];
                    if (!item.IsValid)
                    {
                        envQueryItems[index].TestResults[currentTest] = 0;
                        continue;
                    }
                    FP weightedScore = 0;
                    FP normalizedScore = (envQueryItems[index].TestResults[currentTest] - minValue) / (maxValue - minValue);

                    switch (ScoringEquation)
                    {
                        case EnvQueryTestScoringEquation.Linear:
                            weightedScore = ScaleFactor * normalizedScore;
                            break;
                        case EnvQueryTestScoringEquation.InverseLinear:
                            weightedScore = ScaleFactor * ((FP)1 - normalizedScore);
                            break;
                        case EnvQueryTestScoringEquation.Square:
                            weightedScore = ScaleFactor * (normalizedScore * normalizedScore);
                            break;
                        case EnvQueryTestScoringEquation.HalfSine:
                            weightedScore = ScaleFactor * FPMath.Sin(FP.Pi * normalizedScore);
                            break;
                        case EnvQueryTestScoringEquation.InverseHalfSine:
                            weightedScore = ScaleFactor * -FPMath.Sin(FP.Pi * normalizedScore);
                            break;
                        case EnvQueryTestScoringEquation.HalfSineSquared:
                            weightedScore = ScaleFactor * FPMath.Sin(FP.Pi * normalizedScore) *
                                            FPMath.Sin(FP.Pi * normalizedScore);
                            break;
                        case EnvQueryTestScoringEquation.InverseHalfSineSquared:
                            weightedScore = ScaleFactor * -(FPMath.Sin(FP.Pi * normalizedScore) *
                                                            FPMath.Sin(FP.Pi * normalizedScore));
                            break;
                        /*
                        case EnvQueryTestScoringEquation.SigmoidLike:
                            weightedScore = ScaleFactor *
                                            (((FP)Math.Tanh((FP)4 * (normalizedScore - FP._0_50)) + (FP)1) / (FP)2);
                            break;
                        case EnvQueryTestScoringEquation.InverseSigmoidLike:
                            weightedScore = ScaleFactor *
                                            ((FP)1- (((FP)Math.Tanh((FP)4 * (normalizedScore - FP._0_50)) + (FP)1) /
                                                     (FP)2));
                            break;*/
                        default:
                            break;
                    }

                    envQueryItems[index].Score += weightedScore;
                }
            }
        }
        
        public void NormalizeItemScores(Frame frame, int currentTest, EnvQueryCached* envQuery)
        {
            var envQueryItems = frame.ResolveList(envQuery->envQueryItems);
            var testResultsFirst = frame.ResolveList(envQueryItems.GetPointer(0)->TestResults);
            
            if (envQueryItems.Count < 1)
            {
                return;
            }
            
            FP maxValue = testResultsFirst[currentTest];
            FP minValue = testResultsFirst[currentTest];

            for (var index = 0; index < envQueryItems.Count; index++)
            {
                var item = envQueryItems.GetPointer(index);
                var testResults = frame.ResolveList(item->TestResults);
                
                if (item->IsValid)
                {
                    FP value = testResults[currentTest];
                    if (value > maxValue)
                    {
                        maxValue = value;
                    }

                    if (value < minValue)
                    {
                        minValue = value;
                    }
                }
            }

            if (maxValue != minValue)
            {
                for (var index = 0; index < envQueryItems.Count; index++)
                {
                    var item = envQueryItems.GetPointer(index);
                    var testResults = frame.ResolveList(item->TestResults);
                    
                    if (!item->IsValid)
                    {
                        testResults[currentTest] = 0;
                        continue;
                    }
                    FP weightedScore = 0;
                    FP normalizedScore = (testResults[currentTest] - minValue) / (maxValue - minValue);

                    switch (ScoringEquation)
                    {
                        case EnvQueryTestScoringEquation.Linear:
                            weightedScore = ScaleFactor * normalizedScore;
                            break;
                        case EnvQueryTestScoringEquation.InverseLinear:
                            weightedScore = ScaleFactor * ((FP)1 - normalizedScore);
                            break;
                        case EnvQueryTestScoringEquation.Square:
                            weightedScore = ScaleFactor * (normalizedScore * normalizedScore);
                            break;
                        case EnvQueryTestScoringEquation.HalfSine:
                            weightedScore = ScaleFactor * FPMath.Sin(FP.Pi * normalizedScore);
                            break;
                        case EnvQueryTestScoringEquation.InverseHalfSine:
                            weightedScore = ScaleFactor * -FPMath.Sin(FP.Pi * normalizedScore);
                            break;
                        case EnvQueryTestScoringEquation.HalfSineSquared:
                            weightedScore = ScaleFactor * FPMath.Sin(FP.Pi * normalizedScore) *
                                            FPMath.Sin(FP.Pi * normalizedScore);
                            break;
                        case EnvQueryTestScoringEquation.InverseHalfSineSquared:
                            weightedScore = ScaleFactor * -(FPMath.Sin(FP.Pi * normalizedScore) *
                                                            FPMath.Sin(FP.Pi * normalizedScore));
                            break;
                        /*
                        case EnvQueryTestScoringEquation.SigmoidLike:
                            weightedScore = ScaleFactor *
                                            (((FP)Math.Tanh((FP)4 * (normalizedScore - FP._0_50)) + (FP)1) / (FP)2);
                            break;
                        case EnvQueryTestScoringEquation.InverseSigmoidLike:
                            weightedScore = ScaleFactor *
                                            ((FP)1- (((FP)Math.Tanh((FP)4 * (normalizedScore - FP._0_50)) + (FP)1) /
                                                     (FP)2));
                            break;*/
                        default:
                            break;
                    }

                    item->Score += weightedScore;
                }
            }
        }
    }
}