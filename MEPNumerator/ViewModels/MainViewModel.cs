using Autofac.Features.Indexed;
using MEPNumerator.Event;
using Prism.Events;
using System;
using System.Collections.ObjectModel;

namespace MEPNumerator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        private IIndex<string, IDetailViewModel> _detailViewModelCreator;

        public MainViewModel(IIndex<string, IDetailViewModel> detailViewModelCreator)
        {
       
 
            _detailViewModelCreator = detailViewModelCreator;
            DetailViewModels = new ObservableCollection<IDetailViewModel>();
            DetailViewModels.Add(_detailViewModelCreator[nameof(MechanicViewModel)]);
            DetailViewModels.Add(_detailViewModelCreator[nameof(ElectricViewModel)]);
            DetailViewModels.Add(_detailViewModelCreator[nameof(PipingViewModel)]);
        }



        public IDetailViewModel DetailViewModel { get; }
        public ObservableCollection<IDetailViewModel> DetailViewModels { get; }
    }
}
