using System.Collections.Generic;

namespace Quantum.EQS
{
    [System.Serializable]
    public unsafe class EnvQueryGeneratorComposite : EnvQueryGenerator
    {
        public List<EnvQueryGenerator> generators = new List<EnvQueryGenerator>();
        
        public override List<EnvQueryItem> GenerateItems(Frame frame, EnvQuery* envQuery, int numTests, Transform3D centerOfItems)
        {
            List<EnvQueryItem> items = new();

            foreach (var g in generators)
            {
                items.AddRange(g.GenerateItems(frame, envQuery, numTests, centerOfItems));
            }

            return items;
        }
    }
}