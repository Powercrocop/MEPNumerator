using Autodesk.Revit.UI;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPNumerator.Event
{
    public class StartApplicationLoadExternalCommandDataEvent : PubSubEvent<ExternalCommandData>
    {
    }
}
