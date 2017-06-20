using StructureMap;

namespace QualityGrapher.Utilities.StructureMap
{
    public static class ObjectFactory
    {
        public static readonly IContainer Container = new Container(x =>
        {
            x.AddRegistry(new MainRegistry());
        });
    }
}
