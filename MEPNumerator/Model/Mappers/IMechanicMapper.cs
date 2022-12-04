using Autodesk.Revit.DB;

namespace MEPNumerator.Model.Mappers
{
    public interface IMechanicMapper
    {
        BuiltInParameter BuiltInParameter { get; set; }
        int Id { get; set; }
        bool OverrideParameterValueIsEnabled { get; set; }
        string Prefix { get; set; }
        bool PrefixIsEnabled { get; set; }
        string SelectedParameter { get; set; }
        string SelectedSystemAbbreviation { get; set; }
        bool SystemAbbreviationIsEnabled { get; set; }
    }
}