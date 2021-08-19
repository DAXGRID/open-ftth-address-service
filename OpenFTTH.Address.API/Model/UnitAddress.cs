using System;

namespace OpenFTTH.Address.API.Model
{
    public record UnitAddress : IAddress
    {
        public Guid Id { get; }
        public Guid AccessAddressId { get; }
        public Guid? ExternalId { get; init; }
        public string? FloorName { get; init; }
        public string? SuitName { get; init; }

        public string? Name => null;
        public string? Description => null;

        public UnitAddress(Guid id, Guid accessAddressId)
        {
            Id = id;
            AccessAddressId = accessAddressId;
        }
    }
}

