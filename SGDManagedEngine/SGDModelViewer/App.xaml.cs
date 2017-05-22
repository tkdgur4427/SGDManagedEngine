using System.ComponentModel.Composition.Hosting;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Interop;
using Sce.Atf.Wpf.Models;
using SGDManagedEngine;

namespace WpfApp
{
    public partial class App : AtfApp
    {
        /// <summary>
        /// Gets MEF AggregateCatalog for application</summary>
        protected override AggregateCatalog GetCatalog()
        {
            var typeCatalog = new TypeCatalog(
                typeof(MainWindow),             // Application's main window
                typeof(SGDGameLoopService),
                typeof(SGDGameEngineProxy)
                );

            return new AggregateCatalog(typeCatalog, StandardInteropParts.Catalog, StandardViewModels.Catalog);
        }

        protected override void OnCompositionComplete()
        {

        }
    }
}