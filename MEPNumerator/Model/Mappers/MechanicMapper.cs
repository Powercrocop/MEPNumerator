using Autodesk.Revit.DB;
using MEPNumerator.Data.Repositories;
using MEPNumerator.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPNumerator.Model.Mappers
{
    public class MechanicMapper : IMechanicMapper
    {
        public MechanicMapper(IMechanicRepository mechanicRepository)
        {
            Id = mechanicRepository.GetAll()[0].Id;
            PrefixIsEnabled = mechanicRepository.GetAll()[0].PrefixIsEnabled;
            Prefix = mechanicRepository.GetAll()[0].Prefix;
            SystemAbbreviationIsEnabled = mechanicRepository.GetAll()[0].SystemAbbreviationIsEnabled;
            SelectedSystemAbbreviation = mechanicRepository.GetAll()[0].SelectedSystemAbbreviation;
            SelectedParameter = mechanicRepository.GetAll()[0].SelectedParameter;
            OverrideParameterValueIsEnabled = mechanicRepository.GetAll()[0].OverrideParameterValueIsEnabled;
        }
        public int Id { get; set; }
        public bool PrefixIsEnabled { get; set; }
        public string Prefix { get; set; }

        public bool SystemAbbreviationIsEnabled { get; set; }
        public string SelectedSystemAbbreviation { get; set; }

        public string SelectedParameter { get; set; }

        public bool OverrideParameterValueIsEnabled { get; set; }

        public BuiltInParameter BuiltInParameter { get; set; }
    }
}
