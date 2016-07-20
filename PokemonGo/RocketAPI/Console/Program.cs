using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;
using static PokemonGo.RocketAPI.GeneratedCode.MapObjectsResponse.Types.Payload.Types;

namespace PokemonGo.RocketAPI.Console
{
    class Program
    {
        static int pokeballType = 0;
        static void Main(string[] args)
        {
            Task.Run(() => Execute());
            System.Console.ReadLine();
        }

        static async void Execute()
        {
            var client = new Client(Settings.DefaultLatitude, Settings.DefaultLongitude);

            if (Settings.UsePTC)
            {
                await client.LoginPtc(Settings.PtcUsername, Settings.PtcPassword);
            }
            else
            {
                await client.LoginGoogle();
            }
            var serverResponse = await client.GetServer();
            var profile = await client.GetProfile();
            var settings = await client.GetSettings();
            var mapObjects = await client.GetMapObjects();
            var inventory = await client.GetInventory();
            var pokemons = inventory.Payload[0].Bag.Items.Select(i => i.Item?.Pokemon).Where(p => p != null && p?.PokemonId != InventoryResponse.Types.PokemonProto.Types.PokemonIds.PokemonUnset);

            while (true)
            {
                await ExecuteFarmingPokestopsAndPokemons(client);
                System.Console.WriteLine("Resetting Player Position");
                var update = await client.UpdatePlayerLocation(Settings.DefaultLatitude, Settings.DefaultLongitude);
                await Task.Delay(15000);
            }

            //await ExecuteCatchAllNearbyPokemons(client);


        }

        private static async Task ExecuteFarmingPokestopsAndPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokeStops = mapObjects.Payload.SelectMany(y => y.Profile.SelectMany(i => i.Fort).Where(i => i.FortType == (int)MiscEnums.FortType.CHECKPOINT && i.CooldownCompleteMs < DateTime.UtcNow.ToUnixTime()));
            pokeStops = SortRoute(pokeStops.ToList());

            int waitCount = 0;

            foreach (var pokeStop in pokeStops)
            {
                var update = await client.UpdatePlayerLocation(pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await client.GetFort(pokeStop.FortId, pokeStop.Latitude, pokeStop.Longitude);
                var fortSearch = await client.SearchFort(pokeStop.FortId, pokeStop.Latitude, pokeStop.Longitude);
                var bag = fortSearch.Payload[0];

                System.Console.WriteLine($"Farmed XP: {bag.XpAwarded}, Gems: { bag.GemsAwarded}, Eggs: {bag.EggPokemon} Items: {GetFriendlyItemsString(bag.Items)}");

                var inventory = await client.GetInventory();
                var pokeBalls = inventory.Payload[0].Bag.Items.Select(x => x.Item?.Item).Where(x => x != null && x.Item == (int)MiscEnums.Item.ITEM_POKE_BALL);
                pokeballType = pokeBalls.Count() == 0 || pokeBalls.First().Count == 0  ? (int)MiscEnums.Item.ITEM_GREAT_BALL : (int)MiscEnums.Item.ITEM_POKE_BALL;

                await ExecuteCatchAllNearbyPokemons(client);

                if (bag.XpAwarded == 0)
                {
                    waitCount++;
                }
                if (waitCount > 3)
                {
                    System.Console.WriteLine("Waiting for 30 secs for soft ban");
                    await Task.Delay(30000);
                    waitCount = 0;
                }

                await Task.Delay(8000);
            }
        }

        private static async Task ExecuteCatchAllNearbyPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            await Task.Delay(2000);

            var pokemons = mapObjects.Payload.SelectMany(x => x.Profile.SelectMany(i => i.MapPokemon));
            int j = 0;
            foreach (var pokemon in pokemons)
            {
                j++;
                System.Console.WriteLine($"Encountering a {GetFriendlyPokemonName(pokemon.PokedexTypeId)}");
                var update = await client.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude);
                var encounterPokemonRespone = await client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);

                CatchPokemonResponse caughtPokemonResponse;
                do
                {
                    caughtPokemonResponse = await client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude, pokeballType);
                }
                while (caughtPokemonResponse.Payload[0].Status == 2);

                System.Console.WriteLine(caughtPokemonResponse.Payload[0].Status == 1 ? $"We caught a {GetFriendlyPokemonName(pokemon.PokedexTypeId)}" : $"{GetFriendlyPokemonName(pokemon.PokedexTypeId)} got away..");

                await Task.Delay(5000);
            }
        }

        private static string GetFriendlyPokemonName(MapObjectsResponse.Types.Payload.Types.PokemonIds id)
        {
            var name = Enum.GetName(typeof(InventoryResponse.Types.PokemonProto.Types.PokemonIds), id);
            return name?.Substring(name.IndexOf("Pokemon") + 7);
        }

        private static string GetFriendlyItemsString(IEnumerable<FortSearchResponse.Types.Item> items)
        {
            var enumerable = items as IList<FortSearchResponse.Types.Item> ?? items.ToList();
            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => (MiscEnums.Item)i.Item_)
                .Select(kvp => new { ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount) })
                .Select(y => $"{y.Amount} x {y.ItemName}")
                .Aggregate((a, b) => $"{a}, {b}");
        }

        static List<PokemonFortProto> SortRoute(List<PokemonFortProto> route)
        {
            if (route.Count < 3)
            {
                return route;
            }
            route = route.OrderBy(x => Distance(x.Latitude, x.Longitude, Settings.DefaultLatitude, Settings.DefaultLongitude)).ToList();
            foreach (var stop in route)
            {
                System.Console.WriteLine("Distance: " + Distance(stop.Latitude, stop.Longitude, Settings.DefaultLatitude, Settings.DefaultLongitude));
            }
            var newRoute = new List<PokemonFortProto> { route.First() };
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

        //private static double Distance(double lat1, double long1, double lat2, double long2)
        //{
        //    return Math.Sqrt(Math.Pow(lat1 - lat2, 2) + Math.Pow(long1 - lat2, 2));
        //}

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
