using OpenFTTH.Results;
using OpenFTTH.CQRS;
using System;

namespace OpenFTTH.Address.API.Queries
{
    public class GetAddressInfo : IQuery<Result<GetAddressInfoResult>>
    {
        public static string RequestName => typeof(GetAddressInfo).Name;
        public Guid[]? AccessOrUnitAddressIds { get; }
        public double NearestAddressSearchX { get; }
        public double NearestAddressSearchY { get; }
        public int NearestAddressSearchSrid { get; }
        public int NearestAddressSearchMaxHits { get; }

        /// <summary>
        /// Use this to retrieve access and unit addresse by ids.
        /// The ids can be an internal or external access and unit address id.
        /// </summary>
        /// <param name="accessOrUnitAddressIds"></param>
        public GetAddressInfo(Guid[] accessOrUnitAddressIds)
        {
            AccessOrUnitAddressIds = accessOrUnitAddressIds;
        }

        public GetAddressInfo(double x, double y, int srid, int maxHits)
        {
            NearestAddressSearchX = x;
            NearestAddressSearchY = y;
            NearestAddressSearchSrid = srid;
            NearestAddressSearchMaxHits = maxHits;
        }
    }
}
