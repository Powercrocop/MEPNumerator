using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using MEPNumerator.Event;
using MEPNumerator.Model.Mappers;
using System.Windows;
using System.Collections.ObjectModel;
using Autodesk.Revit.DB.Plumbing;

namespace MEPNumerator.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class MechanicEngine : IMechanicEngine, IExternalEventHandler
    {
        private ExternalCommandData _commandData;
        private UIApplication _uiapp;
        private UIDocument _uidoc;
        private Autodesk.Revit.ApplicationServices.Application _app;
        private Document _doc;
        private List<Element> _elementsForNumbering;

        private IEventAggregator _eventAggregator;
        private IMechanicMapper _mechanicMapper;

        public MechanicEngine(IEventAggregator eventAggregator, IMechanicMapper mechanicMapper)
        {
            _eventAggregator = eventAggregator;
            _mechanicMapper = mechanicMapper;
            _eventAggregator.GetEvent<StartApplicationLoadExternalCommandDataEvent>().Subscribe(OnLoadExternalCommandData);
            _elementsForNumbering = new List<Element>();
        }
        private void OnLoadExternalCommandData(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _uiapp = commandData.Application;
            _uidoc = commandData.Application.ActiveUIDocument;
            _app = commandData.Application.Application;
            _doc = commandData.Application.ActiveUIDocument.Document;
        }
        public void Execute(UIApplication app)
        {
            using (Transaction tx = new Transaction(_doc))
            {
                tx.Start("Mechanic engine");
                NumeratingElements();
                tx.Commit();
            }
        }
        public string GetName()
        {
            return "Mechanic numerator";
        }
        private void NumeratingElements()
        {
            try
            {
                _elementsForNumbering = GetListOfElementsForNumberign();
                List<string> listOfParameterValues = GetListOfParameterValues();

                int elementNumber = 0;

                foreach (Element element in _elementsForNumbering)
                {
                    if (_mechanicMapper.PrefixIsEnabled == true &&
                        _mechanicMapper.OverrideParameterValueIsEnabled == true)
                    {
                        element.get_Parameter(_mechanicMapper.BuiltInParameter).Set(_mechanicMapper.Prefix + " " + elementNumber.ToString());
                        elementNumber += 1;
                    }
                    if (_mechanicMapper.SystemAbbreviationIsEnabled == true &&
                        _mechanicMapper.OverrideParameterValueIsEnabled == true)
                    {
                        element.get_Parameter(_mechanicMapper.BuiltInParameter).Set(_mechanicMapper.SelectedSystemAbbreviation + " " + elementNumber.ToString());
                        elementNumber += 1;
                    }
                    if (_mechanicMapper.PrefixIsEnabled == true &&
                        _mechanicMapper.OverrideParameterValueIsEnabled == false &&
                        (GetParameterValue(element.get_Parameter(_mechanicMapper.BuiltInParameter)) == "" ||
                        GetParameterValue(element.get_Parameter(_mechanicMapper.BuiltInParameter)) == null))
                    {
                        while (listOfParameterValues.Contains(_mechanicMapper.Prefix + " " + elementNumber.ToString()))
                        {
                            elementNumber += 1;
                        }
                        element.get_Parameter(_mechanicMapper.BuiltInParameter).Set(_mechanicMapper.Prefix + " " + elementNumber.ToString());
                        elementNumber += 1;
                    }
                    if (_mechanicMapper.SystemAbbreviationIsEnabled == true &&
                        _mechanicMapper.OverrideParameterValueIsEnabled == false &&
                        (GetParameterValue(element.get_Parameter(_mechanicMapper.BuiltInParameter)) == "" ||
                        GetParameterValue(element.get_Parameter(_mechanicMapper.BuiltInParameter)) == null))
                    {
                        while (listOfParameterValues.Contains(_mechanicMapper.SelectedSystemAbbreviation + " " + elementNumber.ToString()))
                        {
                            elementNumber += 1;
                        }
                        element.get_Parameter(_mechanicMapper.BuiltInParameter).Set(_mechanicMapper.SelectedSystemAbbreviation + " " + elementNumber.ToString());
                        elementNumber += 1;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        private List<Element> GetListOfElementsForNumberign()
        {
            List<PairOfConnectedElements> pairsOfConnectedElements = GetConnectedElements();
            List<Element> listOfElementsForNumberign = new List<Element>();
            List<Element> listOfWaitingsElements = new List<Element>();

            Element firstElement = GetFirstElement();


            if (listOfElementsForNumberign.Count == 0)
            {
                listOfElementsForNumberign.Add(firstElement);
            }

            if (listOfElementsForNumberign.Count > 0)
            {
                for (int i = 0; i < pairsOfConnectedElements.Count; i++)
                {
                    if (listOfElementsForNumberign.Count - 1 < i)
                    {
                        if (listOfWaitingsElements.Count == 0)
                        {
                            break;
                        }
                        listOfElementsForNumberign.Add(listOfWaitingsElements[0]);
                        listOfWaitingsElements.Remove(listOfWaitingsElements[0]);
                    }
                    List<Element> filteredElements = new List<Element>();
                    List<PairOfConnectedElements> filteredPairs =
                    pairsOfConnectedElements.Where(p => (p.Element1.Id == listOfElementsForNumberign[i].Id) ||
                                                       (p.Element2.Id == listOfElementsForNumberign[i].Id)).ToList();


                    foreach (PairOfConnectedElements fp in filteredPairs)
                    {
                        if (!listOfElementsForNumberign.Where(e1 => e1.Id == fp.Element1.Id).Any())
                        {
                            filteredElements.Add(fp.Element1);
                        }
                        if (!listOfElementsForNumberign.Where(e => e.Id == fp.Element2.Id).Any())
                        {
                            filteredElements.Add(fp.Element2);
                        }
                    }

                    for (int j = 0; j < 1; j++)
                    {
                        if (filteredElements.Any())
                        {
                            listOfElementsForNumberign.Add(filteredElements[0]);
                        }
                    }
                    for (int j = 1; j < filteredElements.Count; j++)
                    {
                        listOfWaitingsElements.Add(filteredElements[j]);
                    }
                    filteredPairs.Clear();
                    filteredElements.Clear();
                }
            }
            return listOfElementsForNumberign;
        }
        private List<PairOfConnectedElements> GetConnectedElements()
        {
            List<PairOfConnectedConnectors> connectedConnectors = GetConnectedConnectors();
            List<PairOfConnectedElements> pairOfConnectedElements = new List<PairOfConnectedElements>();
            foreach (var connectedConnector in connectedConnectors)
            {
                Element element1 = connectedConnector.Connector1.Owner;
                Element element2 = connectedConnector.Connector2.Owner;
                pairOfConnectedElements.Add(new PairOfConnectedElements { Element1 = element1, Element2 = element2 });
            }
            return pairOfConnectedElements;
        }
        private Element GetFirstElement()
        {
            ElementId elementId = null;

            elementId = _uidoc.Selection.PickObject(ObjectType.Element, "Select element or ESC to reset the view").ElementId;
            Element element = _doc.GetElement(elementId);
            return element;
        }
        private List<PairOfConnectedConnectors> GetConnectedConnectors()
        {
            List<Connector> allMechanicalConnectors = GetMechanicalConnectors();
            List<PairOfConnectedConnectors> allPairOfConnectedConnectors = new List<PairOfConnectedConnectors>();
            if (allMechanicalConnectors.Any())
            {
                foreach (Connector connector1 in allMechanicalConnectors)
                {
                    Connector connector2 = allMechanicalConnectors.
                    Where(c => c.IsConnectedTo(connector1) && c.Owner.Id != connector1.Owner.Id).First();
                    if (!allPairOfConnectedConnectors.Where(p => (ReferenceEquals(p.Connector1, connector1) ||
                                                                ReferenceEquals(p.Connector1, connector2) ||
                                                                ReferenceEquals(p.Connector2, connector1) ||
                                                                ReferenceEquals(p.Connector2, connector2))).Any())
                    {
                        allPairOfConnectedConnectors.Add(new PairOfConnectedConnectors { Connector1 = connector1, Connector2 = connector2 });
                    }
                }
            }
            return allPairOfConnectedConnectors;
        }
        private List<Connector> GetMechanicalConnectors()
        {
            List<MEPElement> mechanicalElementList = GetMechanicalElements();
            List<Connector> connectors = new List<Connector>();
            foreach (var element in mechanicalElementList)
            {
                if (element.ElementGroup == "Ducts")
                {
                    foreach (Connector connector in ((Duct)element.Element).ConnectorManager.Connectors)
                    {
                        if (connector.IsConnected == true)
                        {
                            connectors.Add(connector);
                        }
                    }
                }
                else if (element.ElementGroup == "FlexDucts")
                {
                    foreach (Connector connector in ((FlexDuct)element.Element).ConnectorManager.Connectors)
                    {
                        if (connector.IsConnected == true)
                        {
                            connectors.Add(connector);
                        }
                    }
                }
                else if (element.ElementGroup == "FamilyInstances")
                {
                    foreach (Connector connector in ((FamilyInstance)element.Element).MEPModel.ConnectorManager.Connectors)
                    {
                        if (connector.IsConnected == true)
                        {
                            connectors.Add(connector);
                        }
                    }
                }
            }
            return connectors;
        }
        private List<MEPElement> GetMechanicalElements()
        {

            FilteredElementCollector ducts = new FilteredElementCollector(_doc)
                                                .OfClass(typeof(Duct));
            FilteredElementCollector flexDucts = new FilteredElementCollector(_doc)
                                               .OfClass(typeof(FlexDuct));

            ElementCategoryFilter ductFittingsFilter = new ElementCategoryFilter(BuiltInCategory.OST_DuctFitting);
            ElementCategoryFilter ductAccesoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_DuctAccessory);
            ElementCategoryFilter airTerminalsFilter = new ElementCategoryFilter(BuiltInCategory.OST_DuctTerminal);
            ElementCategoryFilter mechanicalEquipmentsFilter = new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment);

            IList<ElementFilter> categoryFilters = new List<ElementFilter>();
            categoryFilters.Add(ductFittingsFilter);
            categoryFilters.Add(ductAccesoryFilter);
            categoryFilters.Add(airTerminalsFilter);
            categoryFilters.Add(mechanicalEquipmentsFilter);
            LogicalOrFilter collectorFamilyInstance = new LogicalOrFilter(categoryFilters);

            FilteredElementCollector collectorMultiCategory = new FilteredElementCollector(_doc)
                                                            .OfClass(typeof(FamilyInstance))
                                                            .WherePasses(collectorFamilyInstance);
            List<MEPElement> allMechanicalElementList = new List<MEPElement>();
            foreach (Duct duct in ducts)
            {
                allMechanicalElementList.Add(new MEPElement { ElementGroup = "Ducts", Element = duct });
            }
            foreach (FlexDuct flexDuct in flexDucts)
            {
                allMechanicalElementList.Add(new MEPElement { ElementGroup = "FlexDucts", Element = flexDuct });
            }
            foreach (FamilyInstance fi in collectorMultiCategory)
            {
                if (fi.MEPModel.ConnectorManager != null)
                {
                    allMechanicalElementList.Add(new MEPElement { ElementGroup = "FamilyInstances", Element = fi });

                }
            }
            return allMechanicalElementList;
        }
        private List<string> GetListOfParameterValues()
        {
            List<string> listOfParameterValues = new List<string>();

            foreach (Element element in _elementsForNumbering)
            {
                listOfParameterValues.Add(GetParameterValue(element.get_Parameter(_mechanicMapper.BuiltInParameter)));
            }
            return listOfParameterValues;
        }
        private string GetParameterValue(Parameter parameter)
        {
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    return parameter.AsValueString();
                case StorageType.ElementId:
                    return parameter.AsElementId().ToString();
                case StorageType.Integer:
                    return parameter.AsValueString();
                case StorageType.None:
                    return parameter.AsValueString();
                case StorageType.String:
                    return parameter.AsString();
                default:
                    return "";
            }
        }


        public ObservableCollection<string> GetDuctSystemsAbbreviation(Document document)
        {
            ObservableCollection<string> ductuSystemAbbreviationList = new ObservableCollection<string>();
            FilteredElementCollector ductSystemAbbreviationFilter = new FilteredElementCollector(document).OfClass(typeof(MechanicalSystemType));
            foreach (MechanicalSystemType ds in ductSystemAbbreviationFilter)
            {
                if (!ductuSystemAbbreviationList.Contains(ds.Abbreviation))
                {
                    ductuSystemAbbreviationList.Add(ds.Abbreviation);
                }
            }
            return ductuSystemAbbreviationList;
        }
        public ObservableCollection<string> GetParameters()
        {
            ObservableCollection<string> mechanicalParametersList = new ObservableCollection<string>();
            mechanicalParametersList.Add("Comments");
            mechanicalParametersList.Add("Mark");
            return mechanicalParametersList;
        }

        public ObservableCollection<string> GetPipeSystemsAbbreviation(Document document)
        {
            ObservableCollection<string> pipesSystemAbbreviationList = new ObservableCollection<string>();
            FilteredElementCollector pipesSystemAbbreviationFilter = new FilteredElementCollector(document).OfClass(typeof(PipingSystemType));
            foreach (PipingSystemType ds in pipesSystemAbbreviationFilter)
            {
                if (!pipesSystemAbbreviationList.Contains(ds.Abbreviation))
                {
                    pipesSystemAbbreviationList.Add(ds.Abbreviation);
                }
            }
            return pipesSystemAbbreviationList;
        }
    }
}
