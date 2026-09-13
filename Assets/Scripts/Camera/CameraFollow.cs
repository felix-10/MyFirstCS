using UnityEngine;

namespace Veinfire
{
    public sealed class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new Vector3(0f, 14f, -11f);
        public float Smooth = 10f;
        public bool LookAtTarget = true;

        float _shakeAmp;
        float _shakeTime;

        public static void Shake(float amplitude, float duration)
        {
            var cam = FindFirstObjectByType<CameraFollow>();
            if (cam == null) return;
            cam._shakeAmp = Mathf.Max(cam._shakeAmp, amplitude);
            cam._shakeTime = Mathf.Max(cam._shakeTime, duration);
        }

        void LateUpdate()
        {
            if (Target == null) return;
            var goal = Target.position + Offset;
            transform.position = Vector3.Lerp(
                transform.position,
                goal,
                1f - Mathf.Exp(-Smooth * Time.unscaledDeltaTime));

            if (_shakeTime > 0f)
            {
                _shakeTime -= Time.unscaledDeltaTime;
                var mag = _shakeAmp * Mathf.Clamp01(_shakeTime * 4f);
                transform.position += new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-0.35f, 0.35f),
                    Random.Range(-1f, 1f)) * mag;
                if (_shakeTime <= 0f) _shakeAmp = 0f;
            }

            if (LookAtTarget)
            {
                var look = Target.position + Vector3.up * 0.8f;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(look - transform.position, Vector3.up),
                    1f - Mathf.Exp(-Smooth * Time.unscaledDeltaTime));
            }
        }
    }
}
