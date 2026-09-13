using UnityEngine;

namespace Veinfire
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerCombatStats))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        PlayerCombatStats _stats;
        Rigidbody _body;
        PlayerDash _dash;

        void Awake()
        {
            _stats = GetComponent<PlayerCombatStats>();
            _body = GetComponent<Rigidbody>();
            _dash = GetComponent<PlayerDash>();
            World.PrepareCharacterBody(_body);
            World.MakeTriggerVolume(gameObject);
        }

        void FixedUpdate()
        {
            if (GameSession.IsPaused)
                return;

            if (_dash != null && _dash.IsDashing)
                return;

            var passives = GetComponent<HeroPassives>();
            var move = World.FromInput(ReadMoveInput()) * _stats.MoveSpeed
                       * (passives != null ? passives.MoveMul : 1f)
                       * RunConfig.PlayerMoveMul
                       * (Time.time < RunConfig.PlayerSlowUntil ? 0.5f : 1f);
            World.MovePlanar(_body, move);

            if (move.sqrMagnitude > 0.01f)
            {
                var facing = Quaternion.LookRotation(move.normalized, Vector3.up);
                _body.MoveRotation(Quaternion.Slerp(_body.rotation, facing, 12f * Time.fixedDeltaTime));
            }
        }

        static Vector2 ReadMoveInput()
        {
            var input = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.y -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1f;
            if (input.sqrMagnitude > 1f) input.Normalize();
            return input;
        }
    }
}
