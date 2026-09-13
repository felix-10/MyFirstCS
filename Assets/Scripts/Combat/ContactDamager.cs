using UnityEngine;

namespace Veinfire
{
    public sealed class ContactDamager : MonoBehaviour
    {
        public float DamagePerSecond = 9f;
        PlayerHealth _player;

        void Update()
        {
            if (GameSession.IsPaused) return;
            if (_player == null)
            {
                var motor = FindFirstObjectByType<PlayerMotor>();
                if (motor != null) _player = motor.GetComponent<PlayerHealth>();
            }

            if (_player == null || _player.IsDead) return;
            if (!World.Touching(transform, _player.transform, 0.28f)) return;

            _player.Damage(DamagePerSecond * 0.45f, true, 0.38f);
        }
    }
}
