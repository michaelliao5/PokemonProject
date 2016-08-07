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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace PokemonGo.RocketAPI.Console
{
    public static class SnipeHelper
    {
        static List<PokemonLocation> SnipedIds = new List<PokemonLocation>();
        static int tabuLength = 0;
        
        public static async Task Scan(Client client, GetInventoryResponse inventory)
        {
            var res = await SnipeScanForPokemonUsingDiscord();
            foreach (var pokemon in res.pokemon.Where(x => Common.SnipePokemons.Select(y => (int)y).Contains(x.pokemonId) && !SnipedIds.Any(p => p.Equals(x))))
            {
                System.Console.WriteLine($"OP SNIPER Scanned Location Lat:{pokemon.latitude}, Lng:{pokemon.longitude} Pokemon:{(PokemonId)pokemon.pokemonId}");
                if (await Sniping(client, new Location(pokemon.latitude, pokemon.longitude), inventory))
                {
                    SnipedIds.Add(new PokemonLocation(pokemon.latitude, pokemon.longitude));
                }
            }
            res = await SnipeScanForPokemon();
            foreach (var pokemon in res.pokemon.Where(x => Common.SnipePokemons.Select(y => (int)y).Contains(x.pokemonId) && !SnipedIds.Any(p => p.Equals(x))))
            {
                System.Console.WriteLine($"Scanned Location Lat:{pokemon.latitude}, Lng:{pokemon.longitude} Pokemon:{(PokemonId)pokemon.pokemonId}");
                if (await Sniping(client, new Location(pokemon.latitude, pokemon.longitude), inventory))
                {
                    SnipedIds.Add(new PokemonLocation(pokemon.latitude, pokemon.longitude));
                }
            }
            tabuLength++;
            if(tabuLength > 50)
            {
                tabuLength = 0;
                SnipedIds.Clear();
            }
        }
        private static async Task<bool> Sniping(Client client, Location newLoca, GetInventoryResponse _inventory)
        {
            var curLocation = new Location(client.CurrentLatitude, client.CurrentLongitude);

            await client.Player.UpdatePlayerLocation(newLoca.Latitude, newLoca.Longitude, 100);

            await Task.Delay(5000);

            var mapObjects = await client.Map.GetMapObjects();

            var pokemons = mapObjects.Item1.MapCells.SelectMany(i => i.CatchablePokemons).Where(x => Common.SnipePokemons.Contains(x.PokemonId));
            
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
                    return true;
                }
            }
            await client.Player.UpdatePlayerLocation(curLocation.Latitude, curLocation.Longitude, 100);
            return false;
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
        public class SniperInfo
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Iv { get; set; }
            public DateTime TimeStamp { get; set; }
            public PokemonId Id { get; set; }

            [JsonIgnore]
            public DateTime TimeStampAdded { get; set; } = DateTime.Now;
        }
        private class ScanResult
        {
            public string status { get; set; }
            public List<PokemonLocation> pokemon { get; set; }
        }
        private static async Task<ScanResult> SnipeScanForPokemonUsingDiscord()
        {
            var scanResult = new ScanResult();
            scanResult.pokemon = new List<PokemonLocation>();
            try
            {
                var lines = File.ReadAllLines("c:\\PokemonOutput\\out.txt");
                foreach(var line in lines)
                {
                    var splitted = line.Split('|');
                    var id = int.Parse(splitted[0]);
                    var pokemonId = int.Parse(splitted[1]);
                    var lat = double.Parse(splitted[2]);
                    var lng = double.Parse(splitted[3]);
                    var loca = new PokemonLocation(lat, lng)
                    {
                        id = id,
                        latitude = lat,
                        longitude = lng,
                        pokemonId = pokemonId,
                    };
                    if (!scanResult.pokemon.Any(x => x.Equals(loca)))
                        scanResult.pokemon.Add(loca);
                }
            }
            catch (Exception ex)
            {
                // most likely System.IO.IOException
            }
            return scanResult;
        }
        private static async Task<ScanResult> SnipeScanForPokemon()
        {
            var formatter = new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." };
            ScanResult scanResult = new ScanResult();
            scanResult.pokemon = new List<PokemonLocation>();
            try
            {
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(3);
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await httpClient.GetAsync("http://pokesnipers.com/api/v1/pokemon.json");
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic pokesniper = JsonConvert.DeserializeObject(json);
                    JArray results = pokesniper.results;
                    foreach (var result in results)
                    {
                        PokemonId pokeId;
                        long id;
                        Enum.TryParse(result.Value<string>("name"), out pokeId);
                        long.TryParse(CleanNonDigits(result.Value<string>("id")), out id);
                        var a = new PokemonLocation(Convert.ToDouble(result.Value<string>("coords").Split(',')[0]), Convert.ToDouble(result.Value<string>("coords").Split(',')[1]))
                        {
                            pokemonId = (int)pokeId,
                            id = id,
                        };
                        scanResult.pokemon.Add(a);
                    }
                }
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

        static string CleanNonDigits(string num)
        {
            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(num, "");
        }
    }
}
