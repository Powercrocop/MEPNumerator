using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MEPNumerator.Data.Repositories;
using MEPNumerator.Event;
using MEPNumerator.Model.Mappers;
using MEPNumerator.Revit;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MEPNumerator.ViewModels
{
    public class ElectricViewModel : ViewModelBase, IElectricViewModel
    {
        private ObservableCollection<string> _parameters;

        private IEventAggregator _eventAggregator;
        private IElectricRepository _electricRepository;
        private IElectricMapper _electricMapper;
        private IElectricEngine _electricEngine;
        private BuiltInParameter _builtInParameter;
        private ExternalEvent _electricExternalEvent;

        private bool _saveCanExecute;
        private ExternalCommandData _commandData;
        private UIApplication _uiapp;
        private UIDocument _uidoc;
        private Application _app;
        private Document _doc;

        public ElectricViewModel(IEventAggregator eventAggregator,
            IElectricRepository electricRepository,
            IElectricMapper electricMapper,
            IElectricEngine electricEngine)
        {
            _eventAggregator = eventAggregator;
            _electricRepository = electricRepository;
            _electricMapper = electricMapper;
            _electricEngine = electricEngine;

            _eventAggregator.GetEvent<StartApplicationLoadExternalCommandDataEvent>().Subscribe(OnLoadExternalCommandData);
            _eventAggregator.GetEvent<StartApplicationEvent>().Subscribe(OnLoadData);

            _electricExternalEvent = ExternalEvent.Create(_electricEngine as IExternalEventHandler);

            SaveCommand = new DelegateCommand(OnSaveExecute, OnSaveCanExecute);
            RunCommand = new DelegateCommand(OnRunExecute, OnRunCanExecute);
            CancelCommand = new DelegateCommand(OnCancelExecute, OnCancelCanExecute);
        }
        private void OnLoadExternalCommandData(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _uiapp = commandData.Application;
            _uidoc = commandData.Application.ActiveUIDocument;
            _app = commandData.Application.Application;
            _doc = commandData.Application.ActiveUIDocument.Document;
        }
        private void OnLoadData()
        {
            Prefix = _electricRepository.GetAll()[0].Prefix;
            SelectedParameter = _electricRepository.GetAll()[0].SelectedParameter;
            OverrideParametericIsEnabled = _electricRepository.GetAll()[0].OverrideParameterValueIsEnabled;

            SaveCanExecute = false;
            Parameters = _electricEngine.GetParameters();
        }
        private void OnSaveExecute()
        {
            _electricRepository.GetAll()[0].Prefix = _electricMapper.Prefix;
            _electricRepository.GetAll()[0].SelectedParameter = _electricMapper.SelectedParameter;
            _electricRepository.GetAll()[0].OverrideParameterValueIsEnabled = _electricMapper.OverrideParameterValueIsEnabled;
            _electricRepository.SaveAsync();
            SaveCanExecute = false;

        }
        private bool OnSaveCanExecute()
        {
            if (_electricRepository.GetAll()[0].Prefix == _electricMapper.Prefix &&
            _electricRepository.GetAll()[0].SelectedParameter == _electricMapper.SelectedParameter &&
            _electricRepository.GetAll()[0].OverrideParameterValueIsEnabled == _electricMapper.OverrideParameterValueIsEnabled)
            {
                SaveCanExecute = false;
            }
            else
            {
                SaveCanExecute = true;
            }
            return SaveCanExecute;
        }
        private void OnRunExecute()
        {
            _electricExternalEvent.Raise();
            _eventAggregator.GetEvent<CloseMainWindowViewEvent>().Publish();

        }
        private bool OnRunCanExecute()
        {
            return true;
        }
        private void OnCancelExecute()
        {
            _eventAggregator.GetEvent<CloseMainWindowViewEvent>().Publish();
        }
        private bool OnCancelCanExecute()
        {
            return true;
        }
        public ICommand SaveCommand { get; }
        public ICommand RunCommand { get; }
        public ICommand CancelCommand { get; }
        public string Title
        {
            get { return "Electric"; }
        }
        public bool SaveCanExecute
        {
            get
            {
                return _saveCanExecute;
            }
            set
            {
                _saveCanExecute = value;
                OnPropertyChanged();
            }
        }
        public string Prefix
        {
            get
            {
                return _electricMapper.Prefix;
            }
            set
            {
                _electricMapper.Prefix = value;
                OnPropertyChanged();
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<string> Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                if (_electricMapper.SelectedParameter == null)
                {
                    _electricMapper.SelectedParameter = _electricEngine.GetParameters()[0];
                }
                _parameters = value;
                OnPropertyChanged();
            }
        }
        public string SelectedParameter
        {
            get
            {
                return _electricMapper.SelectedParameter;
            }
            set
            {

                _electricMapper.SelectedParameter = value;
                OnPropertyChanged();
                if (value == "Comments")
                {
                    _builtInParameter = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;
                    _electricMapper.BuiltInParameter = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;
                }
                else if (value == "Mark")
                {
                    _builtInParameter = BuiltInParameter.ALL_MODEL_MARK;
                    _electricMapper.BuiltInParameter = BuiltInParameter.ALL_MODEL_MARK;
                }
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

            }
        }
        public bool OverrideParametericIsEnabled
        {
            get
            {
                return _electricMapper.OverrideParameterValueIsEnabled;
            }
            set
            {
                _electricMapper.OverrideParameterValueIsEnabled = value;
                OnPropertyChanged();
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

    }
}

