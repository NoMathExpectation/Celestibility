using Celeste;
using Celeste.Mod;
using Monocle;
using NoMathExpectation.Celeste.Celestibility.Entities;

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
            menu.Add(new TextMenu.OnOff(Dialog.Clean("Celestibility_setting_speech"), SpeechEnabled)
                .Change(value => ToggleSpeech(value)));
        }

        public void CreatePlaySpeechEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.Button(Dialog.Clean("Celestibility_setting_speech_test_play"))
                .Pressed(() => UniversalSpeech.SpeechSay(TestSpeechText, true, ignoreOff: true)));
        }

        public static void ToggleSpeech(bool? @bool)
        {
            CelestibilityModule.Settings.SpeechEnabled = @bool ?? !CelestibilityModule.Settings.SpeechEnabled;
            string s = CelestibilityModule.Settings.SpeechEnabled ? "on" : "off";
            string log = Dialog.Clean($"Celestibility_speech_{s}");
            LogUtil.Log(log);
            if (Engine.Commands.Open)
            {
                Engine.Commands.Log(log);
            }
            UniversalSpeech.SpeechSay(log, ignoreOff: true);
        }

        [Command("narrate", "(Celestibility) Toggle narrate.")]
        public static void ToggleSpeech()
        {
            ToggleSpeech(null);
        }

        [SettingName("Celestibility_setting_camera")]
        public bool Camera { get; set; } = true;
        [SettingName("Celestibility_setting_bind_toggle_camera")]
        public ButtonBinding CameraBind { get; set; }
        [SettingName("Celestibility_setting_bind_move_camera_to_player")]
        public ButtonBinding CameraMoveToPlayer { get; set; }
        [SettingName("Celestibility_setting_bind_move_camera_left")]
        public ButtonBinding CameraMoveLeft { get; set; }
        [SettingName("Celestibility_setting_bind_move_camera_right")]
        public ButtonBinding CameraMoveRight { get; set; }
        [SettingName("Celestibility_setting_bind_move_camera_up")]
        public ButtonBinding CameraMoveUp { get; set; }
        [SettingName("Celestibility_setting_bind_move_camera_down")]
        public ButtonBinding CameraMoveDown { get; set; }
        [SettingName("Celestibility_setting_bind_move_camera_precise")]
        public ButtonBinding CameraMovePrecise { get; set; }
        [SettingName("Celestibility_setting_bind_camera_narrate")]
        public ButtonBinding CameraNarrate { get; set; }

        public void CreateCameraEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.OnOff(Dialog.Clean("Celestibility_setting_camera"), Camera)
                .Change(value => ToggleCamera(value)));
        }

        public static void ToggleCamera(bool? @bool)
        {
            CelestibilityModule.Settings.Camera = @bool ?? !CelestibilityModule.Settings.Camera;
            AccessCamera.Instance?.Toggle(CelestibilityModule.Settings.Camera);
            string s = CelestibilityModule.Settings.Camera ? "on" : "off";
            string log = Dialog.Clean($"Celestibility_camera_{s}");
            LogUtil.Log(log);
            if (Engine.Commands.Open)
            {
                Engine.Commands.Log(log);
            }
            UniversalSpeech.SpeechSay(log);
        }

        [Command("camera", "(Celestibility) Toggle access camera.")]
        public static void ToggleCamera()
        {
            ToggleCamera(null);
        }
    }
}
