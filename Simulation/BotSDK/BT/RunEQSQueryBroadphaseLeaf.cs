#if QEQS_BOTSDK
namespace Quantum.EQS
{
    [System.Serializable]
    public unsafe class RunEQSQueryBroadphaseLeaf : BTLeaf
    {
        public AssetRef<EnvironmentQueryAsset> eqAsset;
        public AIBlackboardValueKey eqsEntityBlackboardKey;
        public AIBlackboardValueKey positionBlackboardKey;
        public AIBlackboardValueKey targetBlackboardKey;
        
        public override void Init(BTParams btParams, ref AIContext aiContext)
        {
            base.Init(btParams, ref aiContext);
        }

        public override void OnEnter(BTParams btParams, ref AIContext aiContext)
        {
            base.OnEnter(btParams, ref aiContext);
            
            var frame = btParams.Frame as Frame;
            if (!btParams.Blackboard->TryGetEntityRef(frame, eqsEntityBlackboardKey.Key, out var eqsEntity)
                || !frame.Unsafe.TryGetPointer<EnvQueryCached>(eqsEntity, out var eqsCached)) return;

            eqsCached->stage = 0;
        }

        protected override BTStatus OnUpdate(BTParams btParams, ref AIContext aiContext)
        {
            var frame = btParams.Frame as Frame;
            if (!eqAsset.IsValid 
                || !frame.TryFindAsset(eqAsset, out var eqsAsset)
                || !btParams.Blackboard->TryGetEntityRef(frame, targetBlackboardKey.Key, out var targetEntityRef)
                || !frame.Unsafe.TryGetPointer<Transform3D>(targetEntityRef, out var targetTransform)
                || !btParams.Blackboard->TryGetEntityRef(frame, eqsEntityBlackboardKey.Key, out var eqsEntity)) return BTStatus.Failure;

            EnvQueryCached* eqsCached;
            // Entity doesn't exist, create it.
            if (!eqsEntity.IsValid || !frame.Exists(eqsEntity))
            {
                if (!eqsAsset.TryCreateCachedQuery(frame, btParams.Entity, out eqsEntity))
                {
                    return BTStatus.Failure;
                }

                eqsCached = frame.Unsafe.GetPointer<EnvQueryCached>(eqsEntity);
                btParams.Blackboard->Set(frame, eqsEntityBlackboardKey.Key, eqsEntity);
            }
            
            eqsCached = frame.Unsafe.GetPointer<EnvQueryCached>(eqsEntity);

            if (eqsCached->stage == -1)
            {
                frame.Destroy(eqsEntity);
                return BTStatus.Failure;
            }else if (eqsCached->stage == 0)
            {
                eqsCached->targetEntity = targetEntityRef;
                eqsCached->target = *targetTransform;
            }
            else if (eqsCached->stage == 3)
            {
                eqsCached = frame.Unsafe.GetPointer<EnvQueryCached>(eqsEntity);

                if (EnvQueryHelperBroadphase.TryGetBestPosition(frame, eqsCached, out var pos))
                {
                    btParams.Blackboard->Set(frame, positionBlackboardKey.Key, pos);
                }
                return BTStatus.Success;
            }

            return BTStatus.Running;
        }

        public override void OnExit(BTParams btParams, ref AIContext aiContext)
        {
            base.OnExit(btParams, ref aiContext);

            var frame = btParams.Frame as Frame;
            if (!btParams.Blackboard->TryGetEntityRef(frame, eqsEntityBlackboardKey.Key, out var eqsEntity)) return;
            if (!eqsEntity.IsValid || !frame.Exists(eqsEntity)) return;
            //frame.Destroy(eqsEntity);
        }
    }
}
#endif