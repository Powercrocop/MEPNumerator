using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;

namespace MEPNumerator.Revit
{
    public interface IMechanicEngine
    {
        void Execute(UIApplication app);
        ObservableCollection<string> GetDuctSystemsAbbreviation(Document document);
        string GetName();
        ObservableCollection<string> GetParameters();
        ObservableCollection<string> GetPipeSystemsAbbreviation(Document document);
    }
}