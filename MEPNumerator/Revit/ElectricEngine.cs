﻿using Autodesk.Revit.Attributes;
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
using Autodesk.Revit.DB.Electrical;

namespace MEPNumerator.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class ElectricEngine : IElectricEngine, IExternalEventHandler
    {
        private ExternalCommandData _commandData;
        private UIApplication _uiapp;
        private UIDocument _uidoc;
        private Autodesk.Revit.ApplicationServices.Application _app;
        private Document _doc;
        private List<Element> _elementsForNumbering;

        private IEventAggregator _eventAggregator;
        private IElectricMapper _electricMapper;

        public ElectricEngine(IEventAggregator eventAggregator, IElectricMapper electricMapper)
        {
            _eventAggregator = eventAggregator;
            _electricMapper = electricMapper;
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
                tx.Start("Electric engine");
                NumeratingElements();
                tx.Commit();
            }
        }
        public string GetName()
        {
            return "Electric numerator";
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
                    if (_electricMapper.OverrideParameterValueIsEnabled == true)
                    {
                        element.get_Parameter(_electricMapper.BuiltInParameter).Set(_electricMapper.Prefix + " " + elementNumber.ToString());
                        elementNumber += 1;
                    }

                    if (_electricMapper.OverrideParameterValueIsEnabled == false && 
                        (GetParameterValue(element.get_Parameter(_electricMapper.BuiltInParameter)) == "" ||
                        GetParameterValue(element.get_Parameter(_electricMapper.BuiltInParameter)) == null))
                    {
                        while (listOfParameterValues.Contains(_electricMapper.Prefix + " " + elementNumber.ToString()))
                        {
                            elementNumber += 1;
                        }
                        element.get_Parameter(_electricMapper.BuiltInParameter).Set(_electricMapper.Prefix + " " + elementNumber.ToString());
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
            List<Connector> allElectricConnectors = GetElectricConnectors();
            List<PairOfConnectedConnectors> allPairOfConnectedConnectors = new List<PairOfConnectedConnectors>();
            if (allElectricConnectors.Any())
            {
                foreach (Connector connector1 in allElectricConnectors)
                {
                    Connector connector2 = allElectricConnectors.
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
        private List<Connector> GetElectricConnectors()
        {
            List<MEPElement> electricElementList = GetElectricElements();
            List<Connector> connectors = new List<Connector>();
            foreach (var element in electricElementList)
            {
                if (element.ElementGroup == "CableTrays")
                {
                    foreach (Connector connector in ((CableTray)element.Element).ConnectorManager.Connectors)
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
        private List<MEPElement> GetElectricElements()
        {

            FilteredElementCollector cableTrays = new FilteredElementCollector(_doc)
                                                .OfClass(typeof(CableTray));

            ElementCategoryFilter cableTrayFittingsFilter = new ElementCategoryFilter(BuiltInCategory.OST_CableTrayFitting);
            ElementCategoryFilter electricalFixturesFilter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);
            ElementCategoryFilter electricalEquipmentsFilter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalEquipment);

            IList<ElementFilter> categoryFilters = new List<ElementFilter>();
            categoryFilters.Add(cableTrayFittingsFilter);
            categoryFilters.Add(electricalFixturesFilter);
            categoryFilters.Add(electricalEquipmentsFilter);
            LogicalOrFilter collectorFamilyInstance = new LogicalOrFilter(categoryFilters);

            FilteredElementCollector collectorMultiCategory = new FilteredElementCollector(_doc)
                                                            .OfClass(typeof(FamilyInstance))
                                                            .WherePasses(collectorFamilyInstance);
            List<MEPElement> allElectricalElementList = new List<MEPElement>();
            foreach (CableTray cableTray in cableTrays)
            {
                allElectricalElementList.Add(new MEPElement { ElementGroup = "CableTrays", Element = cableTray });
            }

            foreach (FamilyInstance fi in collectorMultiCategory)
            {
                if (fi.MEPModel.ConnectorManager != null)
                {
                    allElectricalElementList.Add(new MEPElement { ElementGroup = "FamilyInstances", Element = fi });

                }
            }
            return allElectricalElementList;
        }
        private List<string> GetListOfParameterValues()
        {
            List<string> listOfParameterValues = new List<string>();

            foreach (Element element in _elementsForNumbering)
            {
                listOfParameterValues.Add(GetParameterValue(element.get_Parameter(_electricMapper.BuiltInParameter)));
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


        public ObservableCollection<string> GetParameters()
        {
            ObservableCollection<string> mechanicalParametersList = new ObservableCollection<string>();
            mechanicalParametersList.Add("Comments");
            mechanicalParametersList.Add("Mark");
            return mechanicalParametersList;
        }

    }
}
