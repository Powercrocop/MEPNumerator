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
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MEPNumerator.ViewModels
{
    public class MechanicViewModel : ViewModelBase, IMechanicViewModel
    {
        private ObservableCollection<string> _systemAbbreviation;
        private ObservableCollection<string> _parameters;

        private IEventAggregator _eventAggregator;
        private IMechanicRepository _mechanicRepository;
        private IMechanicMapper _mechanicMapper;
        private IMechanicEngine _mechanicEngine;
        private BuiltInParameter _builtInParameter;
        private ExternalEvent _mechanicExternalEvent;

        private bool _saveCanExecute;
        private ExternalCommandData _commandData;
        private UIApplication _uiapp;
        private UIDocument _uidoc;
        private Application _app;
        private Document _doc;

        public MechanicViewModel(IEventAggregator eventAggregator,
            IMechanicRepository mechanicRepository,
            IMechanicMapper mechanicMapper,
            IMechanicEngine mechanicEngine)
        {
            _eventAggregator = eventAggregator;
            _mechanicRepository = mechanicRepository;
            _mechanicMapper = mechanicMapper;
            _mechanicEngine = mechanicEngine;

            _eventAggregator.GetEvent<StartApplicationLoadExternalCommandDataEvent>().Subscribe(OnLoadExternalCommandData);
            _eventAggregator.GetEvent<StartApplicationEvent>().Subscribe(OnLoadData);
            
            _mechanicExternalEvent = ExternalEvent.Create(_mechanicEngine as IExternalEventHandler);

            SaveCommand = new DelegateCommand(OnSaveExecute, OnSaveCanExecute);
            RunCommand = new DelegateCommand(OnRunExecute, OnRunCanExecute);
            CancelCommand = new DelegateCommand(OnCancelExecute, OnCancelCanExecute);
        }
        private void OnLoadExternalCommandData (ExternalCommandData commandData)
        {
            _commandData = commandData;
            _uiapp = commandData.Application;
            _uidoc = commandData.Application.ActiveUIDocument;
            _app = commandData.Application.Application;
            _doc = commandData.Application.ActiveUIDocument.Document;
        }
        private void OnLoadData()
        {
            Prefix = _mechanicRepository.GetAll()[0].Prefix;
            PrefixIsEnabled = _mechanicRepository.GetAll()[0].PrefixIsEnabled;
            SelectedSystemAbbreviation = _mechanicRepository.GetAll()[0].SelectedSystemAbbreviation;
            SystemAbbreviationIsEnabled = _mechanicRepository.GetAll()[0].SystemAbbreviationIsEnabled;
            SelectedParameter = _mechanicRepository.GetAll()[0].SelectedParameter;
            OverrideParametericIsEnabled = _mechanicRepository.GetAll()[0].OverrideParameterValueIsEnabled;

            SaveCanExecute = false;
            SystemAbbreviaions = _mechanicEngine.GetDuctSystemsAbbreviation(_doc);
            Parameters = _mechanicEngine.GetParameters();
            if (SelectedSystemAbbreviation == "-")
            {
                SelectedSystemAbbreviation = _mechanicEngine.GetDuctSystemsAbbreviation(_doc)[0];
            }
        }
        private void OnSaveExecute()
        {
            _mechanicRepository.GetAll()[0].PrefixIsEnabled = _mechanicMapper.PrefixIsEnabled;
            _mechanicRepository.GetAll()[0].Prefix = _mechanicMapper.Prefix;
            _mechanicRepository.GetAll()[0].SystemAbbreviationIsEnabled = _mechanicMapper.SystemAbbreviationIsEnabled;
            _mechanicRepository.GetAll()[0].SelectedSystemAbbreviation = _mechanicMapper.SelectedSystemAbbreviation;
            _mechanicRepository.GetAll()[0].SelectedParameter = _mechanicMapper.SelectedParameter;
            _mechanicRepository.GetAll()[0].OverrideParameterValueIsEnabled = _mechanicMapper.OverrideParameterValueIsEnabled;
            _mechanicRepository.SaveAsync();
            SaveCanExecute = false;

        }
        private bool OnSaveCanExecute()
        {
            if (_mechanicRepository.GetAll()[0].PrefixIsEnabled == _mechanicMapper.PrefixIsEnabled &&
            _mechanicRepository.GetAll()[0].Prefix == _mechanicMapper.Prefix &&
            _mechanicRepository.GetAll()[0].SystemAbbreviationIsEnabled == _mechanicMapper.SystemAbbreviationIsEnabled &&
            _mechanicRepository.GetAll()[0].SelectedSystemAbbreviation == _mechanicMapper.SelectedSystemAbbreviation &&
            _mechanicRepository.GetAll()[0].SelectedParameter == _mechanicMapper.SelectedParameter &&
            _mechanicRepository.GetAll()[0].OverrideParameterValueIsEnabled == _mechanicMapper.OverrideParameterValueIsEnabled)
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
            _mechanicExternalEvent.Raise();
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
            get { return "Mechanic"; }
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
                return _mechanicMapper.PrefixIsEnabled;
            }
            set
            {
                _mechanicMapper.PrefixIsEnabled = value;
                if (SystemAbbreviationIsEnabled == _mechanicMapper.PrefixIsEnabled)
                {
                    SystemAbbreviationIsEnabled = !_mechanicMapper.PrefixIsEnabled;
                }
                OnPropertyChanged();
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
        public string Prefix
        {
            get
            {
                return _mechanicMapper.Prefix;
            }
            set
            {
                _mechanicMapper.Prefix = value;
                OnPropertyChanged();
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
        public bool SystemAbbreviationIsEnabled
        {
            get
            {
                return _mechanicMapper.SystemAbbreviationIsEnabled;
            }
            set
            {
                _mechanicMapper.SystemAbbreviationIsEnabled = value;

                if (PrefixIsEnabled == SystemAbbreviationIsEnabled)
                {
                    PrefixIsEnabled = !_mechanicMapper.SystemAbbreviationIsEnabled;
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
                return _mechanicMapper.SelectedSystemAbbreviation;
            }
            set
            {
                _mechanicMapper.SelectedSystemAbbreviation = value;
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
                if (_mechanicMapper.SelectedParameter == null)
                {
                    _mechanicMapper.SelectedParameter = _mechanicEngine.GetParameters()[0];
                }
                _parameters = value;
                OnPropertyChanged();
            }
        }
        public string SelectedParameter
        {
            get 
            {
                return _mechanicMapper.SelectedParameter; 
            }
            set 
            {

                _mechanicMapper.SelectedParameter = value;
                OnPropertyChanged();
                if (value == "Comments")
                {
                    _builtInParameter = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;
                    _mechanicMapper.BuiltInParameter = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;
                }
                else if (value == "Mark")
                {
                    _builtInParameter = BuiltInParameter.ALL_MODEL_MARK;
                    _mechanicMapper.BuiltInParameter = BuiltInParameter.ALL_MODEL_MARK;
                }
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

            }
        }
        public bool OverrideParametericIsEnabled
        {
            get
            {
                return _mechanicMapper.OverrideParameterValueIsEnabled;
            }
            set
            {
                _mechanicMapper.OverrideParameterValueIsEnabled = value;
                OnPropertyChanged();
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

    }
}
