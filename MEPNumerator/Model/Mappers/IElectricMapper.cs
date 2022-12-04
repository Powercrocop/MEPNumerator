using Autodesk.Revit.DB;

namespace MEPNumerator.Model.Mappers
{
    public interface IElectricMapper
    {
        BuiltInParameter BuiltInParameter { get; set; }
        int Id { get; set; }
        bool OverrideParameterValueIsEnabled { get; set; }
        string Prefix { get; set; }
        string SelectedParameter { get; set; }
    }
}