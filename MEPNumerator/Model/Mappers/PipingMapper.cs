using Autodesk.Revit.DB;
using MEPNumerator.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPNumerator.Model.Mappers
{
    public class PipingMapper : IPipingMapper
    {
        public PipingMapper(IPipingRepository pipingRepository)
        {
            Id = pipingRepository.GetAll()[0].Id;
            PrefixIsEnabled = pipingRepository.GetAll()[0].PrefixIsEnabled;
            Prefix = pipingRepository.GetAll()[0].Prefix;
            SystemAbbreviationIsEnabled = pipingRepository.GetAll()[0].SystemAbbreviationIsEnabled;
            SelectedSystemAbbreviation = pipingRepository.GetAll()[0].SelectedSystemAbbreviation;
            SelectedParameter = pipingRepository.GetAll()[0].SelectedParameter;
            OverrideParameterValueIsEnabled = pipingRepository.GetAll()[0].OverrideParameterValueIsEnabled;
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
