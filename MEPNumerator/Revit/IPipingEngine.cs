using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;

namespace MEPNumerator.Revit
{
    public interface IPipingEngine
    {
        void Execute(UIApplication app);
        string GetName();
        ObservableCollection<string> GetParameters();
        ObservableCollection<string> GetPipeSystemsAbbreviation(Document document);
    }
}