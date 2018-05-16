using STARS.Applications.VETS.Plugins.Enterprise.Common.Resources;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Repository
{
    public interface IResourceTreeLoader
    {
        PropertyBag GetResource(IRepository repository, IListItem item);
    }
}
