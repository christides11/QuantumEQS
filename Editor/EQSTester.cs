using UnityEngine;

namespace Quantum.EQS
{
    public class EQSTester : MonoBehaviour
    {
        public EnvironmentQueryAsset queryAsset;

        private void OnDrawGizmos()
        {
            /*
            using (Draw.InLocalSpace(transform)) {
                if (GizmoContext.InSelection(this)) {
                    // Draw a yellow cylinder
                    Draw.WireCylinder(Vector3.zero, Vector3.up, 2f, 0.5f, Color.yellow);
                } else {
                    // Draw a yellow circle with some transparency
                    Draw.xz.Circle(Vector3.zero, 0.5f, Color.yellow * new Color(1, 1, 1, 0.5f));
                }
            }*/
        }
    }
}