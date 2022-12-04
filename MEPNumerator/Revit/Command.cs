#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using MEPNumerator.Data.Repositories;
using MEPNumerator.Event;
using MEPNumerator.Model.Mappers;
using MEPNumerator.Startup;
using MEPNumerator.ViewModels;
using Prism.Events;

#endregion

namespace MEPNumerator.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        private ExternalCommandData _commandData;
        private UIApplication _uiapp;
        private UIDocument _uidoc;
        private Application _app;
        private Document _doc;

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
                _commandData = commandData;
                _uiapp = commandData.Application;
                _uidoc = commandData.Application.ActiveUIDocument;
                _app = commandData.Application.Application;
                _doc = commandData.Application.ActiveUIDocument.Document;
                ApplicationStartup();

            return Result.Succeeded;
        }
        private void ApplicationStartup()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainWindow = container.Resolve<MainWindow>();
            mainWindow.Show();

            
            var eventAggregator = container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<StartApplicationLoadExternalCommandDataEvent>().Publish(_commandData);
            eventAggregator.GetEvent<StartApplicationEvent>().Publish();
        }
    }
}
