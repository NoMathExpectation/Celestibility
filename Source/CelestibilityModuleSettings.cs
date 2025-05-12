using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework.Input;
using Monocle;
using NoMathExpectation.Celeste.Celestibility.Entities;
using NoMathExpectation.Celeste.Celestibility.Speech;

namespace NoMathExpectation.Celeste.Celestibility
{
    public class CelestibilityModuleSettings : EverestModuleSettings
    {
        [SettingName("Celestibility_setting_speech")]
        public bool SpeechEnabled { get; set; } = true;
        //[SettingName("Celestibility_setting_bind_speech")]
        //public ButtonBinding SpeechBind { get; set; }

        [SettingName("Celestibility_setting_speech_test_text")]
        [SettingMaxLength(1023)]
        public string TestSpeechText { get; set; } = "This is an example of speech text.";

        public bool PlaySpeech { get; set; } = false;

        public void CreateSpeechEnabledEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.OnOff("Celestibility_setting_speech".DialogClean(), SpeechEnabled)
                .Change(value => ToggleSpeech(value)));
        }

        public void CreatePlaySpeechEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.Button("Celestibility_setting_speech_test_play".DialogClean())
                .Pressed(() => UniversalSpeech.SpeechSay(TestSpeechText, true, ignoreOff: true)));
        }

        public static void ToggleSpeech(bool? @bool)
        {
            CelestibilityModule.Settings.SpeechEnabled = @bool ?? !CelestibilityModule.Settings.SpeechEnabled;
            string s = CelestibilityModule.Settings.SpeechEnabled ? "on" : "off";
            string log = $"Celestibility_speech_{s}".DialogClean();
            LogUtil.Log(log);
            if (Engine.Commands.Open)
            {
                Engine.Commands.Log(log);
            }
            log.SpeechSay(ignoreOff: true);
        }

        [Command("narrate", "(Celestibility) Toggle narrate.")]
        public static void ToggleSpeech()
        {
            ToggleSpeech(null);
        }

        public const string speechProviderNone = "Celestibility_setting_speech_provider_none";

        public string SpeechProvider { get; set; } = speechProviderNone;

        public void CreateSpeechProviderEntry(TextMenu menu, bool inGame)
        {
            TextMenu.Option<string> entry = new("Celestibility_setting_speech_provider".DialogClean());
            entry.Change(value =>
            {
                SpeechProvider = value;
                SpeechEngine.SetCurrent(value);
            });
            menu.Add(entry);

            var providers = SpeechEngine.GetProviders();
            if (providers.Count <= 0)
            {
                SpeechEngine.SetCurrent(speechProviderNone);
                entry.Add(speechProviderNone.DialogClean(), null, true);
                return;
            }

            SpeechProvider = SpeechEngine.TrySetCurrentDefault();

            foreach (var (name, _) in providers)
            {
                entry.Add(name.DialogClean(), name, SpeechProvider == name);
            }
            return;
        }

        [SettingName("Celestibility_setting_camera")]
        public bool Camera { get; set; } = true;
        [SettingName("Celestibility_setting_bind_toggle_camera")]
        public ButtonBinding CameraBind { get; set; } = new ButtonBinding(0, Keys.Y);
        [SettingName("Celestibility_setting_bind_move_camera_to_player")]
        public ButtonBinding CameraMoveToPlayer { get; set; } = new ButtonBinding(0, Keys.H);
        [SettingName("Celestibility_setting_bind_move_camera_left")]
        public ButtonBinding CameraMoveLeft { get; set; } = new ButtonBinding(0, Keys.J);
        [SettingName("Celestibility_setting_bind_move_camera_right")]
        public ButtonBinding CameraMoveRight { get; set; } = new ButtonBinding(0, Keys.L);
        [SettingName("Celestibility_setting_bind_move_camera_up")]
        public ButtonBinding CameraMoveUp { get; set; } = new ButtonBinding(0, Keys.I);
        [SettingName("Celestibility_setting_bind_move_camera_down")]
        public ButtonBinding CameraMoveDown { get; set; } = new ButtonBinding(0, Keys.K);
        [SettingName("Celestibility_setting_bind_move_camera_precise")]
        public ButtonBinding CameraMovePrecise { get; set; } = new ButtonBinding(0, Keys.RightShift);
        [SettingName("Celestibility_setting_bind_camera_narrate")]
        public ButtonBinding CameraNarrate { get; set; } = new ButtonBinding(0, Keys.U);
        [SettingName("Celestibility_setting_bind_camera_position")]
        public ButtonBinding CameraPosition { get; set; } = new ButtonBinding(0, Keys.OemOpenBrackets);
        [SettingName("Celestibility_setting_bind_player_position")]
        public ButtonBinding PlayerPosition { get; set; } = new ButtonBinding(0, Keys.OemCloseBrackets);
        [SettingName("Celestibility_setting_narrate_tutorial")]
        public ButtonBinding Tutorial { get; set; } = new ButtonBinding(0, Keys.T);

        public void CreateCameraEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.OnOff("Celestibility_setting_camera".DialogClean(), Camera)
                .Change(value => ToggleCamera(value)));
        }

        public static void ToggleCamera(bool? @bool)
        {
            CelestibilityModule.Settings.Camera = @bool ?? !CelestibilityModule.Settings.Camera;
            AccessCamera.Instance?.Toggle(CelestibilityModule.Settings.Camera);
            string s = CelestibilityModule.Settings.Camera ? "on" : "off";
            string log = $"Celestibility_camera_{s}".DialogClean();
            LogUtil.Log(log);
            if (Engine.Commands.Open)
            {
                Engine.Commands.Log(log);
            }
            log.SpeechSay();
        }

        [Command("camera", "(Celestibility) Toggle access camera.")]
        public static void ToggleCamera()
        {
            ToggleCamera(null);
        }

        [SettingName("Celestibility_setting_radar")]
        public bool RadarEnabled { get; set; } = true;
        [SettingName("Celestibility_setting_radar_max_distance")]
        [SettingRange(1, 128, true)]
        public int RadarMaxDistance { get; set; } = 64;
        [SettingName("Celestibility_setting_radar_toggle")]
        public ButtonBinding RadarToggle { get; set; } = new ButtonBinding(0, Keys.D0);
        [SettingName("Celestibility_setting_radar_increase_distance")]
        public ButtonBinding RadarIncreaseDistance { get; set; } = new ButtonBinding(0, Keys.OemPlus);
        [SettingName("Celestibility_setting_radar_decrease_distance")]
        public ButtonBinding RadarDecreaseDistance { get; set; } = new ButtonBinding(0, Keys.OemMinus);

        [SettingName("Celestibility_setting_read_ground")]
        public bool ReadGround { get; set; } = true;

        [SettingSubMenu]
        public class DebugSubMenu
        {
            [SettingName("Celestibility_setting_debug_dump_unknown_entities")]
            [SettingSubText("Celestibility_setting_debug_dump_unknown_entities_desc")]
            public bool DumpUnknownEntities { get; set; } = false;
        }

        [SettingName("Celestibility_setting_debug")]
        public DebugSubMenu Debug { get; set; } = new();
    }
}
