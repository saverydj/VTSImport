using System.Collections.Generic;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Resources;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Repository
{
    public interface IDeserializer
    {
        PropertyBag Deserialize(string type, object id, string response, IUnitsLoader unitsLoader = null);

        IEnumerable<IListItem> DeserializeList(string response);

        IEnumerable<KeyValuePair<string, string>> DeserializeUnits(string response);
    }
}
