using System;
using System.Collections.Generic;
using System.Text;

namespace MEPNumerator.Model.Entities
{
    public class Electric
    {
        public int Id { get; set; }
        public string Prefix { get; set; }

        public string SelectedParameter { get; set; }

        public bool OverrideParameterValueIsEnabled { get; set; }
    }
}
