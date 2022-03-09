using FluentResults;
using Microsoft.Extensions.Logging;
using OpenFTTH.Address.API.Model;
using OpenFTTH.Address.API.Queries;
using OpenFTTH.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenFTTH.Address.Business.QueryHandling
{
    public class GetAddressInfoQueryHandler
        : IQueryHandler<GetAddressInfo, Result<GetAddressInfoResult>>
    {
        private readonly ILogger<GetAddressInfoQueryHandler> _logger;
        private readonly IAddressRepository _addressRepository;

        public GetAddressInfoQueryHandler(ILogger<GetAddressInfoQueryHandler> logger, IAddressRepository addressRepository)
        {
            _logger = logger;
            _addressRepository = addressRepository;
        }

        public Task<Result<GetAddressInfoResult>> HandleAsync(GetAddressInfo query)
        {
            if (query.AccessOrUnitAddressIds != null)
            {
                return Task.FromResult(GetAddressesByIds(query));
            }
            else if (query.NearestAddressSearchX > 0)
            {
                return Task.FromResult(GetNearestAddressesByCoordinate(query));
            }
            else
                return Task.FromResult(Result.Fail<GetAddressInfoResult>("Invalid query parameters. Must either contain a list of address ids or coordinate information for nearest address search"));
        }

        public Result<GetAddressInfoResult> GetAddressesByIds(GetAddressInfo query)
        {
            if (query.AccessOrUnitAddressIds == null)
                return Result.Fail("Please don't call GetAddressByIds if AccessOrUnitAddressIds is null");

            var addressIds = RemoveDublicatedIds(query.AccessOrUnitAddressIds);

            var addressSearchResult = _addressRepository.FetchAccessAndUnitAddressesByIds(addressIds);

            List<AddressHit> hits = new();

            Dictionary<Guid, AccessAddress> accessAddresses = new();
            Dictionary<Guid, UnitAddress> unitAddresses = new();

            foreach (var addressSearchItem in addressSearchResult)
            {
                if (addressSearchItem.Item1 != Guid.Empty)
                {
                    hits.Add(new AddressHit()
                    {
                        Key = addressSearchItem.Item1,
                        RefClass = addressSearchItem.Item2 is AccessAddress ? AddressEntityClass.AccessAddress : AddressEntityClass.UnitAddress,
                        RefId = addressSearchItem.Item2.Id
                    });
                }

                if (addressSearchItem.Item2 is AccessAddress && !accessAddresses.ContainsKey(addressSearchItem.Item2.Id))
                    accessAddresses.Add(addressSearchItem.Item2.Id, (AccessAddress)addressSearchItem.Item2);
                else if (addressSearchItem.Item2 is UnitAddress && !unitAddresses.ContainsKey(addressSearchItem.Item2.Id))
                    unitAddresses.Add(addressSearchItem.Item2.Id, (UnitAddress)addressSearchItem.Item2);
            }

            return Result.Ok<GetAddressInfoResult>(new GetAddressInfoResult(hits.ToArray(), accessAddresses.Values.ToArray(), unitAddresses.Values.ToArray()));
        }

        public Result<GetAddressInfoResult> GetNearestAddressesByCoordinate(GetAddressInfo query)
        {
            if (query.NearestAddressSearchX == 0)
                return Result.Fail("NearestAddressSearchX must be > 0");

            if (query.NearestAddressSearchY == 0)
                return Result.Fail("NearestAddressSearchY must be > 0");

            if (query.NearestAddressSearchSrid == 0)
                return Result.Fail("NearestAddressSearchSrid must be > 0");


            var addressSearchResult = _addressRepository.FetchNearestAccessAndUnitAddresses(query.NearestAddressSearchX, query.NearestAddressSearchY, query.NearestAddressSearchSrid, query.NearestAddressSearchMaxHits);

            List<AddressHit> hits = new();

            Dictionary<Guid, AccessAddress> accessAddresses = new();
            Dictionary<Guid, UnitAddress> unitAddresses = new();

            foreach (var addressSearchItem in addressSearchResult)
            {
                if (addressSearchItem.Item1 >= 0)
                {
                    hits.Add(new AddressHit()
                    {
                        Key = addressSearchItem.Item2.Id,
                        Distance = addressSearchItem.Item1,
                        RefClass = addressSearchItem.Item2 is AccessAddress ? AddressEntityClass.AccessAddress : AddressEntityClass.UnitAddress,
                        RefId = addressSearchItem.Item2.Id
                    });
                }

                if (addressSearchItem.Item2 is AccessAddress && !accessAddresses.ContainsKey(addressSearchItem.Item2.Id))
                    accessAddresses.Add(addressSearchItem.Item2.Id, (AccessAddress)addressSearchItem.Item2);
                else if (addressSearchItem.Item2 is UnitAddress && !unitAddresses.ContainsKey(addressSearchItem.Item2.Id))
                    unitAddresses.Add(addressSearchItem.Item2.Id, (UnitAddress)addressSearchItem.Item2);
            }

            return Result.Ok<GetAddressInfoResult>(new GetAddressInfoResult(hits.ToArray(), accessAddresses.Values.ToArray(), unitAddresses.Values.ToArray()));
        }

        private static Guid[] RemoveDublicatedIds(Guid[] accessOrUnitAddressIds)
        {
            HashSet<Guid> result = new();
        
            foreach (var addressId in accessOrUnitAddressIds)
            {
                if (!result.Contains(addressId))
                    result.Add(addressId);
            }

            return result.ToArray();
        }
    }
}
