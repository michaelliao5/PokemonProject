using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Console
{
    public static class Helper
    {
        public static async Task<List<PokemonFamily>> GetPokemonFamilies(GetInventoryResponse inventory)
        {
            var families = from item in inventory.InventoryDelta.InventoryItems
                           where item.InventoryItemData?.PokemonFamily != null
                           where item.InventoryItemData?.PokemonFamily.FamilyId != PokemonFamilyId.FamilyUnset
                           group item by item.InventoryItemData?.PokemonFamily.FamilyId into family
                           select new PokemonFamily
                           {
                               FamilyId = family.First().InventoryItemData.PokemonFamily.FamilyId,
                               Candy = family.First().InventoryItemData.PokemonFamily.Candy
                           };


            return families.ToList();
        }

        public static async Task<ItemId> GetBestBall(int pokemonCP, GetInventoryResponse inventory)
        {
            var pokeBallsCount = inventory.InventoryDelta.InventoryItems.Select(x => x.InventoryItemData?.Item).Where(x => x != null && x.ItemId == ItemId.ItemPokeBall).FirstOrDefault()?.Count ?? 0;
            var greatBallsCount = inventory.InventoryDelta.InventoryItems.Select(x => x.InventoryItemData?.Item).Where(x => x != null && x.ItemId == ItemId.ItemGreatBall).FirstOrDefault()?.Count ?? 0;
            var ultraBallsCount = inventory.InventoryDelta.InventoryItems.Select(x => x.InventoryItemData?.Item).Where(x => x != null && x.ItemId == ItemId.ItemUltraBall).FirstOrDefault()?.Count ?? 0;
            var masterBallsCount = inventory.InventoryDelta.InventoryItems.Select(x => x.InventoryItemData?.Item).Where(x => x != null && x.ItemId == ItemId.ItemMasterBall).FirstOrDefault()?.Count ?? 0;

            System.Console.WriteLine($"Pokeballs Left: {pokeBallsCount + greatBallsCount + ultraBallsCount}");

            if (masterBallsCount > 0 && pokemonCP >= 2000)
                return ItemId.ItemMasterBall;
            else if (ultraBallsCount > 0 && pokemonCP >= 1500)
                return ItemId.ItemUltraBall;
            else if (greatBallsCount > 0 && pokemonCP >= 1000)
                return ItemId.ItemGreatBall;

            return ItemId.ItemPokeBall;
        }

        public static string GetFriendlyItemsString(IEnumerable<ItemAward> items)
        {
            var enumerable = items as IList<ItemAward> ?? items.ToList();

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => i.ItemId)
                          .Select(kvp => new { ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount) })
                          .Select(y => $"{y.Amount} x {y.ItemName}")
                          .Aggregate((a, b) => $"{a}, {b}");
        }

        public static List<FortData> SortRoute(List<FortData> route)
        {
            if (route.Count < 3)
            {
                return route;
            }
            route = route.OrderBy(x => Distance(x.Latitude, x.Longitude, Settings.DefaultLatitude, Settings.DefaultLongitude)).ToList();
            foreach (var stop in route)
            {
                System.Console.WriteLine("PokeStop Distance: " + Distance(stop.Latitude, stop.Longitude, Settings.DefaultLatitude, Settings.DefaultLongitude));
            }
            var newRoute = new List<FortData> { route.First() };
            route.RemoveAt(0);
            int i = 0;
            while (route.Any())
            {
                var next = route.OrderBy(x => Distance(x.Latitude, x.Longitude, newRoute.Last().Latitude, newRoute.Last().Longitude)).First();
                newRoute.Add(next);
                route.Remove(next);
                i++;
                if (i > 50)
                {
                    break;
                }
            }
            return newRoute;
        }

        private static double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad(lat2 - lat1);  // deg2rad below
            var dLon = deg2rad(lon2 - lon1);
            var a =
              Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
              Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
              ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        private static double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
