using log4net;
using StructureMap;

namespace GraphDataRepository.Utilities.StructureMap
{
    public class MainRegistry : Registry
    {
        public MainRegistry()
        {
            For<ILog>().Use(context => LogManager.GetLogger(context.ParentType ?? typeof(MainRegistry)));
        }
    }
}
