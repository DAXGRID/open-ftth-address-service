using OpenFTTH.Core;
using System;

namespace OpenFTTH.Address.API.Model
{
    public class AddressHit : IIdentifiedObject
    {
        public Guid Key { get; set; }
        public Guid RefId { get; set; }
        public AddressEntityClass RefClass { get; set; }

        public Guid Id => Key;
        public string? Name => null;
        public string? Description => null;

    }
}
