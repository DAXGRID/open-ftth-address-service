using NetTopologySuite.Geometries;
using Npgsql;
using NpgsqlTypes;
using OpenFTTH.Address.API.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.Address.Business.Repository
{
    public class PostgresAddressRepository : IAddressRepository
    {
        private readonly string _connectionString;

        public PostgresAddressRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<(Guid, IAddress)> FetchAccessAndUnitAddressesByIds(Guid[] accessOrUnitAddressIds)
        {
            var accessAddressesWithUnitAddresses = FindAccessAddressesAndRelatedUnitAddresses(accessOrUnitAddressIds);

            foreach (var address in accessAddressesWithUnitAddresses)
            {
                yield return (address.Item1, address.Item2);
            }

            var unitAddressesWithAccessAddress = FindUnitAddressesAndRelatedAccessAddress(accessOrUnitAddressIds);

            foreach (var address in unitAddressesWithAccessAddress)
            {
                yield return (address.Item1, address.Item2);
            }
        }

        public IEnumerable<(double, IAddress)> FetchNearestAccessAndUnitAddresses(double x, double y, int srid, int maxHits)
        {
            return FindNearestAddresses(x, y, srid, maxHits);
        }

        private IEnumerable<(Guid, IAddress)> FindAccessAddressesAndRelatedUnitAddresses(Guid[] accessOrUnitAddressIds)
        {
            List<(Guid, AccessAddress)> result = new();

            var accessOrUnitAddressIdsLookup = accessOrUnitAddressIds.ToHashSet<Guid>();

            Dictionary<Guid, (Guid, IAddress)> addressHitById = new();

            Stopwatch sw = new();
            sw.Start();
            using var conn = GetConnection();

            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT
                    aa.id, 
                    ST_X(aa.coord), 
                    ST_Y(aa.coord), 
                    aa.house_number, 
                    aa.post_district_code, 
                    aa.post_district_name,                    
                    aa.access_address_external_id,
                    aa.road_code, 
                    aa.road_name, 
                    aa.town_name, 
                    aa.municipal_code,
                    ua.id,
                    ua.unit_address_external_id,
                    ua.floor_name,
                    ua.suit_name
                FROM 
                    location.official_access_address aa
                LEFT OUTER JOIN
                    location.official_unit_address ua on ua.access_address_id = aa.id
                WHERE 
                    (aa.id = ANY(:guid_ids) or aa.access_address_external_id = ANY(:str_ids)) and aa.coord is not null";

            cmd.Parameters.Add("guid_ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid).Value = accessOrUnitAddressIds;
            cmd.Parameters.Add("str_ids", NpgsqlDbType.Array | NpgsqlDbType.Varchar).Value = accessOrUnitAddressIds.Select(u => u.ToString()).ToArray();

            using var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                var accessAddressId = rdr.GetGuid(0);

                Guid? accessAddressExternalId = rdr.IsDBNull(6) ? null : Guid.Parse(rdr.GetString(6));

                Guid? unitAddressId = rdr.IsDBNull(11) ? null : rdr.GetGuid(11);

                if (!addressHitById.ContainsKey(accessAddressId))
                {
                    var aa = new AccessAddress(
                        id: rdr.GetGuid(0),
                        addressPoint: new Point(rdr.GetDouble(1), rdr.GetDouble(2)),
                        unitAddressIds: rdr.IsDBNull(11) ? Array.Empty<Guid>() : new Guid[] { rdr.GetGuid(11) }
                    )
                    {
                        HouseNumber = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                        PostDistrictCode = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                        PostDistrict = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                        ExternalId = rdr.IsDBNull(6) ? null : Guid.Parse(rdr.GetString(6)),
                        RoadCode = rdr.IsDBNull(7) ? null : rdr.GetString(7),
                        RoadName = rdr.IsDBNull(8) ? null : rdr.GetString(8),
                        TownName = rdr.IsDBNull(9) ? null : rdr.GetString(9),
                        MunicipalCode = rdr.IsDBNull(10) ? null : rdr.GetString(10),
                    };

                    if (accessOrUnitAddressIdsLookup.Contains(aa.Id))
                        addressHitById.Add(accessAddressId, (aa.Id, aa));
                    else if (aa.ExternalId != null && accessOrUnitAddressIdsLookup.Contains(aa.ExternalId.Value))
                        addressHitById.Add(accessAddressId, (aa.ExternalId.Value, aa));
                }

                if (unitAddressId != null && !addressHitById.ContainsKey(unitAddressId.Value))
                {
                    var ua = new UnitAddress(unitAddressId.Value, accessAddressId)
                    {
                        ExternalId = rdr.IsDBNull(12) ? null : Guid.Parse(rdr.GetString(12)),
                        FloorName = rdr.IsDBNull(13) ? null : rdr.GetString(13),
                        SuitName = rdr.IsDBNull(14) ? null : rdr.GetString(14)
                    };

                    if (accessOrUnitAddressIdsLookup.Contains(accessAddressId))
                        addressHitById.Add(ua.Id, (Guid.Empty, ua));
                    else if (accessAddressExternalId != null && accessOrUnitAddressIdsLookup.Contains(accessAddressExternalId.Value))
                        addressHitById.Add(ua.Id, (Guid.Empty, ua));
                }
            }

            sw.Stop();

            return addressHitById.Values;
        }

        private IEnumerable<(double, IAddress)> FindNearestAddresses(double x, double y, int srid, int maxHits)
        {
            var xStr = x.ToString(CultureInfo.InvariantCulture);
            var yStr = y.ToString(CultureInfo.InvariantCulture);

            using var conn = GetConnection();

            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT 
                  aa.id,
                  ST_Distance(aa.coord, ST_Transform('SRID=" + srid + ";POINT(" + xStr + " " + yStr + @")'::geometry, 25832)) AS dist
                FROM
                  location.official_access_address aa
                WHERE
                   aa.coord is not null
                ORDER BY
                  aa.coord <->ST_Transform('SRID=" + srid + ";POINT(" + xStr + " " + yStr + @")'::geometry, 25832)
                LIMIT " + maxHits;

            using var rdr = cmd.ExecuteReader();

            Dictionary<Guid, double> accessAddressDistanceById = new();

            while (rdr.Read())
            {
                var accessAddressId = rdr.GetGuid(0);
                var distance = rdr.GetDouble(1);

                accessAddressDistanceById.Add(accessAddressId, distance);
            }

            if (accessAddressDistanceById.Count == 0)
                return Array.Empty<(double, IAddress)>();

            var addresses = FindAccessAddressesAndRelatedUnitAddresses(accessAddressDistanceById.Keys.ToArray());

            Dictionary<Guid, (double, IAddress)> result = new();

            foreach (var addressHit in addresses)
            {
                if (!result.ContainsKey(addressHit.Item2.Id))
                {
                    if (addressHit.Item2 is AccessAddress)
                        result.Add(addressHit.Item2.Id, (accessAddressDistanceById[addressHit.Item2.Id], addressHit.Item2));
                    else
                        result.Add(addressHit.Item2.Id, (-1, addressHit.Item2));
                }
            }

            return result.Values;
        }

        private IEnumerable<(Guid, IAddress)> FindUnitAddressesAndRelatedAccessAddress(Guid[] accessOrUnitAddressIds)
        {
            List<(Guid, AccessAddress)> result = new();

            var accessOrUnitAddressIdsLookup = accessOrUnitAddressIds.ToHashSet<Guid>();

            Dictionary<Guid, (Guid, IAddress)> addressHitById = new();

            Stopwatch sw = new();
            sw.Start();
            using var conn = GetConnection();

            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT
                    aa.id, 
                    ST_X(aa.coord), 
                    ST_Y(aa.coord), 
                    aa.house_number, 
                    aa.post_district_code, 
                    aa.post_district_name,                    
                    aa.access_address_external_id,
                    aa.road_code, 
                    aa.road_name, 
                    aa.town_name, 
                    aa.municipal_code,
                    ua.id,
                    ua.unit_address_external_id,
                    ua.floor_name,
                    ua.suit_name
                FROM 
                    location.official_unit_address ua
                LEFT OUTER JOIN
                    location.official_access_address aa on aa.id = ua.access_address_id
                WHERE 
                    aa.id is not null and aa.coord is not null and (ua.id = ANY(:guid_ids) or ua.unit_address_external_id = ANY(:str_ids))";

            cmd.Parameters.Add("guid_ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid).Value = accessOrUnitAddressIds;
            cmd.Parameters.Add("str_ids", NpgsqlDbType.Array | NpgsqlDbType.Varchar).Value = accessOrUnitAddressIds.Select(u => u.ToString()).ToArray();

            using var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Guid accessAddressId = rdr.GetGuid(0);
                Guid unitAddressId = rdr.GetGuid(11);
                Guid? unitAddressExternalId = rdr.IsDBNull(12) ? null : Guid.Parse(rdr.GetString(12));

                if (!addressHitById.ContainsKey(accessAddressId))
                {
                    var aa = new AccessAddress(
                        id: rdr.GetGuid(0),
                        addressPoint: new Point(rdr.GetDouble(1), rdr.GetDouble(2)),
                        unitAddressIds: rdr.IsDBNull(11) ? Array.Empty<Guid>() : new Guid[] { rdr.GetGuid(11) }
                    )
                    {
                        HouseNumber = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                        PostDistrictCode = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                        PostDistrict = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                        ExternalId = rdr.IsDBNull(6) ? null : Guid.Parse(rdr.GetString(6)),
                        RoadCode = rdr.IsDBNull(7) ? null : rdr.GetString(7),
                        RoadName = rdr.IsDBNull(8) ? null : rdr.GetString(8),
                        TownName = rdr.IsDBNull(9) ? null : rdr.GetString(9),
                        MunicipalCode = rdr.IsDBNull(10) ? null : rdr.GetString(10),
                    };

                    if (accessOrUnitAddressIdsLookup.Contains(unitAddressId))
                        addressHitById.Add(aa.Id, (Guid.Empty, aa));
                    else if (unitAddressExternalId != null && accessOrUnitAddressIdsLookup.Contains(unitAddressExternalId.Value))
                        addressHitById.Add(aa.Id, (Guid.Empty, aa));
                }

                if (!addressHitById.ContainsKey(unitAddressId))
                {
                    var ua = new UnitAddress(unitAddressId, accessAddressId)
                    {
                        ExternalId = rdr.IsDBNull(12) ? null : Guid.Parse(rdr.GetString(12)),
                        FloorName = rdr.IsDBNull(13) ? null : rdr.GetString(13),
                        SuitName = rdr.IsDBNull(14) ? null : rdr.GetString(14)
                    };

                    if (accessOrUnitAddressIdsLookup.Contains(ua.Id))
                        addressHitById.Add(unitAddressId, (ua.Id, ua));
                    else if (unitAddressExternalId != null && accessOrUnitAddressIdsLookup.Contains(unitAddressExternalId.Value))
                        addressHitById.Add(unitAddressId, (unitAddressExternalId.Value, ua));
                }
            }

            sw.Stop();

            return addressHitById.Values;
        }

        private NpgsqlConnection GetConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
