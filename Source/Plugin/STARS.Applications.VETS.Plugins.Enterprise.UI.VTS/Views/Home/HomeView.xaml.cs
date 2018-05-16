using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STARS.Applications.VETS.Plugins.VTS.UI.Views.Home
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        public void MouseDoubleClickHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.GetPosition(GridViewElement).Y > 28)
            {
                ShowTestProperties.Command.Execute(null);
            }
            e.Handled = true;
        }

        private void SaveResources(object sender, RoutedEventArgs e)
        {
            SaveAsNewResource.Command.Execute(null);
            ControlSaveAs.Command.Execute(null);
        }

        private void CancelSave(object sender, RoutedEventArgs e)
        {
            ControlSaveAs.Command.Execute(null);
        }
    }
}
