namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Repository
{
    public interface IUnitsLoader
    {
        bool IsLoaded { get; }

        void LoadUnits(IRepository repository);

        string GetUnit(string type, string propertyName);

        bool TryGetUnit(string type, string propertyName, out string unit);
    }
}
