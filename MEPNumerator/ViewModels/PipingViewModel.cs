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
    public class PipingViewModel : ViewModelBase, IPipingViewModel
    {
        private ObservableCollection<string> _systemAbbreviation;
        private ObservableCollection<string> _parameters;

        private IEventAggregator _eventAggregator;
        private IPipingRepository _pipingRepository;
        private IPipingMapper _pipingMapper;
        private IPipingEngine _pipingEngine;
        private BuiltInParameter _builtInParameter;
        private ExternalEvent _pipingExternalEvent;

        private bool _saveCanExecute;
        private ExternalCommandData _commandData;
        private UIApplication _uiapp;
        private UIDocument _uidoc;
        private Application _app;
        private Document _doc;

        public PipingViewModel(IEventAggregator eventAggregator,
            IPipingRepository pipingRepository,
            IPipingMapper pipingMapper,
            IPipingEngine pipingEngine)
        {
            _eventAggregator = eventAggregator;
            _pipingRepository = pipingRepository;
            _pipingMapper = pipingMapper;
            _pipingEngine = pipingEngine;

            _eventAggregator.GetEvent<StartApplicationLoadExternalCommandDataEvent>().Subscribe(OnLoadExternalCommandData);
            _eventAggregator.GetEvent<StartApplicationEvent>().Subscribe(OnLoadData);

            _pipingExternalEvent = ExternalEvent.Create(_pipingEngine as IExternalEventHandler);

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
            Prefix = _pipingRepository.GetAll()[0].Prefix;
            PrefixIsEnabled = _pipingRepository.GetAll()[0].PrefixIsEnabled;
            SelectedSystemAbbreviation = _pipingRepository.GetAll()[0].SelectedSystemAbbreviation;
            SystemAbbreviationIsEnabled = _pipingRepository.GetAll()[0].SystemAbbreviationIsEnabled;
            SelectedParameter = _pipingRepository.GetAll()[0].SelectedParameter;
            OverrideParametericIsEnabled = _pipingRepository.GetAll()[0].OverrideParameterValueIsEnabled;

            SaveCanExecute = false;
            SystemAbbreviaions = _pipingEngine.GetPipeSystemsAbbreviation(_doc);
            Parameters = _pipingEngine.GetParameters();
            if (SelectedSystemAbbreviation == "-")
            {
                SelectedSystemAbbreviation = _pipingEngine.GetPipeSystemsAbbreviation(_doc)[0];
            }
        }
        private void OnSaveExecute()
        {
            _pipingRepository.GetAll()[0].PrefixIsEnabled = _pipingMapper.PrefixIsEnabled;
            _pipingRepository.GetAll()[0].Prefix = _pipingMapper.Prefix;
            _pipingRepository.GetAll()[0].SystemAbbreviationIsEnabled = _pipingMapper.SystemAbbreviationIsEnabled;
            _pipingRepository.GetAll()[0].SelectedSystemAbbreviation = _pipingMapper.SelectedSystemAbbreviation;
            _pipingRepository.GetAll()[0].SelectedParameter = _pipingMapper.SelectedParameter;
            _pipingRepository.GetAll()[0].OverrideParameterValueIsEnabled = _pipingMapper.OverrideParameterValueIsEnabled;
            _pipingRepository.SaveAsync();
            SaveCanExecute = false;

        }
        private bool OnSaveCanExecute()
        {
            if (_pipingRepository.GetAll()[0].PrefixIsEnabled == _pipingMapper.PrefixIsEnabled &&
            _pipingRepository.GetAll()[0].Prefix == _pipingMapper.Prefix &&
            _pipingRepository.GetAll()[0].SystemAbbreviationIsEnabled == _pipingMapper.SystemAbbreviationIsEnabled &&
            _pipingRepository.GetAll()[0].SelectedSystemAbbreviation == _pipingMapper.SelectedSystemAbbreviation &&
            _pipingRepository.GetAll()[0].SelectedParameter == _pipingMapper.SelectedParameter &&
            _pipingRepository.GetAll()[0].OverrideParameterValueIsEnabled == _pipingMapper.OverrideParameterValueIsEnabled)
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
            _pipingExternalEvent.Raise();
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
            get { return "Piping"; }
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
        public bool PrefixIsEnabled
        {
            get
            {
                return _pipingMapper.PrefixIsEnabled;
            }
            set
            {
                _pipingMapper.PrefixIsEnabled = value;
                if (SystemAbbreviationIsEnabled == _pipingMapper.PrefixIsEnabled)
                {
                    SystemAbbreviationIsEnabled = !_pipingMapper.PrefixIsEnabled;
                }
                OnPropertyChanged();
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
        public string Prefix
        {
            get
            {
                return _pipingMapper.Prefix;
            }
            set
            {
                _pipingMapper.Prefix = value;
                OnPropertyChanged();
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
        public bool SystemAbbreviationIsEnabled
        {
            get
            {
                return _pipingMapper.SystemAbbreviationIsEnabled;
            }
            set
            {
                _pipingMapper.SystemAbbreviationIsEnabled = value;

                if (PrefixIsEnabled == SystemAbbreviationIsEnabled)
                {
                    PrefixIsEnabled = !_pipingMapper.SystemAbbreviationIsEnabled;
                }
                OnPropertyChanged();
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

            }
        }
        public ObservableCollection<string> SystemAbbreviaions
        {
            get
            {
                return _systemAbbreviation;
            }
            set
            {
                _systemAbbreviation = value;
                OnPropertyChanged();
            }
        }
        public string SelectedSystemAbbreviation
        {
            get
            {
                return _pipingMapper.SelectedSystemAbbreviation;
            }
            set
            {
                _pipingMapper.SelectedSystemAbbreviation = value;
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
                if (_pipingMapper.SelectedParameter == null)
                {
                    _pipingMapper.SelectedParameter = _pipingEngine.GetParameters()[0];
                }
                _parameters = value;
                OnPropertyChanged();
            }
        }
        public string SelectedParameter
        {
            get
            {
                return _pipingMapper.SelectedParameter;
            }
            set
            {

                _pipingMapper.SelectedParameter = value;
                OnPropertyChanged();
                if (value == "Comments")
                {
                    _builtInParameter = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;
                    _pipingMapper.BuiltInParameter = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;
                }
                else if (value == "Mark")
                {
                    _builtInParameter = BuiltInParameter.ALL_MODEL_MARK;
                    _pipingMapper.BuiltInParameter = BuiltInParameter.ALL_MODEL_MARK;
                }
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

            }
        }
        public bool OverrideParametericIsEnabled
        {
            get
            {
                return _pipingMapper.OverrideParameterValueIsEnabled;
            }
            set
            {
                _pipingMapper.OverrideParameterValueIsEnabled = value;
                OnPropertyChanged();
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

    }

}

