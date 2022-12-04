using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MEPNumerator.Model.Entities
{
    public class Mechanic
    {
        public int Id {get; set;}
        public bool PrefixIsEnabled { get; set; }
        public string Prefix { get; set; }

        public bool SystemAbbreviationIsEnabled { get; set; }
        public string SelectedSystemAbbreviation { get; set; }

        public string SelectedParameter { get; set; }

        public bool OverrideParameterValueIsEnabled { get; set; }

    }
}
