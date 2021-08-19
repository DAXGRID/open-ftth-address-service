using OpenFTTH.Core;
using System;

namespace OpenFTTH.Address.API.Model
{
    public interface IAddress : IIdentifiedObject
    {
        Guid? ExternalId { get; }
    }
}
