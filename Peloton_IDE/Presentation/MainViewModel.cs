using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Peloton_IDE.Presentation;

namespace Peloton_IDE.Presentation
{
    public partial class MainViewModel : ObservableObject
    {
        public string? Title { get; }

        [ObservableProperty]
        private string? name;

        public ICommand GoToIDEConfig { get; }
        public ICommand GoToTranslate { get; }

        public MainViewModel(
            INavigator navigator,
            IStringLocalizer localizer)
        {
            _navigator = navigator;
            Title = $"Main - {localizer["ApplicationName"]}";
            GoToIDEConfig = new AsyncRelayCommand(GoToIDEConfigView);
            GoToTranslate = new AsyncRelayCommand(GoToTranslateConfigView);
        }

        private async Task GoToIDEConfigView()
        {
            await _navigator.NavigateViewModelAsync<IDEConfigViewModel>(this, data: new Entity(Name!));
        }
        private async Task GoToTranslateConfigView()
        {
            await _navigator.NavigateViewModelAsync<TranslateViewModel>(this, data: new Entity(Name!));
        }

        private INavigator _navigator;
    }
}