using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPNumerator.Revit
{
    public class PairOfConnectedConnectors
    {
        public Connector Connector1 { get; set; }
        public int Connector1Id { get; set; }
        public Connector Connector2 { get; set; }
        public int Connector2Id { get; set; }
    }
}
