using System;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Resources
{
    public interface IListItem
    {
        object Id { get; }

        string Type { get; }

        string DisplayName { get; }

        DateTime ModifiedAt { get; }
    }
}