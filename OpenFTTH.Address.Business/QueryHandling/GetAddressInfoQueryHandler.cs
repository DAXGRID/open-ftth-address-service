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
            var addressSearchResult = _addressRepository.FetchAccessAndUnitAddressesByIds(query.AccessOrUnitAddressIds);

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

            var result = Result.Ok<GetAddressInfoResult>(new GetAddressInfoResult(hits.ToArray(), accessAddresses.Values.ToArray(), unitAddresses.Values.ToArray()));

            return Task.FromResult(result);
        }



    }
}
