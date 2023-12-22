using System;

namespace OpenFTTH.Address.API.Model
{
    public record UnitAddress : IAddress
    {
        public Guid Id { get; }
        public Guid AccessAddressId { get; }
        public Guid? ExternalId { get; }
        public string? FloorName { get; }
        public string? SuitName { get; }

        public string? Name => null;
        public string? Description => null;

        public UnitAddress(Guid id, Guid accessAddressId, Guid? externalId, string? floorName, string? suitName)
        {
            Id = id;
            AccessAddressId = accessAddressId;
            ExternalId = externalId;
            FloorName = floorName;
            SuitName = suitName;

            if (floorName != null && floorName.Trim() == "")
                FloorName = null;

            if (SuitName != null && SuitName.Trim() == "")
                SuitName = null;
        }
    }
}

