using Autodesk.Revit.UI;
using Autofac;
using MEPNumerator.Data.Repositories;
using MEPNumerator.DataAccess;
using MEPNumerator.Model.Mappers;
using MEPNumerator.Revit;
using MEPNumerator.ViewModels;
using Prism.Events;

namespace MEPNumerator.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            builder.RegisterType<MechanicRepository>().As<IMechanicRepository>();
            builder.RegisterType<ElectricRepository>().As<IElectricRepository>();
            builder.RegisterType<PipingRepository>().As<IPipingRepository>();

            builder.RegisterType<MechanicMapper>().As<IMechanicMapper>().SingleInstance();
            builder.RegisterType<ElectricMapper>().As<IElectricMapper>().SingleInstance();
            builder.RegisterType<PipingMapper>().As<IPipingMapper>().SingleInstance();

            builder.RegisterType<MechanicEngine>().As<IMechanicEngine>();
            builder.RegisterType<ElectricEngine>().As<IElectricEngine>();
            builder.RegisterType<PipingEngine>().As<IPipingEngine>();


            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<MainViewModel>().AsSelf();
            builder.RegisterType<MEPNumeratorDbContext>().AsSelf();
            builder.RegisterType<Command>().AsSelf();


            builder.RegisterType<MechanicViewModel>()
                .Keyed<IDetailViewModel>(nameof(MechanicViewModel));
            builder.RegisterType<ElectricViewModel>()
                .Keyed<IDetailViewModel>(nameof(ElectricViewModel));
            builder.RegisterType<PipingViewModel>()
                .Keyed<IDetailViewModel>(nameof(PipingViewModel));

            return builder.Build();
        }
    }
}
