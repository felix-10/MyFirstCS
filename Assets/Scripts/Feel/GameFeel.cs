using UnityEngine;
using UnityEngine.UI;

namespace Veinfire
{
    public sealed class GameFeel : MonoBehaviour
    {
        static GameFeel _i;
        Image _hurt;
        ParticleSystem _sparks;

        public static void Ensure()
        {
            AudioHub.Ensure();
            AudioHub.EnsureListener();
            var host = GameObject.Find("VeinfireAudio");
            if (host == null) return;
            if (_i == null) _i = host.GetComponent<GameFeel>();
            if (_i == null) _i = host.AddComponent<GameFeel>();
            if (_i._sparks == null || _i._hurt == null)
                _i.Build();
        }

        void Build()
        {
            var old = transform.Find("FeelCanvas");
            if (old != null) Destroy(old.gameObject);
            var sparksOld = transform.Find("Sparks");
            if (sparksOld != null) Destroy(sparksOld.gameObject);
            var canvasGo = new GameObject("FeelCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 250;
            canvas.pixelPerfect = false;
            var group = canvasGo.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            group.ignoreParentGroups = true;
            var raycaster = canvasGo.GetComponent<GraphicRaycaster>();
            if (raycaster != null) Destroy(raycaster);
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var hurtGo = new GameObject("Hurt", typeof(RectTransform), typeof(Image));
            hurtGo.transform.SetParent(canvasGo.transform, false);
            var rect = (RectTransform)hurtGo.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _hurt = hurtGo.GetComponent<Image>();
            _hurt.sprite = SpriteFactory.Solid(Color.white, 8);
            _hurt.color = new Color(0.7f, 0f, 0f, 0f);
            _hurt.raycastTarget = false;
            _hurt.maskable = false;

            var sparkGo = new GameObject("Sparks");
            sparkGo.transform.SetParent(transform, false);
            _sparks = sparkGo.AddComponent<ParticleSystem>();
            _sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = _sparks.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.4f;
            main.startLifetime = 0.28f;
            main.startSpeed = 6f;
            main.startSize = 0.18f;
            main.gravityModifier = 1.6f;
            main.maxParticles = 120;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = _sparks.emission;
            emission.enabled = false;
            var shape = _sparks.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;
            var renderer = sparkGo.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            if (shader != null) renderer.material = new Material(shader);
        }

        void LateUpdate()
        {
            if (_hurt == null) return;
            var c = _hurt.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * 1.8f);
            _hurt.color = c;
        }

        public static void Hit(Vector3 position, float amount, bool kill)
        {
            Ensure();
            AudioHub.Play(kill ? Sfx.Kill : Sfx.Hit, kill ? 1f : 0.62f);
            CameraFollow.Shake(kill ? 0.18f : 0.1f, kill ? 0.14f : 0.08f);
            Burst(position + Vector3.up * 0.8f, kill ? new Color(1f, 0.25f, 0.12f) : new Color(1f, 0.85f, 0.35f), kill ? 22 : 12);
        }

        public static void Shoot(Vector3 position)
        {
            Ensure();
            AudioHub.Play(Sfx.Shoot, 0.42f);
            Burst(position, new Color(1f, 0.8f, 0.3f), 6);
        }

        public static void Hurt()
        {
            Ensure();
            AudioHub.Play(Sfx.Hurt, 0.8f);
            CameraFollow.Shake(0.34f, 0.2f);
            if (_i._hurt != null)
                _i._hurt.color = new Color(0.75f, 0.05f, 0.08f, 0.48f);
        }

        public static void Dash(Vector3 position)
        {
            Ensure();
            AudioHub.Play(Sfx.Dash, 0.55f);
            CameraFollow.Shake(0.12f, 0.1f);
            Burst(position, new Color(0.8f, 0.9f, 1f), 10);
        }

        public static void Pickup()
        {
            Ensure();
            AudioHub.Play(Sfx.Pickup, 0.55f);
        }

        public static void LevelUp()
        {
            Ensure();
            AudioHub.Play(Sfx.LevelUp, 0.7f);
        }

        public static void Ui()
        {
            Ensure();
            AudioHub.Play(Sfx.Ui, 0.45f);
        }

        public static void Boss()
        {
            Ensure();
            AudioHub.Play(Sfx.Boss, 0.9f);
            CameraFollow.Shake(0.35f, 0.4f);
        }

        public static void Death()
        {
            Ensure();
            AudioHub.Play(Sfx.Death, 0.85f);
            CameraFollow.Shake(0.4f, 0.45f);
        }

        public static void Explosion(Vector3 position)
        {
            Ensure();
            AudioHub.Play(Sfx.Explosion, 0.8f);
            CameraFollow.Shake(0.3f, 0.22f);
            Burst(position, new Color(1f, 0.45f, 0.1f), 28);
        }

        public static void Pulse(Vector3 position)
        {
            Ensure();
            AudioHub.Play(Sfx.Explosion, 0.42f);
            CameraFollow.Shake(0.12f, 0.12f);
            Burst(position, new Color(1f, 0.32f, 0.55f), 16);
        }

        public static void EventPulse()
        {
            Ensure();
            AudioHub.Play(Sfx.Boss, 0.5f);
            CameraFollow.Shake(0.2f, 0.28f);
        }

        static void Burst(Vector3 position, Color color, int count)
        {
            if (_i == null || _i._sparks == null) return;
            var emit = new ParticleSystem.EmitParams
            {
                position = position,
                startColor = color,
                startSize = 0.16f,
                startLifetime = 0.32f,
                velocity = Vector3.up * 1.5f
            };
            _i._sparks.Emit(emit, count);
        }
    }
}
