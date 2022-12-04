using Autodesk.Revit.UI;
using System.Collections.ObjectModel;

namespace MEPNumerator.Revit
{
    public interface IElectricEngine
    {
        void Execute(UIApplication app);
        string GetName();
        ObservableCollection<string> GetParameters();
    }
}