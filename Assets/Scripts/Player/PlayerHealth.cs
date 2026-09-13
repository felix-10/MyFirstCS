using System;
using UnityEngine;

namespace Veinfire
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        public event Action<float, float> Changed;
        public event Action Died;

        PlayerCombatStats _stats;
        float _hp;
        bool _dead;
        float _invulnUntil;
        float _shield;
        float _shieldUntil;
        float _healShow;

        public float Current => _hp;
        public float Max
        {
            get
            {
                var extra = 0f;
                var passives = GetComponent<HeroPassives>();
                if (passives != null) extra = passives.ExtraMaxHp;
                return _stats != null ? _stats.MaxHp + extra : extra;
            }
        }
        public bool IsDead => _dead;
        public bool IsInvulnerable => Time.unscaledTime < _invulnUntil;

        WorldHealthBar _bar;

        void Awake()
        {
            _stats = GetComponent<PlayerCombatStats>();
            _hp = _stats.MaxHp;
            GrantIFrames(0.8f);
            Notify();
        }

        void Start()
        {
            _bar = WorldHealthBar.Attach(transform, new Color(0.86f, 0.12f, 0.16f), 2.05f);
            Notify();
        }

        void Update()
        {
            if (_dead || GameSession.IsPaused || _stats == null) return;
            if (Time.time >= _shieldUntil) _shield = 0f;
            if (_stats.HealthRegen <= 0f || _hp >= Max) return;
            Heal(_stats.HealthRegen * Time.deltaTime * RunConfig.RegenMul, false);
        }

        void OnDestroy()
        {
            if (_bar != null) Destroy(_bar.gameObject);
        }

        public void GrantIFrames(float seconds)
        {
            _invulnUntil = Time.unscaledTime + seconds;
        }

        public void GrantShield(float amount, float duration)
        {
            if (RunConfig.HasMod(RelicMod.ShieldWall)) amount *= 1.35f;
            if (_dead || amount <= 0f) return;
            if (Time.time < _shieldUntil && _shield > 0f) return;
            _shield = amount;
            _shieldUntil = Time.time + duration;
            Notify();
        }

        void Notify()
        {
            Changed?.Invoke(_hp, Max);
            _bar?.Set(_hp, Max);
        }

        public void Heal(float amount, bool show = true)
        {
            if (_dead || amount <= 0f) return;
            var before = _hp;
            _hp = Mathf.Min(Max, _hp + amount);
            Notify();
            if (!show) return;
            var gained = _hp - before;
            _healShow += gained;
            if (_healShow >= 1f)
            {
                DamagePopup.SpawnTag(transform.position, $"+{Mathf.RoundToInt(_healShow)}", HitKind.Heal);
                _healShow = 0f;
            }
        }

        public void Damage(float amount, bool burstIFrames = false, float iframe = 0.12f)
        {
            if (_dead || amount <= 0f) return;
            if (IsInvulnerable) return;
            if (RunConfig.HasCurse(CurseId.FrostCalamity) && UnityEngine.Random.value < 0.12f)
                RunConfig.PlayerSlowUntil = Time.time + 0.85f;

            var resist = 20f / (20f + Mathf.Max(0f, _stats.Armor));
            var taken = amount * resist;
            if (Time.time < _shieldUntil && _shield > 0f)
            {
                var absorb = Mathf.Min(_shield, taken);
                _shield -= absorb;
                taken -= absorb;
                if (_shield <= 0f) _shieldUntil = 0f;
            }

            if (taken <= 0f)
            {
                Notify();
                return;
            }

            if (RunConfig.HasRevive && !RunConfig.UsedRevive && _hp - taken <= 0f)
            {
                RunConfig.UsedRevive = true;
                RunConfig.HasRevive = false;
                _hp = Max * 0.45f;
                GrantIFrames(1.2f);
                GrantShield(Max * 0.2f, 3f);
                Notify();
                GameFeel.LevelUp();
                return;
            }

            _hp -= taken;
            RunConfig.DamageTaken += taken;
            if (burstIFrames) GrantIFrames(Mathf.Max(0.08f, iframe));
            Notify();
            var flash = GetComponent<HitFlash>();
            flash?.Play();
            if (_hp <= 0f)
            {
                _dead = true;
                _hp = 0f;
                GameFeel.Death();
                Died?.Invoke();
            }
            else
            {
                GameFeel.Hurt();
            }
        }

        public void Restore(float current, float max)
        {
            if (_stats == null) _stats = GetComponent<PlayerCombatStats>();
            _stats.MaxHp = Mathf.Max(1f, max);
            _hp = Mathf.Clamp(current, 1f, Max);
            _dead = false;
            Notify();
        }

        public void RaiseMaxHp(float extra, bool fill)
        {
            _stats.MaxHp += extra;
            if (fill) _hp = Max;
            else _hp = Mathf.Min(_hp + extra, Max);
            Notify();
        }
    }
}
