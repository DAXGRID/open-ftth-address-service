using OpenFTTH.Address.API.Model;
using OpenFTTH.Util;

namespace OpenFTTH.Address.API.Queries
{
    public record GetAddressInfoResult
    {
        public LookupCollection<AddressHit> AddressHits { get; }

        public LookupCollection<AccessAddress> AccessAddresses { get; }

        public LookupCollection<UnitAddress> UnitAddresses { get; }

        public GetAddressInfoResult(AddressHit[] addressHits, AccessAddress[] accessAddresses, UnitAddress[] unitAddresses)
        {
            AddressHits = new LookupCollection<AddressHit>(addressHits);
            AccessAddresses = new LookupCollection<AccessAddress>(accessAddresses);
            UnitAddresses = new LookupCollection<UnitAddress>(unitAddresses);
        }
    }
}
