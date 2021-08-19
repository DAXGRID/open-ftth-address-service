using OpenFTTH.Address.API.Model;
using System;
using System.Collections.Generic;

namespace OpenFTTH.Address.Business
{
    public interface IAddressRepository
    {
        IEnumerable<(Guid,IAddress)> FetchAccessAndUnitAddressesByIds(Guid[] accessOrUnitAddressIds);
    }
}
