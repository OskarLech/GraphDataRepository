using StructureMap;

namespace GraphDataRepository.Utilities.StructureMap
{
    public static class ObjectFactory
    {
        public static readonly IContainer Container = new Container(x =>
        {
            x.AddRegistry(new MainRegistry());
        });
    }
}
