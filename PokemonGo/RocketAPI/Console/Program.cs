using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AllEnum;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;

namespace PokemonGo.RocketAPI.Console
{
    class Program
    {
        static int pokeballType = 0;
        static int myMaxPokemon = 400;        

        static void Main(string[] args)
        {
            Task.Run(() => Execute());
             System.Console.ReadLine();
        }
        
        static async void Execute()
        {
            var client = new Client(Settings.DefaultLatitude, Settings.DefaultLongitude);

            if (Settings.AuthType == AuthType.Ptc)
                await client.DoPtcLogin(Settings.PtcUsername, Settings.PtcPassword);
            else if (Settings.AuthType == AuthType.Google)
                Settings.GoogleRefreshToken = await client.DoGoogleLogin(Settings.GoogleRefreshToken);
            
            await client.SetServer();
            var profile = await client.GetProfile();
            var settings = await client.GetSettings();
            var mapObjects = await client.GetMapObjects();
            var inventory = await client.GetInventory();
            var pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon).Where(p => p != null && p?.PokemonId > 0);

            await Client.TransferAllButStrongestUnwantedPokemon(client);

            while (true)
            {
                await ExecuteFarmingPokestopsAndPokemons(client);
                System.Console.WriteLine("Resetting Player Position");
                var update = await client.UpdatePlayerLocation(Settings.DefaultLatitude, Settings.DefaultLongitude);
                await Task.Delay(15000);
            }            
        }

        private static async Task ExecuteFarmingPokestopsAndPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts).Where(i => i.Type == FortType.Checkpoint && i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime());
            pokeStops = SortRoute(pokeStops.ToList());

            int waitCount = 0;

            foreach (var pokeStop in pokeStops)
            {
                var update = await client.UpdatePlayerLocation(pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await client.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                var fortSearch = await client.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                if (fortSearch != null)
                {
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Farmed XP: {fortSearch.ExperienceAwarded}, Gems: { fortSearch.GemsAwarded}, Eggs: {fortSearch.PokemonDataEgg} Items: {GetFriendlyItemsString(fortSearch.ItemsAwarded)}");
                    var inventory = await client.GetInventory();
                    if (inventory != null && inventory.InventoryDelta != null)
                    {
                        var pokeBalls = inventory.InventoryDelta.InventoryItems.Select(x => x.InventoryItemData?.Item).Where(x => x != null && x.Item_ == ItemType.Pokeball);
                        pokeballType = pokeBalls.Count() == 0 || pokeBalls.First().Count == 0 ? (int)MiscEnums.Item.ITEM_GREAT_BALL : (int)MiscEnums.Item.ITEM_POKE_BALL;

                        var pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Pokemon).Where(p => p != null && p?.PokemonId > 0);

                        System.Console.WriteLine("PokemonCount:" + pokemons.Count());
                        if (pokemons.Count() >= myMaxPokemon)
                        {
                            Environment.Exit(0);
                        }

                        await ExecuteCatchAllNearbyPokemons(client);

                        if (fortSearch.ExperienceAwarded == 0)
                        {
                            waitCount++;
                        }
                        if (waitCount > 3)
                        {
                            System.Console.WriteLine("Waiting for 30 secs for soft ban");
                            await Task.Delay(30000);
                            waitCount = 0;
                        }
                    }
                }

                await Task.Delay(8000);
            }
        }

        private static async Task ExecuteCatchAllNearbyPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);

            foreach (var pokemon in pokemons)
            {
                var update = await client.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude);
                var encounterPokemonRespone = await client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);

                CatchPokemonResponse caughtPokemonResponse;
                do
                {
                    caughtPokemonResponse = await client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude, MiscEnums.Item.ITEM_POKE_BALL); //note: reverted from settings because this should not be part of settings but part of logic
                } 
                while(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed);

                System.Console.WriteLine(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess ? $"[{DateTime.Now.ToString("HH:mm:ss")}] We caught a {pokemon.PokemonId}" : $"[{DateTime.Now.ToString("HH:mm:ss")}] {pokemon.PokemonId} got away..");
                await Task.Delay(5000);
            }
        }

        private static string GetFriendlyItemsString(IEnumerable<FortSearchResponse.Types.ItemAward> items)
        {
            var enumerable = items as IList<FortSearchResponse.Types.ItemAward> ?? items.ToList();

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => i.ItemId)
                          .Select(kvp => new {ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount)})
                          .Select(y => $"{y.Amount} x {y.ItemName}")
                          .Aggregate((a, b) => $"{a}, {b}");
        }

        static List<FortData> SortRoute(List<FortData> route)
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
