using UnityEngine;

namespace Veinfire
{
    public static class World
    {
        const float CastRadius = 0.4f;
        const float Skin = 0.06f;

        public static Vector3 Planar(Vector3 from, Vector3 to)
        {
            var delta = to - from;
            delta.y = 0f;
            return delta;
        }

        public static float BodyRadius(Transform t)
        {
            if (t == null) return 0.5f;
            var scale = Mathf.Max(t.lossyScale.x, t.lossyScale.z);
            var cap = t.GetComponent<CapsuleCollider>();
            if (cap != null) return Mathf.Max(0.28f, cap.radius * scale);
            return 0.5f * scale;
        }

        public static bool Touching(Transform a, Transform b, float pad = 0.25f)
        {
            if (a == null || b == null) return false;
            return Planar(a.position, b.position).magnitude <= BodyRadius(a) + BodyRadius(b) + pad;
        }

        public static Vector3 FromInput(Vector2 input)
        {
            return new Vector3(input.x, 0f, input.y);
        }

        public static bool IsMapSolid(Collider collider)
        {
            if (collider == null || !collider.enabled || collider.isTrigger) return false;
            var t = collider.transform;
            while (t != null)
            {
                if (t.name == "MapSolid") return true;
                t = t.parent;
            }
            return false;
        }

        public static void MovePlanar(Rigidbody body, Vector3 planarVelocity)
        {
            if (body == null) return;
            planarVelocity.y = 0f;
            Physics.autoSyncTransforms = true;
            Physics.SyncTransforms();
            var from = body.position;
            GetSweep(body, from, out var low, out var high, out var radius);
            from = Depenetrate(from, low, high, radius);
            GetSweep(body, from, out low, out high, out radius);
            var next = CapsuleMove(from, planarVelocity * Time.fixedDeltaTime, low, high, radius, body);
            next.y = from.y;
            body.MovePosition(next);
        }

        static void GetSweep(Rigidbody body, Vector3 pos, out Vector3 low, out Vector3 high, out float radius)
        {
            var cap = body.GetComponent<CapsuleCollider>();
            var scale = body.transform.lossyScale;
            if (cap != null)
            {
                radius = cap.radius * Mathf.Max(scale.x, scale.z) * 0.92f;
                var height = Mathf.Max(cap.height * scale.y, radius * 2f + 0.05f);
                var center = pos + Vector3.Scale(cap.center, scale);
                var half = Mathf.Max(0.02f, height * 0.5f - radius);
                low = center + Vector3.down * half;
                high = center + Vector3.up * half;
                return;
            }

            radius = CastRadius;
            low = pos + Vector3.up * 0.2f;
            high = pos + Vector3.up * 1.5f;
        }

        static Vector3 Depenetrate(Vector3 pos, Vector3 low, Vector3 high, float radius)
        {
            var hits = Physics.OverlapCapsule(low, high, radius, ~0, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < hits.Length; i++)
            {
                var col = hits[i];
                if (!IsMapSolid(col)) continue;
                var origin = (low + high) * 0.5f;
                var closest = col.ClosestPoint(origin);
                var push = origin - closest;
                push.y = 0f;
                if (push.sqrMagnitude < 0.0001f)
                {
                    push = Planar(col.bounds.center, pos);
                    if (push.sqrMagnitude < 0.0001f) continue;
                }

                var dist = push.magnitude;
                var need = radius + Skin;
                if (dist < need)
                    pos += push / dist * (need - dist);
            }

            return pos;
        }

        static Vector3 CapsuleMove(Vector3 from, Vector3 delta, Vector3 low, Vector3 high, float radius, Rigidbody body)
        {
            var mag = delta.magnitude;
            if (mag < 0.0001f) return from;
            var dir = delta / mag;
            if (Physics.CapsuleCast(low, high, radius, dir, out var hit, mag + Skin, ~0, QueryTriggerInteraction.Ignore)
                && IsMapSolid(hit.collider))
            {
                var allowed = Mathf.Max(0f, hit.distance - Skin);
                var stopped = from + dir * allowed;
                GetSweep(body, stopped, out var sLow, out var sHigh, out var sRadius);
                var n = hit.normal;
                n.y = 0f;
                if (n.sqrMagnitude < 0.01f) return Depenetrate(stopped, sLow, sHigh, sRadius);
                n.Normalize();
                var slide = delta - n * Vector3.Dot(delta, n);
                slide.y = 0f;
                var sm = slide.magnitude;
                if (sm < 0.0001f) return Depenetrate(stopped, sLow, sHigh, sRadius);
                if (Physics.CapsuleCast(sLow, sHigh, sRadius, slide / sm, out hit, sm + Skin, ~0, QueryTriggerInteraction.Ignore)
                    && IsMapSolid(hit.collider))
                    return Depenetrate(stopped, sLow, sHigh, sRadius);
                var slid = stopped + slide;
                GetSweep(body, slid, out var dLow, out var dHigh, out var dRadius);
                return OverlapsSolid(dLow, dHigh, dRadius) ? Depenetrate(stopped, sLow, sHigh, sRadius) : slid;
            }

            var dest = from + delta;
            GetSweep(body, dest, out var nLow, out var nHigh, out var nRadius);
            return OverlapsSolid(nLow, nHigh, nRadius) ? from : dest;
        }

        static bool OverlapsSolid(Vector3 low, Vector3 high, float radius)
        {
            var hits = Physics.OverlapCapsule(low, high, radius, ~0, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < hits.Length; i++)
                if (IsMapSolid(hits[i])) return true;
            return false;
        }

        public static void PrepareCharacterBody(Rigidbody body)
        {
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = true;
            body.constraints = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ
                               | RigidbodyConstraints.FreezePositionY;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public static void MakeTriggerVolume(GameObject go)
        {
            foreach (var mesh in go.GetComponentsInChildren<MeshCollider>())
                mesh.enabled = false;

            var capsule = go.GetComponent<CapsuleCollider>();
            if (capsule == null)
                capsule = go.AddComponent<CapsuleCollider>();
            capsule.enabled = true;
            capsule.isTrigger = true;
            capsule.direction = 1;
            capsule.radius = 0.42f;
            capsule.height = 2f;
            capsule.center = Vector3.zero;

            foreach (var collider in go.GetComponents<Collider>())
            {
                if (collider is MeshCollider) continue;
                collider.isTrigger = true;
            }
        }
    }
}
