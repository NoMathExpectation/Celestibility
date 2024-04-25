namespace NoMathExpectation.Celeste.Celestibility.Speech
{
    public interface ISpeechProvider
    {
        public string Name { get; }

        public void Say(string text, bool interrupt = false);

        public void Stop();
    }
}
