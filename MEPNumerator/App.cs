#region Namespaces
using Autodesk.Revit.UI;
using MEPNumerator.Data.Repositories;
using MEPNumerator.DataAccess;
using MEPNumerator.Model.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace MEPNumerator
{
    class App : IExternalApplication
    {
        static string AddInPath = typeof(App).Assembly.Location;
        public Result OnStartup(UIControlledApplication application)
        {
            CreateDatabase();
            try
            {
                // create a Ribbon panel which contains three 
                // stackable buttons and one single push button

                string firstPanelName = "MEPNumerator";
                RibbonPanel panel = application.CreateRibbonPanel(firstPanelName);

                // set the information about the command we will 
                // be assigning to the button 


                PushButtonData pushButtonData = new PushButtonData(
                  "MEPNumerator",
                  "MEPNumerator",
                  AddInPath,
                  "MEPNumerator.Revit.Command");

                //' add a button to the panel 

                PushButton pushButton = panel.AddItem(pushButtonData)
                  as PushButton;

                //' add an icon 

                pushButton.LargeImage = LoadPNGImageFromResource(
                "MEPNumerator.Icon.MEPNumerator.png");

                // add a tooltip 
                pushButton.ToolTip =
                  "This add-in will help you number the elements in model";

                // long description

                pushButton.LongDescription =
                    "The add-in numbers the connected elements belonging to the mechanical electrical and piping branches. Before starting the application, you need to specify the prefix and the parameter to be filled in. Numbering will start after clicking on the first element to be marked.";

                // Context (F1) Help - new in 2013 
                //string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // %AppData% 

                //string path;

                //path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                //    + @"\Autodesk\ApplicationPlugins\MEPNumerator.bundle\Contents\help.html";

                //ContextualHelp contextHelp = new ContextualHelp(
                //    ContextualHelpType.ChmFile,
                //    path); // hard coding for simplicity. 

                //pushButton.SetContextualHelp(contextHelp);

                return Autodesk.Revit.UI.Result.Succeeded;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "MEPNumerator Ribbon");
                return Autodesk.Revit.UI.Result.Failed;
            }

        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }


        private void CreateDatabase()
        {
            using (var dbContext = new MEPNumeratorDbContext())
            {
                dbContext.Database.EnsureCreated();

                if (!dbContext.Mechanics.Any<Mechanic>())
                {
                    dbContext.Mechanics.Add(new Mechanic()
                    {
                        PrefixIsEnabled = true,
                        Prefix = "-",
                        SystemAbbreviationIsEnabled = false,
                        SelectedSystemAbbreviation = "-",
                        SelectedParameter = "Comments",
                        OverrideParameterValueIsEnabled = false
                    });
                    MechanicRepository mechanicRepository = new MechanicRepository(dbContext);
                    mechanicRepository.SaveAsync();


                    if (!dbContext.Electrics.Any<Electric>())
                    {
                        dbContext.Electrics.Add(new Electric()
                        {
                            Prefix = "-",
                            SelectedParameter = "Comments",
                            OverrideParameterValueIsEnabled = false
                        });
                        ElectricRepository electricRepository = new ElectricRepository(dbContext);
                        electricRepository.SaveAsync();
                    }

                    if (!dbContext.Pipings.Any<Piping>())
                    {
                        dbContext.Pipings.Add(new Piping()
                        {
                            PrefixIsEnabled = true,
                            Prefix = "-",
                            SystemAbbreviationIsEnabled = false,
                            SelectedSystemAbbreviation = "-",
                            SelectedParameter = "Comments",
                            OverrideParameterValueIsEnabled = false
                        });
                        PipingRepository pipingRepository = new PipingRepository(dbContext);
                        pipingRepository.SaveAsync();
                    }
                }

            }
        }

         private static System.Windows.Media.ImageSource LoadPNGImageFromResource(string imageResourceName)
        {
            System.Reflection.Assembly dotNetAssembly =
              System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream iconStream =
              dotNetAssembly.GetManifestResourceStream(imageResourceName);
            System.Windows.Media.Imaging.PngBitmapDecoder bitmapDecoder =
              new System.Windows.Media.Imaging.PngBitmapDecoder(iconStream,
                System.Windows.Media.Imaging.BitmapCreateOptions.
                PreservePixelFormat, System.Windows.Media.Imaging.
                BitmapCacheOption.Default);
            System.Windows.Media.ImageSource imageSource =
              bitmapDecoder.Frames[0];
            return imageSource;
        }

    }
}
