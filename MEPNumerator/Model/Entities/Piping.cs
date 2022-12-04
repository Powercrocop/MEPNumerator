using System;
using System.Collections.Generic;
using System.Text;

namespace MEPNumerator.Model.Entities
{
    public class Piping
    {
        public int Id { get; set; }
        public bool PrefixIsEnabled { get; set; }
        public string Prefix { get; set; }

        public bool SystemAbbreviationIsEnabled { get; set; }
        public string SelectedSystemAbbreviation { get; set; }

        public string SelectedParameter { get; set; }

        public bool OverrideParameterValueIsEnabled { get; set; }
    }
}
