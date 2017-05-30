using GraphDataRepository.Server.BrightstarDb;
using StructureMap;

namespace GraphDataRepository.Utilities.StructureMap
{
    public class MainRegistry : Registry
    {
        public MainRegistry()
        {
            For<IBrightstarClient>().Use<BrightstarClient>();
        }
    }
}
