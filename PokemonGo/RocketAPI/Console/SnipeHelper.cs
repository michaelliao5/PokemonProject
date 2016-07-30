using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using POGOProtos.Enums;
using POGOProtos.Networking.Responses;
using POGOProtos.Inventory.Item;

namespace PokemonGo.RocketAPI.Console
{
    public static class SnipeHelper
    {
        public static async Task Scan(Client client, GetInventoryResponse inventory)
        {
            foreach(var location in Common.DratiniSpawn)
            {
                var res = SnipeScanForPokemon(location);
                foreach(var pokemon in res.pokemon.Where(x => x.pokemonId == (int)PokemonId.Dratini))
                {
                    System.Console.WriteLine($"Scanned Location Lat:{location.Latitude}, Lng:{location.Longitude} Pokemon:{(PokemonId)pokemon.pokemonId}");
                    await Sniping(client, location, inventory);
                }
            }
        }
        private static async Task Sniping(Client client, Location newLoca, GetInventoryResponse _inventory)
        {
            var curLocation = new Location(client.CurrentLatitude, client.CurrentLongitude);

            await client.Player.UpdatePlayerLocation(newLoca.Latitude, newLoca.Longitude, 100);

            var mapObjects = await client.Map.GetMapObjects();

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons).Where(x => x.PokemonId == PokemonId.Dratini);
            
            foreach (var pokemon in pokemons)
            {
                System.Console.WriteLine($"Started Sniping {pokemon.PokemonId}");
                var encounterPokemonRespone = await client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);

                await client.Player.UpdatePlayerLocation(curLocation.Latitude, curLocation.Longitude, 100);

                CatchPokemonResponse caughtPokemonResponse;

                var berryUsed = false;
                var inventoryBalls = _inventory;

                var items = inventoryBalls.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData.Item).Where(p => p != null);
                if (encounterPokemonRespone.WildPokemon != null)
                {
                    var pokeBall = await Helper.GetBestBall(encounterPokemonRespone.WildPokemon.PokemonData.Cp, inventoryBalls);
                    do
                    {
                        if (!berryUsed && encounterPokemonRespone.CaptureProbability.CaptureProbability_.First() < 0.4 && Common.BerryPokemons.Contains(pokemon.PokemonId) && items.Where(p => p.ItemId == ItemId.ItemRazzBerry).Count() > 0 && items.Where(p => p.ItemId == ItemId.ItemRazzBerry).First().Count > 0)
                        {
                            berryUsed = true;
                            System.Console.Write($"Use Rasperry (" + items.Where(p => p.ItemId == ItemId.ItemRazzBerry).First().Count + ")!");
                            await client.Encounter.UseCaptureItem(pokemon.EncounterId, ItemId.ItemRazzBerry, pokemon.SpawnPointId);
                            //await Task.Delay(3000);
                        }
                        caughtPokemonResponse = await client.Encounter.CatchPokemon(pokemon.EncounterId, pokemon.SpawnPointId, pokeBall);

                    } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed || caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
                    
                    System.Console.WriteLine(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess
                        ? $"[{DateTime.Now.ToString("HH:mm:ss")}] We sniped a {pokemon.PokemonId} with CP {encounterPokemonRespone?.WildPokemon?.PokemonData?.Cp} "
                        : $"[{DateTime.Now.ToString("HH:mm:ss")}] {pokemon.PokemonId} got away..");
                    System.Console.WriteLine($"Finished Sniping {pokemon.PokemonId}");
                }
            }
        }
        private class PokemonLocation
        {
            public long id { get; set; }
            public double expiration_time { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public int pokemonId { get; set; }

            public PokemonLocation(double _latitude, double _longitude)
            {
                latitude = _latitude;
                longitude = _longitude;
            }

            public override int GetHashCode()
            {
                return this.ToString().GetHashCode();
            }

            public override string ToString()
            {
                return latitude.ToString("0.0000") + ", " + longitude.ToString("0.0000");
            }

            public bool Equals(PokemonLocation obj)
            {
                return Math.Abs(latitude - obj.latitude) < 0.0001 && Math.Abs(longitude - obj.longitude) < 0.0001;
            }

            public override bool Equals(Object obj) // contains calls this here
            {
                if (obj == null)
                    return false;

                PokemonLocation p = obj as PokemonLocation;
                if ((System.Object)p == null) // no cast available
                {
                    return false;
                }

                return Math.Abs(latitude - p.latitude) < 0.0001 && Math.Abs(longitude - p.longitude) < 0.0001;
            }
        }
        private class ScanResult
        {
            public string status { get; set; }
            public List<PokemonLocation> pokemon { get; set; }
        }
        private static ScanResult SnipeScanForPokemon(Location location)
        {
            var formatter = new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." };
            var uri = $"https://pokevision.com/map/data/{location.Latitude.ToString(formatter)}/{location.Longitude.ToString(formatter)}";

            ScanResult scanResult;
            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.Accept = "application/json";
                request.Method = "GET";
                request.Timeout = 1000;

                var resp = request.GetResponse();
                var reader = new StreamReader(resp.GetResponseStream());

                scanResult = JsonConvert.DeserializeObject<ScanResult>(reader.ReadToEnd());
            }
            catch (Exception)
            {
                scanResult = new ScanResult()
                {
                    status = "fail",
                    pokemon = new List<PokemonLocation>()
                };
            }
            return scanResult;
        }
    }
}
