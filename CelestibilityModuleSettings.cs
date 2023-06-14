using Celeste;
using Celeste.Mod;
using Monocle;

namespace NoMathExpectation.Celeste.Celestibility
{
    public class CelestibilityModuleSettings : EverestModuleSettings
    {
        [SettingName("Celestibility_setting_speech")]
        public bool SpeechEnabled { get; set; } = true;

        [SettingName("Celestibility_setting_speech_test_text")]
        [SettingMaxLength(1023)]
        public string TestSpeechText { get; set; } = "This is an example of speech text.";

        public bool PlaySpeech { get; set; } = false;

        public void CreatePlaySpeechEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.Button(Dialog.Clean("Celestibility_setting_speech_test_play"))
                .Pressed(() => UniversalSpeech.SpeechSay(TestSpeechText, true)));
        }

        [Command("narrate", "(Celestibility) Toggle narrate.")]
        public static void ToggleSpeech()
        {
            CelestibilityModule.Settings.SpeechEnabled = !CelestibilityModule.Settings.SpeechEnabled;
            Engine.Commands.Log($"Narrate {(CelestibilityModule.Settings.SpeechEnabled ? "on" : "off")}.");
        }

        [SettingName("Celestibility_setting_camera")]
        public bool Camera { get; set; } = true;
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
    }
}
