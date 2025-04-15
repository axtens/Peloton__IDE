namespace Peloton_IDE.Presentation
{
    public partial record TranslateViewModel(Entity Entity)
    {
        private string? _sourceText;

        public string? SourceText
        {
            get { return _sourceText; }
            set
            {
                if (_sourceText != value)
                {
                    _sourceText = value;
                }
            }
        }
    }
}