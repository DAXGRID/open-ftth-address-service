using OpenFTTH.Address.API.Model;
using System;
using System.Collections.Generic;

namespace OpenFTTH.Address.Business.Repository
{
    /// <summary>
    /// Just for testing
    /// </summary>
    public class InMemAddressRepository : IAddressRepository
    {
        public Dictionary<Guid, AccessAddress> _accessAddressesById = new();
        public Dictionary<Guid, AccessAddress> _accessAddressesByExternalId = new();

        public Dictionary<Guid, UnitAddress> _unitAddressesById = new();
        public Dictionary<Guid, UnitAddress> _unitAddressesByExternalId = new();

        public InMemAddressRepository(IEnumerable<IAddress> addresses)
        {
            AddAddresses(addresses);
        }

        public void AddAddresses(IEnumerable<IAddress> addresses)
        {
            foreach (var address in addresses)
            {
                switch (address)
                {
                    case AccessAddress accessAddress:
                        AddAccessAddress(accessAddress);
                        break;

                    case UnitAddress unitAddress:
                        AddUnitAddress(unitAddress);
                        break;
                }
            }
        }

        public void AddAccessAddress(AccessAddress accessAddress)
        {
            _accessAddressesById.Add(accessAddress.Id, accessAddress);

            if (accessAddress.ExternalId != null)
                _accessAddressesByExternalId.Add(accessAddress.ExternalId.Value, accessAddress);
        }

        public void AddUnitAddress(UnitAddress unitAddress)
        {
            _unitAddressesById.Add(unitAddress.Id, unitAddress);

            if (unitAddress.ExternalId != null)
                _unitAddressesByExternalId.Add(unitAddress.ExternalId.Value, unitAddress);
        }

        public IEnumerable<(Guid,AccessAddress)> FetchAccessAddressesByIds(Guid[] accessOrUnitAddressIds)
        {
            List<(Guid,AccessAddress)> result = new();

            foreach (var id in accessOrUnitAddressIds)
            {
                if (_accessAddressesById.TryGetValue(id, out AccessAddress? accessAddressById))
                {
                    result.Add((id, accessAddressById));
                }
                else if (_accessAddressesByExternalId.TryGetValue(id, out AccessAddress? accessAddressByExternalId))
                {
                    result.Add((id, accessAddressByExternalId));
                }
                else if (_unitAddressesById.TryGetValue(id, out UnitAddress? unitAddressById))
                {
                    var accessAddress = _accessAddressesById[unitAddressById.AccessAddressId];

                    result.Add((id, accessAddress));
                }
                else if (_unitAddressesByExternalId.TryGetValue(id, out UnitAddress? unitAddressByExternalId))
                {
                    var accessAddress = _accessAddressesById[unitAddressByExternalId.AccessAddressId];

                    result.Add((id, accessAddress));
                }
            }

            return result;
        }

        public IEnumerable<(Guid, IAddress)> FetchAccessAndUnitAddressesByIds(Guid[] accessOrUnitAddressIds)
        {
            List<(Guid, IAddress)> result = new();

            HashSet<Guid> addedAddresses = new();

            foreach (var id in accessOrUnitAddressIds)
            {
                if (_accessAddressesById.TryGetValue(id, out AccessAddress? accessAddressById))
                {
                    if (!addedAddresses.Contains(accessAddressById.Id))
                        addedAddresses.Add(accessAddressById.Id);

                    result.Add((id, accessAddressById));
                    AddRelatedUnitAddresses(result, addedAddresses, accessAddressById);
                }
                else if (_accessAddressesByExternalId.TryGetValue(id, out AccessAddress? accessAddressByExternalId))
                {
                    if (!addedAddresses.Contains(accessAddressByExternalId.Id))
                        addedAddresses.Add(accessAddressByExternalId.Id);

                    result.Add((id, accessAddressByExternalId));
                    AddRelatedUnitAddresses(result, addedAddresses, accessAddressByExternalId);
                }
                else if (_unitAddressesById.TryGetValue(id, out UnitAddress? unitAddressById))
                {
                    if (!addedAddresses.Contains(unitAddressById.Id))
                        addedAddresses.Add(unitAddressById.Id);

                    result.Add((id, unitAddressById));

                    AddRelatedAccessAddress(result, addedAddresses, unitAddressById);
                }
                else if (_unitAddressesByExternalId.TryGetValue(id, out UnitAddress? unitAddressByExternalId))
                {
                    if (!addedAddresses.Contains(unitAddressByExternalId.Id))
                        addedAddresses.Add(unitAddressByExternalId.Id);

                    result.Add((id, unitAddressByExternalId));

                    AddRelatedAccessAddress(result, addedAddresses, unitAddressByExternalId);
                }
            }

            return result;
        }

        private void AddRelatedUnitAddresses(List<(Guid, IAddress)> result, HashSet<Guid> addedAddresses, AccessAddress accessAddressById)
        {
            foreach (var unitAddressId in accessAddressById.UnitAddressIds)
            {
                result.Add((Guid.Empty, _unitAddressesById[unitAddressId]));
            }
        }

        private void AddRelatedAccessAddress(List<(Guid, IAddress)> result, HashSet<Guid> addedAddresses, UnitAddress unitAddress)
        {
            result.Add((Guid.Empty, _accessAddressesById[unitAddress.AccessAddressId]));
        }

        public IEnumerable<(double, IAddress)> FetchNearestAccessAndUnitAddresses(double x, double y, int srid, int maxHits)
        {
            throw new NotImplementedException();
        }
    }
}
