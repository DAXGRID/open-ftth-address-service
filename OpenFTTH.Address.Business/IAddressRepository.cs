using OpenFTTH.Address.API.Model;
using System;
using System.Collections.Generic;

namespace OpenFTTH.Address.Business
{
    public interface IAddressRepository
    {
        IEnumerable<(Guid,IAddress)> FetchAccessAndUnitAddressesByIds(Guid[] accessOrUnitAddressIds);

        IEnumerable<(double, IAddress)> FetchNearestAccessAndUnitAddresses(double x, double y, int srid, int maxHits);
    }
}
