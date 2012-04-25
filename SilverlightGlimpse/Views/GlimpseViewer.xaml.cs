using System.Windows;
using SilverlightGlimpse.Services;

namespace SilverlightGlimpse.Views
{
    public partial class GlimpseViewer
    {
        public GlimpseViewer()
        {
            InitializeComponent();
            DataContext = Glimpse.Service;
        }

        private void btnContract_Click(object sender, RoutedEventArgs e)
        {
            layoutViewer.Visibility = Visibility.Collapsed;
        }

        private void btnExpand_Click(object sender, RoutedEventArgs e)
        {
            layoutViewer.Visibility = Visibility.Visible;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Glimpse.Service.Clear();
        }
    }
}