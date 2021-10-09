using FluentAssertions;
using OpenFTTH.Address.API.Model;
using OpenFTTH.Address.Business;
using OpenFTTH.Address.Business.Repository;
using System;
using System.Linq;
using Xunit;

namespace OpenFTTH.Address.Tests
{
    public class AddressRespositoryTests
    {
        private readonly IAddressRepository _addressRepository;

        public AddressRespositoryTests(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        [Fact]
        public void QueryAccessAddresses_ByInternalAccessAddressId_ShouldSucceed()
        {
            var key = Guid.Parse("02a0b95e-b7f1-4888-bd10-074ef49f196c");

            var result = _addressRepository.FetchAccessAndUnitAddressesByIds(new Guid[] { key }).ToList();

            // Assert
            result.Count.Should().Be(2);

            var accessAddress = result.Find(r => r.Item1 == key).Item2 as AccessAddress;

            accessAddress.RoadName.Should().StartWith("Engum");
            accessAddress.HouseNumber.Should().Be("3");
            accessAddress.UnitAddressIds.Count().Should().Be(1);
        }

        [Fact]
        public void QueryAccessAddresses_ByExternalAccessAddressId_ShouldSucceed()
        {
            var key = Guid.Parse("0a3f5090-b718-32b8-e044-0003ba298018");

            var result = _addressRepository.FetchAccessAndUnitAddressesByIds(new Guid[] { key }).ToList();

            // Assert
            result.Count.Should().Be(2);

            var accessAddress = result.Find(r => r.Item1 == key).Item2 as AccessAddress;
            accessAddress.RoadName.Should().StartWith("Engum");
        }

        [Fact]
        public void QueryAccessAddresses_ByInternalUnitAddressId_ShouldSucceed()
        {
            var key = Guid.Parse("d81c1428-1fe2-44bf-be71-57a5cfe8ac6c");

            var result = _addressRepository.FetchAccessAndUnitAddressesByIds(new Guid[] { key }).ToList();

            // Assert
            result.Count.Should().Be(2);

            var unitAddress = result.Find(r => r.Item1 == key).Item2 as UnitAddress;

            var accessAddress = result.Find(r => r.Item2.Id == unitAddress.AccessAddressId).Item2 as AccessAddress;

            accessAddress.RoadName.Should().StartWith("Engum");
        }

        [Fact]
        public void QueryAccessAddresses_ByExternalUnitAddressId_ShouldSucceed()
        {
            var key = Guid.Parse("0a3f50bc-aa89-32b8-e044-0003ba298018");

            var result = _addressRepository.FetchAccessAndUnitAddressesByIds(new Guid[] { key }).ToList();

            // Assert
            result.Count.Should().Be(2);

            var unitAddress = result.Find(r => r.Item1 == key).Item2 as UnitAddress;

            var accessAddress = result.Find(r => r.Item2.Id == unitAddress.AccessAddressId).Item2 as AccessAddress;

            accessAddress.RoadName.Should().StartWith("Engum");
        }

        [Fact]
        public void QuerySameAddress_ByDifferentTypesOfIds_ShouldReturnMultipleResult()
        {
            var keys = new Guid[] {
                Guid.Parse("02a0b95e-b7f1-4888-bd10-074ef49f196c"), // Engum møllevej 3 (internal access address id)
                Guid.Parse("0a3f50bc-aa89-32b8-e044-0003ba298018"), // Engum møllevej 3 (external unit address id)
            };

            var result = _addressRepository.FetchAccessAndUnitAddressesByIds(keys).ToList();

            // Assert
            result.Count.Should().Be(4); // 4 hits with access address and two unit address
            result.Should().Contain(a => a.Item1 == Guid.Parse("02a0b95e-b7f1-4888-bd10-074ef49f196c"));
            result.Should().Contain(a => a.Item1 == Guid.Parse("0a3f50bc-aa89-32b8-e044-0003ba298018"));
        }

        [Fact]
        public void QueryTwoAddresses_ShouldReturnBothAccessAndUnitAddresses()
        {
            var keys = new Guid[] {
                Guid.Parse("02a0b95e-b7f1-4888-bd10-074ef49f196c"), // Engum møllevej 3 (internal access address id)
                Guid.Parse("5d639c7c-64e7-42c7-828e-5f615a13424b"), // Vesterbrogade 7A (internal unit address id)
            };

            var result = _addressRepository.FetchAccessAndUnitAddressesByIds(keys).ToList();

            // Assert
            result.Count.Should().Be(4); // 2 access addresses + 2 belonging unit addresses
        }

        [Fact]
        public void QueryLerbjergAddresses_ShouldReturnBothAccessAndUnitAddresses()
        {
            if (!(_addressRepository is PostgresAddressRepository))
                return;

            var keys = new Guid[] {
                Guid.Parse("0a3f50c3-86a2-32b8-e044-0003ba298018"), // Lerbjerg 17 enh id
                Guid.Parse("0a3f50c3-86a4-32b8-e044-0003ba298018"), // Lerbjerg 19 enh id
                Guid.Parse("0a3f50c3-86a5-32b8-e044-0003ba298018") // Lerbjerg 21 enh id
            };

            var result = _addressRepository.FetchAccessAndUnitAddressesByIds(keys).ToList();

            // Assert
            result.Count.Should().Be(6);
        }



    }
}
