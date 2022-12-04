using Autodesk.Revit.DB;
using MEPNumerator.Data.Repositories;

namespace MEPNumerator.Model.Mappers
{
    public class ElectricMapper : IElectricMapper
    {
        public ElectricMapper(IElectricRepository pipingRepository)
        {
            Id = pipingRepository.GetAll()[0].Id;
            Prefix = pipingRepository.GetAll()[0].Prefix;
            SelectedParameter = pipingRepository.GetAll()[0].SelectedParameter;
            OverrideParameterValueIsEnabled = pipingRepository.GetAll()[0].OverrideParameterValueIsEnabled;
        }
        public int Id { get; set; }
        public string Prefix { get; set; }


        public string SelectedParameter { get; set; }

        public bool OverrideParameterValueIsEnabled { get; set; }

        public BuiltInParameter BuiltInParameter { get; set; }
    }


}
