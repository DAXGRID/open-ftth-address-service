using FluentAssertions;
using OpenFTTH.Results;
using OpenFTTH.Address.API.Model;
using OpenFTTH.Address.API.Queries;
using OpenFTTH.Address.Business;
using OpenFTTH.Address.Business.Repository;
using OpenFTTH.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenFTTH.Address.Tests
{
    public class AddressServiceTests
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly IAddressRepository _addressRepository;


        public AddressServiceTests(IQueryDispatcher queryDispatcher, IAddressRepository addressRepository)
        {
            _queryDispatcher = queryDispatcher;
            _addressRepository = addressRepository;
        }

        [Fact]
        public async void QueryAccessAddressByInternalId_ShouldSucceed()
        {
            // Engum Møllevej 3, 7120 Vejle Ø (internal access address id)
            Guid sutAccessAddressId = Guid.Parse("02a0b95e-b7f1-4888-bd10-074ef49f196c");

            var getAddressInfoQuery = new GetAddressInfo(new Guid[] { sutAccessAddressId });

            var result = await _queryDispatcher.HandleAsync<GetAddressInfo, Result<GetAddressInfoResult>>(getAddressInfoQuery);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Value.AddressHits.Count().Should().Be(1);
            result.Value.AccessAddresses.Count().Should().Be(1);
            result.Value.UnitAddresses.Count().Should().Be(1);

            var addressHit = result.Value.AddressHits.First();

            addressHit.RefClass.Should().Be(AddressEntityClass.AccessAddress);

            var accessAddress = result.Value.AccessAddresses[addressHit.RefId];
            accessAddress.RoadName.Should().StartWith("Engum");
            accessAddress.HouseNumber.Should().Be("3");
            accessAddress.PostDistrict.Should().StartWith("Vejle");
            accessAddress.PostDistrictCode.Should().Be("7120");

            var unitAddress = result.Value.UnitAddresses.First();
            unitAddress.AccessAddressId.Should().Be(accessAddress.Id);
        }

        [Fact]
        public async void QueryNonExistingAccessAddress_ShouldReturnNoHits()
        {
            // Just a random guid
            Guid sutAccessAddressId = Guid.Parse("b0894287-409c-453d-93ee-03b9e66ff069");

            var getAddressInfoQuery = new GetAddressInfo(new Guid[] { sutAccessAddressId });

            var result = await _queryDispatcher.HandleAsync<GetAddressInfo, Result<GetAddressInfoResult>>(getAddressInfoQuery);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Value.AddressHits.Count().Should().Be(0);
            result.Value.AccessAddresses.Count().Should().Be(0);
            result.Value.UnitAddresses.Count().Should().Be(0);
        }

        [Fact]
        public async void QueryThreeAddressesByExternalId_ShouldSucceed()
        {
            // Engum Møllevej 3, 7120 Vejle Ø (external unit address id)
            Guid sutAddressId1 = Guid.Parse("0a3f50bc-aa89-32b8-e044-0003ba298018");

            // Vesterbrogade 7A, Hedensted (external access address id)
            Guid sutAddressId2 = Guid.Parse("0a3f508f-8504-32b8-e044-0003ba298018");

            // Rådhusgade 3 st, Horsens (external unit address id)
            Guid sutAddressId3 = Guid.Parse("3bc4989e-c838-4b42-bf43-cbb027587074");

            var getAddressInfoQuery = new GetAddressInfo(new Guid[] { sutAddressId1, sutAddressId2, sutAddressId3 });

            var result = await _queryDispatcher.HandleAsync<GetAddressInfo, Result<GetAddressInfoResult>>(getAddressInfoQuery);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Value.AddressHits.Count().Should().Be(3);
            result.Value.AccessAddresses.Count().Should().Be(3);
            result.Value.UnitAddresses.Count().Should().Be(3);

            result.Value.AddressHits.ContainsKey(sutAddressId1).Should().BeTrue();
            result.Value.AddressHits.ContainsKey(sutAddressId2).Should().BeTrue();
            result.Value.AddressHits.ContainsKey(sutAddressId3).Should().BeTrue();

            var engumHit = result.Value.AddressHits[sutAddressId1];
            var engumUnitAddress = result.Value.UnitAddresses[engumHit.RefId];
            var engumAccessAddress = result.Value.AccessAddresses[engumUnitAddress.AccessAddressId];

            engumAccessAddress.RoadName.Should().StartWith("Engum");
            engumAccessAddress.HouseNumber.Should().Be("3");
        }


        [Fact]
        public async void QueryNearest_ShouldSucceed()
        {
            if (!(_addressRepository is PostgresAddressRepository))
                return;

            var getAddressInfoQuery = new GetAddressInfo(9.658086, 55.742261, 4326, 3);

            var result = await _queryDispatcher.HandleAsync<GetAddressInfo, Result<GetAddressInfoResult>>(getAddressInfoQuery);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Value.AddressHits.Count().Should().Be(3);

        }
    }
}

