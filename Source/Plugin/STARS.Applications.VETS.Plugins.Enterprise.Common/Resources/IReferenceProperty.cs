namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Resources
{
    public interface IReferenceProperty
    {
        string Type { get; }

        string Id { get; }
        
        string DisplayName { get; }

        bool IsEmpty { get; }
    }
}
