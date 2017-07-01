using Libraries.Server.BrightstarDb;
using QualityGrapher.Globalization.Resources;
using StructureMap;

namespace QualityGrapher.Utilities.StructureMap
{
    public class MainRegistry : Registry
    {
        public MainRegistry()
        {
            For<IBrightstarClient>().Use<BrightstarClient>();

            //Singletons
            For<DynamicData>().Use<DynamicData>().Singleton();
        }
    }
}
