using ApiDocumentationComparer.ViewModels;
using System.Windows;

namespace ApiDocumentationComparer
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel mainViewModel;

        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            this.mainViewModel = mainViewModel;
            DataContext = mainViewModel;
        }
    }
}
