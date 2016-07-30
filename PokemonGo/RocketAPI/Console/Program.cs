using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using System.Timers;
using POGOProtos.Inventory;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using POGOProtos.Map.Pokemon;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Data;
using System.Diagnostics;

namespace PokemonGo.RocketAPI.Console
{
    class Program
    {
        static int myMaxPokemon = 250;
        static int basePokemonCount = 250;        
        static string password = "Poke1234";
        static bool publishEnabled = false;
        static GetInventoryResponse _inventory;
        static Stopwatch _timer = new Stopwatch();

        static void Main(string[] args)
        {
            if (args != null && args.Count() > 0)
            {
                try
                {
                    Settings.AuthType = AuthType.Ptc;
                    Settings.PtcUsername = args[0];
                    Settings.PtcPassword = "Poke1234";
                    if (args.Count() > 1)
                    {
                        if (args[0].Trim().ToLowerInvariant() == "google")
                        {
                            if (args[1].Trim().ToLowerInvariant() == "dratini")
                            {
                                Settings.AuthType = AuthType.Google;
                                Settings.PtcUsername = args[2];
                            }
                            else
                            {
                                Settings.AuthType = AuthType.Google;
                                Settings.PtcUsername = args[1];
                            }
                        }
                        else if (args[0].Trim().ToLowerInvariant() == "dratini")
                        {
                            Settings.PtcUsername = args[1];
                            Settings.DratiniMode = true;
                        }
                        else
                        {
                            Settings.PtcPassword = args[1];
                        }
                        if(args.Count() == 4)
                        {
                            Settings.DefaultLongitude = long.Parse(args[3]);
                            Settings.DefaultLatitude = long.Parse(args[2]);
                            System.Console.WriteLine($"Location Set to Lat: {Settings.DefaultLatitude} Long: {Settings.DefaultLongitude}");
                        }
                        else
                        {
                            System.Console.WriteLine($"Location Set to New York");
                        }                        
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine($"Error in Command Line input");
                    UserInput();
                }
            }
            else
            {
                UserInput();
            }
            Task.Run(() => Execute());
            System.Console.ReadLine();
        }

        static void UserInput()
        {
            System.Console.WriteLine($"Location Set to New York Central Park by Default");
            System.Console.WriteLine($"Input Longitude, press enter if going with New York, Enter 1 to Santa Monica, Enter 2 to San Fran, Enter 3 to Sydney:");
            var longi = System.Console.ReadLine();
            if (!string.IsNullOrEmpty(longi))
            {
                if(longi.Trim() == "1")
                {
                    Settings.DefaultLatitude = Settings.SantaMonicaLatitude;
                    Settings.DefaultLongitude = Settings.SantaMonicaLongitude;
                }
                else if(longi.Trim() == "3")
                {
                    Settings.DefaultLatitude = Settings.SidneyLatitude;
                    Settings.DefaultLongitude = Settings.SidneyLongitude;
                }
                else if (longi.Trim() == "2")
                {
                    Settings.DefaultLatitude = Settings.SanFranLatitude;
                    Settings.DefaultLongitude = Settings.SanFranLongitude;
                }
                else
                {
                    System.Console.WriteLine($"Input Latitude:");
                    Settings.DefaultLatitude = Double.Parse(System.Console.ReadLine());
                    Settings.DefaultLongitude = Double.Parse(longi);
                }                
            }

            System.Console.WriteLine($"Using PTC? (y/n)?");
            var ptc = System.Console.ReadLine();

            if (ptc == "y" || Settings.AuthType == AuthType.Ptc)
            {
                Settings.AuthType = AuthType.Ptc;
                System.Console.WriteLine($"PTC Username:");
                Settings.PtcUsername = System.Console.ReadLine();
                System.Console.WriteLine($"PTC Password:");
                Settings.PtcPassword = System.Console.ReadLine();
            }
            else
            {
                Settings.AuthType = AuthType.Google;
                System.Console.WriteLine($"google Username:");
                Settings.GoogleUsername = System.Console.ReadLine();
                System.Console.WriteLine($"google Password:");
                Settings.GooglePassword = System.Console.ReadLine();
            }
        }

        static async void Execute()
        {
            if (Settings.DratiniMode)
            {
                Settings.DefaultLatitude = Settings.DratiniLatitude;
                Settings.DefaultLongitude = Settings.DratiniLongitude;
                System.Console.WriteLine("Starting Dratini Farm");
            }
            if(Settings.UsingIV)
            {
                System.Console.WriteLine("Using IV Mode");
            }
            var client = new Client(Settings.DefaultLatitude, Settings.DefaultLongitude, Settings.PtcUsername, Settings.PtcPassword, Settings.GoogleUsername, Settings.GooglePassword, Settings.GoogleRefreshToken, Settings.AuthType);
        ReExecute:
            try
            {
                _timer.Start();
                if (Settings.AuthType == AuthType.Ptc)
                {
                    await client.Login.DoPtcLogin(Settings.PtcUsername, Settings.PtcPassword);
                }
                else if (Settings.AuthType == AuthType.Google)
                    await client.Login.DoGoogleLogin(Settings.GoogleUsername, Settings.GooglePassword);                
                
                while (true)
                {
                    if(publishEnabled)
                    {
                        await PublishingAccount(client);
                        await Task.Delay(15000);
                        System.Console.ReadLine();
                    }
                    else if(Settings.DratiniMode)
                    {
                        _inventory = await client.Inventory.GetInventory();
                        var pokemons = _inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData).Where(p => p != null);
                        var pokeUpgrades = _inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.InventoryUpgrades).Where(p => p != null).SelectMany(x => x.InventoryUpgrades_).Where(x => x.UpgradeType == InventoryUpgradeType.IncreasePokemonStorage).Count();
                        myMaxPokemon = basePokemonCount + pokeUpgrades * 50;

                        System.Console.WriteLine($"PokemonCount:" + pokemons.Count() + " Out of " + myMaxPokemon);


                        if (pokemons.Count() >= myMaxPokemon - 20)
                        {
                            await TransferAllButStrongestUnwantedPokemon(client);
                        }

                        await ExecuteFarmingDratinis(client);
                    }
                    else if(Settings.RenameMode)
                    {
                        await RenameAllPokemons(client);
                    }
                    else
                    {
                        await ExecuteFarmingPokestopsAndPokemons(client);
                    }
                    
                    System.Console.WriteLine("Resetting Player Position");
                    var update = await client.Player.UpdatePlayerLocation(Settings.DefaultLatitude, Settings.DefaultLongitude, 100);
                    await RecycleItems(client);
                    if(Settings.DratiniMode)
                    {
                        await Task.Delay(1000);
                    }
                    else
                    {
                        await Task.Delay(15000);
                    }                
                    //If timer hits 30 mins reset
                    if(_timer.ElapsedMilliseconds > 1800000)
                    {
                        System.Console.WriteLine("Resetting to refresh timeout");
                        await Task.Delay(5000);
                        _timer.Reset();
                        goto ReExecute;
                    }   
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"Exception occurred, Restarting..");
                await Task.Delay(10000);
                goto ReExecute;
            }
        }

