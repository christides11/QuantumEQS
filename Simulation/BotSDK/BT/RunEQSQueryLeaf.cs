#if QEQS_BOTSDK
namespace Quantum.EQS
{
    [System.Serializable]
    public unsafe class RunEQSQueryLeaf : BTLeaf
    {
        public AssetRef<EnvironmentQueryAsset> behaviourTree;
        public AIBlackboardValueKey positionBlackboardKey;
        public AIBlackboardValueKey targetBlackboardKey;
        
        public override void Init(BTParams btParams, ref AIContext aiContext)
        {
            base.Init(btParams, ref aiContext);
        }

        protected override BTStatus OnUpdate(BTParams btParams, ref AIContext aiContext)
        {
            var frame = btParams.Frame as Frame;
            if (!behaviourTree.IsValid 
                || !frame.TryFindAsset(behaviourTree, out var bt)
                || !btParams.Blackboard->TryGetEntityRef(frame, targetBlackboardKey.Key, out var targetEntityRef)
                || !frame.Unsafe.TryGetPointer<Transform3D>(targetEntityRef, out var targetTransform)) return BTStatus.Failure;

            var eqsQuery = bt.CreateQuery(frame, btParams.Entity);
            var envQuery = &eqsQuery;

            envQuery->targetEntity = targetEntityRef;
            envQuery->target = *targetTransform;

            if (!EnvQueryHelper.TryExecuteQueryImmediate(frame, envQuery, out var itemsSorted)
                || itemsSorted.Count == 0)
            {
                Log.Debug("Failure A.");
                return BTStatus.Failure;
            }

            var pos = itemsSorted[0].GetWorldPosition();
            btParams.Blackboard->Set(frame, positionBlackboardKey.Key, pos);
            return BTStatus.Success;
        }
    }
}
#endif