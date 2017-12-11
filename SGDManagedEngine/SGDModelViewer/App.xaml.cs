using System.ComponentModel.Composition.Hosting;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Interop;
using Sce.Atf.Wpf.Models;
//using SGDManagedEngine;
using SGDRayTracer;

namespace WpfApp
{
    public partial class App : AtfApp
    {
        /// <summary>
        /// Gets MEF AggregateCatalog for application</summary>
        protected override AggregateCatalog GetCatalog()
        {
            // testing
            H1CPURayTracerSettings Settings = new H1CPURayTracerSettings();
            Settings.Width = 800;
            Settings.Height = 600;

            H1CPURayTracer RayTracer = new H1CPURayTracer(Settings);
            RayTracer.Render();

            var typeCatalog = new TypeCatalog(
                typeof(MainWindow)             // Application's main window
                //typeof(SGDGameLoopService),
                //typeof(SGDGameEngineProxy)
                );

            return new AggregateCatalog(typeCatalog, StandardInteropParts.Catalog, StandardViewModels.Catalog);
        }

        protected override void OnCompositionComplete()
        {

        }
    }
}