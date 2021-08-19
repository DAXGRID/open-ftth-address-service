using FluentResults;
using OpenFTTH.CQRS;
using System;

namespace OpenFTTH.Address.API.Queries
{
    public class GetAddressInfo : IQuery<Result<GetAddressInfoResult>>
    {
        public static string RequestName => typeof(GetAddressInfo).Name;
        public Guid[] AccessOrUnitAddressIds { get; }

        public GetAddressInfo(Guid[] accessOrUnitAddressIds)
        {
            AccessOrUnitAddressIds = accessOrUnitAddressIds;
        }
    }
}
