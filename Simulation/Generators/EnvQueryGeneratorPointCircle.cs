using System.Collections.Generic;
using Photon.Deterministic;

namespace Quantum.EQS
{
    [System.Serializable]
    public unsafe class EnvQueryGeneratorPointCircle : EnvQueryGenerator
    {
        public EnvQueryContext generateAround = EnvQueryContext.Querier;
        public FP radius = 4;
        public FP spaceBetween = 1;

        public override List<EnvQueryItem> GenerateItems(Frame frame, EnvQuery* envQuery, int numTests, Transform3D centerOfItems)
        {
            return GenerateItems(frame, envQuery, numTests, centerOfItems, radius, spaceBetween);
        }

        public List<EnvQueryItem> GenerateItems(Frame frame, EnvQuery* envQuery, int numTests, Transform3D centerOfItems, FP radius, FP spaceBetween)
        {
            return null;
            /*
            var origin = GetPosition(frame, generateAround, envQuery);

            List<EnvQueryItem> items = new List<EnvQueryItem>();

            var position = FPVector3.Zero;
            items.Add(new EnvQueryItem(frame, numTests, position, origin));

            int numOfStepsForRadialDirection = (int)FPMath.Ceiling(radius / spaceBetween);

            for(int ri = 1; ri <= numOfStepsForRadialDirection; ri++)
            {
                for(int k = 0; k < ri*8; k++)
                {
                    FP theta = (FP)1 / ri * k * FP.Pi / (FP)4;
                    position.X = ri * spaceBetween * FPMath.Sin(theta);
                    position.Y = 0;
                    position.Z = ri * spaceBetween * FPMath.Cos(theta);
                    items.Add(new EnvQueryItem(frame, numTests, position, origin));
                }
            }

            return items;*/
        }
    }
}
