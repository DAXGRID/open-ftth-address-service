using NetTopologySuite.Geometries;
using System;

namespace OpenFTTH.Address.API.Model
{
    public record AccessAddress : IAddress
    {
        public Guid Id { get; }
        public Point AddressPoint { get; }
        public string? HouseHumber { get; init; }
        public string? PostDistrictCode { get; init; }
        public string? PostDistrict { get; init; }
        public Guid? ExternalId { get; init; }
        public string? RoadCode { get; init; }
        public string? RoadName { get; init; }
        public string? TownName { get; init; }
        public string? MunicipalCode { get; init; }

        public Guid[] UnitAddressIds { get; }

        public string? Name => null;
        public string? Description => null;

        public AccessAddress(Guid id, Point addressPoint, Guid[] unitAddressIds)
        {
            Id = id;
            AddressPoint = addressPoint;
            UnitAddressIds = unitAddressIds;
        }
    }
}
