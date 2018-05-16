namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Repository
{
    public interface IPropertyBagExt
    {
        string Property { get; }

        string GetRequestStringPart();
    }
}
