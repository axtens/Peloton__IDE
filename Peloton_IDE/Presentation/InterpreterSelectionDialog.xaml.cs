namespace Peloton_IDE.Presentation
{
    public sealed partial class InterpreterSelectionDialog : ContentDialog
    {
        public InterpreterSelectionDialog()
        {
            this.InitializeComponent();
        }

        private void OKHandler(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Microsoft.UI.Xaml.Data.BindingExpression bindingExpressionName = PathToInterpreter.GetBindingExpression(TextBox.TextProperty);
            bindingExpressionName.UpdateSource();
        }
    }
}
