using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using MEPNumerator.Event;
using MEPNumerator.Revit;
using MEPNumerator.Startup;
using MEPNumerator.ViewModels;
using Prism.Events;
using System;
using System.Windows;

namespace MEPNumerator
{
    public partial class MainWindow : Window
    {
        private IEventAggregator _eventAggregator;

        public MainWindow(MainViewModel mainViewModel, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            DataContext = mainViewModel;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<CloseMainWindowViewEvent>().Subscribe(OnCloseMainWindowView);
        }
        private void OnCloseMainWindowView()
        {
            Close();
        }
    }
}
