using Celeste;
using Celeste.Mod;

namespace NoMathExpectation.Celeste.Celestibility
{
    public class CelestibilityModuleSettings : EverestModuleSettings
    {
        [SettingName("Speech Text")]
        [SettingMaxLength(1023)]
        public string TestSpeechText { get; set; } = "This is an example of speech text.";

        public bool PlaySpeech { get; set; } = false;

        public void CreatePlaySpeechEntry(TextMenu menu, bool inGame)
        {
            menu.Add(new TextMenu.Button("Play Speech")
                .Pressed(() => UniversalSpeech.speechSay(TestSpeechText, true)));
        }
    }
}
