using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.EQS
{
    public unsafe class EnvQueryTestDot : EnvQueryTest
    {
        public enum DirMode
        {
            Rotation,
            TwoPoints
        }

        public enum RotMode
        {
            Forward,
            Backward,
            Right,
            Left
        }

        public enum TestMode
        {
            Dot_3d,
            Dot_2d
        }

        [System.Serializable]
        public struct LineDefinition
        {
            public DirMode mode;
            [DrawIf(nameof(mode), (int)DirMode.Rotation, CompareOperator.Equal)]
            public EnvQueryContext rotation;

            [DrawIf(nameof(mode), (int)DirMode.Rotation, CompareOperator.Equal)]
            public RotMode rotationMode;

            [DrawIf(nameof(mode), (int)DirMode.TwoPoints, CompareOperator.Equal)]
            public EnvQueryContext LineFrom;
            [DrawIf(nameof(mode), (int)DirMode.TwoPoints, CompareOperator.Equal)]
            public EnvQueryContext LineTo;

        }

        public LineDefinition lineA;
        public LineDefinition lineB;
        
        public TestMode testMode;
        public bool AbsoluteValue;

        public override void RunTest(Frame frame, EnvQuery* envQuery, int currentTest, List<EnvQueryItem> envQueryItems)
        {
            if (IsActive)
            {
                for (var index = 0; index < envQueryItems.Count; index++)
                {
                    var item = envQueryItems[index];
                    
                    bool hasLineA = CalculateLine(frame, lineA, envQuery, ref item, out FPVector3 aLine);
                    if (!hasLineA)
                    {
                        item.TestResults[currentTest] = 0;
                        continue;
                    }
                    bool hasLineB = CalculateLine(frame, lineA, envQuery, ref item, out FPVector3 bLine);
                    if (!hasLineB)
                    {
                        item.TestResults[currentTest] = 0;
                        continue;
                    }

                    FP dotValue = FPVector3.Dot(aLine, bLine);
                    if (AbsoluteValue) dotValue = FPMath.Abs(dotValue);
                    
                    item.TestResults[currentTest] = dotValue;                    
                }
                
            }else
            {
                for (var index = 0; index < envQueryItems.Count; index++)
                {
                    var item = envQueryItems[index];
                    item.TestResults[currentTest] = 0;
                }
            }
        }

        private bool CalculateLine(Frame frame, LineDefinition lineDefinition, EnvQuery* envQuery, ref EnvQueryItem item, out FPVector3 vector)
        {
            switch (lineDefinition.mode)
            {
                case DirMode.Rotation:
                    Transform3D trans = GetTransform(frame, lineDefinition.rotation, envQuery, ref item);
                    
                    vector = lineDefinition.rotationMode switch
                    {
                        RotMode.Forward => trans.Forward,
                        RotMode.Backward => trans.Back,
                        RotMode.Right => trans.Right,
                        RotMode.Left => trans.Left,
                        _ => default
                    };
                    return true;
                case DirMode.TwoPoints:
                    if (!TryGetPosition(frame, lineDefinition.LineFrom, envQuery, ref item, out var vFrom)
                        || !TryGetPosition(frame, lineDefinition.LineFrom, envQuery, ref item, out var vTo))
                    {
                        vector = default;
                        return false;
                    }

                    vector = (vTo - vFrom).Normalized;
                    return true;
            }
            
            
            vector = default;
            return false;
        }

        private Transform3D GetTransform(Frame frame, EnvQueryContext lineDefinitionRotation, EnvQuery* envQuery, ref EnvQueryItem item)
        {
            switch (lineDefinitionRotation)
            {
                case EnvQueryContext.Querier:
                    return frame.Get<Transform3D>(envQuery->querier);
                case EnvQueryContext.Item:
                    break;
                case EnvQueryContext.Target:
                    return envQuery->target;
            }

            return default;
        }

        private bool TryGetPosition(Frame frame, EnvQueryContext context, EnvQuery* envQuery, ref EnvQueryItem item, out FPVector3 position)
        {
            switch (context)
            {
                case EnvQueryContext.Querier:
                    position = frame.Unsafe.GetPointer<Transform3D>(envQuery->querier)->Position;
                    return true;
                case EnvQueryContext.Item:
                    position = item.GetWorldPosition();
                    return true;
                case EnvQueryContext.Target:
                    position = envQuery->target.Position;
                    return true;
            }
            
            position = default;
            return false;
        }
    }
}