        private static async Task RenameAllPokemons(Client client)
        {
            var inventory = await client.Inventory.GetInventory();
            if (inventory != null && inventory.InventoryDelta != null)
            {
                var pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData).Where(p => p != null && p.PokemonId != null);
                foreach(var pokemon in pokemons.Where(x => x.Nickname != x.PokemonId.ToString()))
                {
                    System.Console.WriteLine($"Renaming Pokemon {pokemon.PokemonId}");
                    await client.Inventory.NicknamePokemon(pokemon.Id, pokemon.PokemonId.ToString());
                    
                }
            }
        }

        private static async Task ExecuteFarmingDratinis(Client client)
        {
            var mapObjects = await client.Map.GetMapObjects();
            foreach (var coord in Common.Coordinates)
            {
                var update = await client.Player.UpdatePlayerLocation(coord.Item1, coord.Item2, 100);
                
                await ExecuteCatchAllNearbyPokemons(client);
            }
        }

        private static async Task ExecuteFarmingPokestopsAndPokemons(Client client)
        {
            var mapObjects = await client.Map.GetMapObjects();

            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts).Where(i => i.Type == FortType.Checkpoint && i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime());
            pokeStops = Helper.SortRoute(pokeStops.ToList());

            int waitCount = 0;

            foreach (var pokeStop in pokeStops)
            {
                var update = await client.Player.UpdatePlayerLocation(pokeStop.Latitude, pokeStop.Longitude, 100);
                var fortInfo = await client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);                
                var fortSearch = await client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                
                if (fortSearch != null)
                {
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Farmed XP: {fortSearch.ExperienceAwarded}, Gems: { fortSearch.GemsAwarded}, Eggs: {fortSearch.PokemonDataEgg} Items: {Helper.GetFriendlyItemsString(fortSearch.ItemsAwarded)}");
                    _inventory = await client.Inventory.GetInventory();
                    if (_inventory != null && _inventory.InventoryDelta != null)
                    {
                        var pokemons = _inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData).Where(p => p != null);
                        var playerData = _inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PlayerStats).Where(p => p != null && p?.Level > 0);
                        var pData = playerData.FirstOrDefault();
                        var pokeUpgrades = _inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.InventoryUpgrades).Where(p => p != null).SelectMany(x => x.InventoryUpgrades_).Where(x => x.UpgradeType == InventoryUpgradeType.IncreasePokemonStorage).Count();
                        myMaxPokemon = basePokemonCount + pokeUpgrades * 50;

                        System.Console.WriteLine($"PokemonCount:" + pokemons.Count() + " Out of " + myMaxPokemon);

                        if (pData != null)
                        {
                            System.Console.Title = string.Format("{0} level {1:0} - ({2:0} / {3:0})",
                              Settings.PtcUsername,+pData.Level,
                             +(pData.Experience - pData.PrevLevelXp),
                              +(pData.NextLevelXp - pData.PrevLevelXp));
                            if(pData.Level >= Settings.PublishLevel)
                            {
                                publishEnabled = true;
                            }
                        }


                        if (pokemons.Count() >= myMaxPokemon-20)
                        {
                            await TransferAllButStrongestUnwantedPokemon(client);
                        }

                        if(pokeStop.LureInfo != null)
                        {
                            await ExecuteCatchLurePokemonsTask(client, pokeStop);
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

                await Task.Delay(3000);
            }
        }

        private static async Task ExecuteCatchLurePokemonsTask(Client client, FortData fort)
        {
            System.Console.WriteLine("Looking for lure pokemon..");

            var fortId = fort.Id;

            var pokemonId = fort.LureInfo.ActivePokemonId;

            var encounterId = fort.LureInfo.EncounterId;
            var encounter = await client.Encounter.EncounterLurePokemon(encounterId, fortId);

            if (encounter.Result == DiskEncounterResponse.Types.Result.Success)
            {
                var berryUsed = false;
                var inventoryBalls = _inventory;
                
                var items = inventoryBalls.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData.Item).Where(p => p != null);
                CatchPokemonResponse caughtPokemonResponse;
                var pokeBall = await Helper.GetBestBall(encounter.PokemonData.Cp, inventoryBalls);
                do
                {
                    if (!berryUsed && Common.BerryPokemons.Contains(pokemonId) && items.Where(p => p.ItemId == ItemId.ItemRazzBerry).Count() > 0 && items.Where(p => p.ItemId == ItemId.ItemRazzBerry).First().Count > 0)
                    {
                        berryUsed = true;
                        System.Console.Write($"Use Rasperry (" + items.Where(p => p.ItemId == ItemId.ItemRazzBerry).First().Count + ")!");
                        await client.Encounter.UseCaptureItem(encounterId, ItemId.ItemRazzBerry, fort.Id);
                        //await Task.Delay(3000);
                    }
                    caughtPokemonResponse = await client.Encounter.CatchPokemon(encounterId, fort.Id, pokeBall);
                    
                } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed || caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);

                string Iv = Math.Round(PokemonInfo.CalculatePokemonPerfection(encounter?.PokemonData), 2).ToString();

                if (Settings.UsingIV && caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                {
                    await Task.Delay(500);
                    await client.Inventory.NicknamePokemon(caughtPokemonResponse.CapturedPokemonId, Iv);

                }

                System.Console.WriteLine(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess ? 
                        $"[{DateTime.Now.ToString("HH:mm:ss")}] We caught a {pokemonId} with CP {encounter?.PokemonData?.Cp} IV {Iv}"
                        : $"[{DateTime.Now.ToString("HH:mm:ss")}] {pokemonId} got away.. ");
            }else
            {
                System.Console.WriteLine($"Encounter problem: Lure Pokemon {encounter.Result}");
            }
        }

        private static async Task ExecuteCatchAllNearbyPokemons(Client client)
        {
            var mapObjects = await client.Map.GetMapObjects();

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);

            if(Settings.DratiniMode)
            {
                pokemons = pokemons.Where(x => x.PokemonId == PokemonId.Dratini || x.PokemonId == PokemonId.Dragonair || x.PokemonId == PokemonId.Dragonite);
            }

            foreach (var pokemon in pokemons)
            {
                //var update = await client.Player.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude, 100);
                var encounterPokemonRespone = await client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);

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

                    string Iv = Math.Round(PokemonInfo.CalculatePokemonPerfection(encounterPokemonRespone?.WildPokemon?.PokemonData), 2).ToString();

                    if (Settings.UsingIV && caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                    {
                        await Task.Delay(500);
                        await client.Inventory.NicknamePokemon(caughtPokemonResponse.CapturedPokemonId, Iv);
                        
                    }

                    System.Console.WriteLine(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess 
                        ? $"[{DateTime.Now.ToString("HH:mm:ss")}] We caught a {pokemon.PokemonId} with CP {encounterPokemonRespone?.WildPokemon?.PokemonData?.Cp}  IV {Iv}" 
                        : $"[{DateTime.Now.ToString("HH:mm:ss")}] {pokemon.PokemonId} got away..");
                }
                if (Settings.DratiniMode)
                {
                    var families = await Helper.GetPokemonFamilies(inventoryBalls);
                    System.Console.Title = string.Format($"{Settings.PtcUsername} Dratini Candy: {families.First(x => x.FamilyId == PokemonFamilyId.FamilyDratini)?.Candy}");
                    _inventory = await client.Inventory.GetInventory();
                }

                //await Task.Delay(5000);
            }
        }

        private static async Task TransferAllButStrongestUnwantedPokemon(Client client)
        {
            System.Console.WriteLine("Transferring Pokemon");
            var pokemonTypes = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>();
            var inventory = await client.Inventory.GetInventory();
            var pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData).Where(p => p != null && p?.PokemonId > 0);

            foreach (var unwantedPokemonType in pokemonTypes.Where(x => !Common.WantedPokemons.Contains(x)))
            {
                var pokemonOfDesiredType = pokemons.Where(p => p.PokemonId == unwantedPokemonType)
                                                   .OrderByDescending(p => Settings.UsingIV ? PokemonInfo.CalculatePokemonPerfection(p) : p.Cp)
                                                   .ToList();

                var unwantedPokemon = new List<PokemonData>();

                if (unwantedPokemonType == PokemonId.Dratini || unwantedPokemonType == PokemonId.Eevee)
                {
                    if (pokemonOfDesiredType.Count > 4)
                    {
                        unwantedPokemon = pokemonOfDesiredType.Skip(4) // keep the strongest one for potential battle-evolving
                                                          .ToList();
                    }
                }
                else
                {
                    unwantedPokemon = pokemonOfDesiredType.Skip(1) // keep the strongest one for potential battle-evolving
                                                          .ToList();
                }

                System.Console.WriteLine($"Transfering {unwantedPokemon.Count} pokemons of type {unwantedPokemonType}");
                await TransferAllGivenPokemons(client, unwantedPokemon);
            }

            System.Console.WriteLine("[!] finished Transferring");
        }

        private static async Task TransferAllGivenPokemons(Client client, List<PokemonData> unwantedPokemons)
        {
            foreach (var pokemon in unwantedPokemons)
            {
                var transferPokemonResponse = await client.Inventory.TransferPokemon(pokemon.Id);

                /*
                ReleasePokemonOutProto.Status {
                   UNSET = 0;
                   SUCCESS = 1;
                   POKEMON_DEPLOYED = 2;
                   FAILED = 3;
                   ERROR_POKEMON_IS_EGG = 4;
                }*/

                if (transferPokemonResponse.Result == ReleasePokemonResponse.Types.Result.Success)
                {
                    System.Console.WriteLine($"Transferred {pokemon.PokemonId}");
                }
                else
                {
                    var status = transferPokemonResponse.Result;

                    System.Console.WriteLine($"Somehow failed to transfer {pokemon.PokemonId}. ReleasePokemonOutProto.Status was {status.ToString()}");
                }

                
            }
        }

        private static async Task RecycleItems(Client client)
        {
            var inventory = await client.Inventory.GetInventory();

            var itemRecycleList = new List<ItemId>
            {
                ItemId.ItemSuperPotion,
                ItemId.ItemPotion,
                ItemId.ItemRevive,
                ItemId.ItemMaxPotion,
                ItemId.ItemMaxRevive,
                ItemId.ItemHyperPotion,
                ItemId.ItemRazzBerry
            };

            var items = inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.Item)
                .Where(p => p != null && itemRecycleList.Contains(p.ItemId));

            foreach (var item in items.Where(x => x.Count > 0))
            {
                var transfer = await client.Inventory.RecycleItem((ItemId)item.ItemId, item.Count);
                System.Console.WriteLine($"Recycled {item.Count}x {(ItemId)item.ItemId}");
                
            }
        }
        private static async Task PublishingAccount(Client client)
        {
            System.Console.WriteLine("Publishing Account");            
            var inventory = await client.Inventory.GetInventory();
            var pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData).Where(p => p != null && p?.PokemonId > 0);
            var firstEvolvePokemons = pokemons.Where(x => Common.EvolveLevelOnePokemons.Contains(x.PokemonId)).OrderByDescending(x => x.Cp).GroupBy(x => x.PokemonId).ToArray();

            System.Console.WriteLine("Level one Evolving");

            foreach (var monType in firstEvolvePokemons)
            {
                var mon = monType.First();
                EvolvePokemonResponse response;
                do
                {
                    response = await client.Inventory.EvolvePokemon(mon.Id);
                } while (response.Result != EvolvePokemonResponse.Types.Result.Success && response.Result != EvolvePokemonResponse.Types.Result.FailedInsufficientResources);
                System.Console.WriteLine($"Successfully evolved {mon.PokemonId} CP {mon.Cp}");
            }

            await Task.Delay(5000);
            inventory = await client.Inventory.GetInventory();
            pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData).Where(p => p != null && p?.PokemonId > 0);
            var secondEvolvePokemons = pokemons.Where(x => Common.EvolveLevelTwoPokemons.Contains(x.PokemonId)).OrderByDescending(x => x.Cp).GroupBy(x => x.PokemonId).ToArray();

            System.Console.WriteLine("Level two Evolving");

            foreach (var monType in secondEvolvePokemons)
            {
                var mon = monType.First();
                EvolvePokemonResponse response;
                do
                {
                    response = await client.Inventory.EvolvePokemon(mon.Id);
                } while (response.Result != EvolvePokemonResponse.Types.Result.Success && response.Result != EvolvePokemonResponse.Types.Result.FailedInsufficientResources);
                System.Console.WriteLine($"Successfully evolved {mon.PokemonId} CP {mon.Cp}");
            }

            await Task.Delay(5000);
            inventory = await client.Inventory.GetInventory();
            pokemons = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData).Where(p => p != null && p?.PokemonId > 0);
            var powerUpMons = pokemons.Where(x => Common.PublishingPowerUpPokemons.Contains(x.PokemonId)).OrderByDescending(x => x.Cp).GroupBy(x => x.PokemonId).ToArray();

            /*
            System.Console.WriteLine("Starting Powering");

            //POWERING NOT WORKING
            foreach (var monType in powerUpMons)
            {
                var mon = monType.First();
                UpgradePokemonResponse response;

                do
                {
                    response = await client.Inventory.UpgradePokemon(mon.Id);
                    System.Console.WriteLine($"Powered {mon.PokemonId} CP {mon.Cp}");
                } while (response.Result != UpgradePokemonResponse.Types.Result.Success);

                System.Console.WriteLine($"Successfully Powered {mon.PokemonId} CP {mon.Cp}");
            }
            */

            System.Console.WriteLine("[!] finished Publishing");
            await Task.Delay(5000);
        }
    }
}
