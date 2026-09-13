using UnityEngine;

namespace Veinfire
{
    public static class GameFonts
    {
        static Font _ui;

        public static Font UI
        {
            get
            {
                if (_ui != null) return _ui;
                var names = new[]
                {
                    "Microsoft YaHei UI",
                    "Microsoft YaHei",
                    "微软雅黑",
                    "PingFang SC",
                    "SimHei",
                    "Arial"
                };
                for (var i = 0; i < names.Length; i++)
                {
                    _ui = Font.CreateDynamicFontFromOSFont(names[i], 20);
                    if (_ui != null) break;
                }

                if (_ui == null)
                    _ui = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                          ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (_ui != null)
                    _ui.RequestCharactersInTexture("0123456789灼湿岩暴石化拾 /!+", 48);
                return _ui;
            }
        }
    }
}
