namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Repository
{
    public interface ICondition
    {
        string Property { get; }

        string GetRequestStringPart();
    }
}
