using UnityEngine;

namespace Veinfire
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerDash : MonoBehaviour
    {
        public float Distance = 6.5f;
        public float Duration = 0.14f;
        public float Cooldown = 2.0f;

        Rigidbody _body;
        PlayerHealth _health;
        PlayerCombatStats _stats;
        float _cd;
        float _cdMax = 2f;
        float _dashLeft;
        Vector3 _dashVel;

        public bool IsDashing => _dashLeft > 0f;
        public float CooldownLeft => Mathf.Max(0f, _cd);
        public float Charge01
        {
            get
            {
                if (_cd <= 0f) return 1f;
                return 1f - Mathf.Clamp01(_cd / Mathf.Max(0.05f, _cdMax));
            }
        }

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _health = GetComponent<PlayerHealth>();
            _stats = GetComponent<PlayerCombatStats>();
        }

        void Update()
        {
            if (GameSession.IsPaused || _health != null && _health.IsDead) return;
            _cd -= Time.deltaTime;
            if (!IsDashing && _cd <= 0f && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)))
                BeginDash();
        }

        void FixedUpdate()
        {
            if (!IsDashing || GameSession.IsPaused) return;
            _dashLeft -= Time.fixedDeltaTime;
            World.MovePlanar(_body, _dashVel);
        }

        void BeginDash()
        {
            var input = Vector3.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.z += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.z -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1f;
            if (input.sqrMagnitude < 0.01f) input = transform.forward;
            input.y = 0f;
            input.Normalize();

            var speed = Distance / Mathf.Max(0.05f, Duration);
            _dashVel = input * speed;
            _dashLeft = Duration;
            var passives = GetComponent<HeroPassives>();
            var cdMul = (_stats != null ? _stats.CooldownMul : 1f) * (passives != null ? passives.GeraltCdMul : 1f);
            _cdMax = Cooldown * cdMul;
            var iframe = Duration + 0.08f + (_stats != null ? _stats.DashIFrameBonus : 0f);
            if (RunConfig.RelicOn("冲刺纹章"))
            {
                _cd = 0f;
                iframe *= 0.7f;
            }
            else
                _cd = _cdMax;
            _health?.GrantIFrames(iframe);
            GameFeel.Dash(transform.position);
            InRunDirector.OnDash(transform.position);
        }

        public void ShortenCooldown(float seconds)
        {
            _cd = Mathf.Max(0f, _cd - seconds);
        }
    }
}
