using System.Windows;
using OllamaWpfClient.Services;
using OllamaWpfClient.ViewModels;

namespace OllamaWpfClient
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            IOllamaClient ollamaClient = new OllamaClient();
            _viewModel = new MainViewModel(ollamaClient);
            DataContext = _viewModel;

            Loaded += (_, _) => _viewModel.LoadModelsCommand.Execute(null);
        }
    }
}
