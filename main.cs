using Life;
using Life.BizSystem;
using Life.CheckpointSystem;
using Life.DB;
using Life.Network;
using Life.UI;
using Life.VehicleSystem;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HueGoFast
{
    public class HueGoFast : HueHelper.AllHelper
    {
        public HueGoFast(IGameAPI aPI) : base(aPI) { }

        List<string> PlayerInGoFast = new List<string>();

        public static string SuccessColors = "#85E085";
        public static string ErrorColors = "#DD4B4E";
        public static string WarningColors = "#FCBE86";
        public static string InfoColors = "#4287F9";
        public static string GreyColors = "#ADADAD";
        public static string PurpleColors = "#DB70DB";

        public static Config config;

        public class Config
        {
            public int Reward;

            public int MinPolicier;

            public int NombreDeCannabis;
        }

        public void CreateConfig()
        {
            string directoryPath = pluginsPath + "/HueGofast";

            string configFilePath = directoryPath + "/config.json";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new Config
                {
                    Reward = 3000,

                    MinPolicier = 1,

                    NombreDeCannabis = 10,

                };
                string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(defaultConfig, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configFilePath, jsonContent);
            }

            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFilePath));
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();

            CreateConfig();

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine("[HueGofast V.1.0.0] initialized success");

            Console.ForegroundColor = ConsoleColor.White;
        }



        public override void OnPlayerSpawnCharacter(Player player, NetworkConnection conn, Characters character)
        {
            base.OnPlayerSpawnCharacter(player, conn, character);

            if (Nova.serverInfo.mapId == 0)
            {

                NCheckpoint RequestGoFast = new NCheckpoint(player.netId, new Vector3(885.6271f, 52.39577f, -984.6579f), NAction =>
                {
                    OnEnterBluePoints(player);
                });
                player.CreateCheckpoint(RequestGoFast);

            }
            else
            {
                NCheckpoint RequestGoFast = new NCheckpoint(player.netId, new Vector3(535.0926f, 50.00305f, 722.0909f), NAction =>
                {
                    OnEnterBluePointsVSB(player);
                });
                player.CreateCheckpoint(RequestGoFast);

            }
        }

        public void OnEnterBluePoints(Player player)
        {
            foreach (var players in Nova.server.Players)
            {
                int count = 0;

                if (players.biz.IsActivity(Activity.Type.LawEnforcement) && players.serviceMetier) 
                {
                    count++;
                }

                if (count < config.MinPolicier)
                {
                    player.SendText($"<color={ErrorColors}>[HueGofast]</color> Il n'y a pas assez de policiers en ville en service !");
                }
                else
                {
                    if (!PlayerInGoFast.Contains(player.GetFullName()))
                    {
                        UIPanel panel = new UIPanel("Gofast - By Zerox_Hue", UIPanel.PanelType.TabPrice);

                        panel.AddTabLine("Drogues", "", IconHelper.GetItemIcon(28), ui =>
                        {
                            StartGoFast(player);

                        });

                        PanelHelper.CloseButton(player, panel);

                        PanelHelper.ValidButton(player, panel);

                        PanelHelper.ShowPanel(player, panel);



                    }
                    else
                    {
                       player.SendText($"<color={ErrorColors}>Tu es déja dans un Gofast !");
                    }
                }
            }


        }

        public void OnEnterBluePointsVSB(Player player)
        {
            foreach (var players in Nova.server.Players)
            {
                int count = 0;

                if(players.biz.IsActivity(Activity.Type.LawEnforcement) && players.serviceMetier)
                {
                    count++;
                }

                if (count < config.MinPolicier)
                {
                    player.SendText($"<color={ErrorColors}>[HueGofast]</color> Il n'y a pas assez de policiers en ville !");
                }
                else
                {
                    if (!PlayerInGoFast.Contains(player.GetFullName()))
                    {
                        UIPanel panel = new UIPanel("Gofast - By Zerox_Hue", UIPanel.PanelType.TabPrice);

                        panel.AddTabLine("Drogues", "", IconHelper.GetItemIcon(28), ui =>
                        {
                            StartGoFastVSB(player);

                        });

                        PanelHelper.CloseButton(player, panel);

                        PanelHelper.ValidButton(player, panel);

                        PanelHelper.ShowPanel(player, panel);
                    }
                    else
                    {
                        player.SendText($"<color={ErrorColors}>Tu es déja dans un Gofast !");
                    }

                }
            }


        }

        public async void StartGoFastVSB(Player player)
        {
            if (player.setup.inventory.items[player.setup.inventory.GetItemSlotById(28)].number >= config.NombreDeCannabis)
            {
                PlayerInGoFast.Add(player.GetFullName());

                Vector3 FirstpointV3 = new Vector3(707.4426f, 50.00305f, 670.4072f);

                player.setup.inventory.RemoveItem(28, config.NombreDeCannabis, true);

                player.setup.TargetSetGPSTarget(FirstpointV3);

                player.Notify("Gofast", "Gofast commencé avec succés !", NotificationManager.Type.Success);


                NVehicleCheckpoint Firstpoint = new NVehicleCheckpoint(player.netId, FirstpointV3, (Action<NVehicleCheckpoint, uint>)(async (checkpoint, someUint) =>
                {
                    player.DestroyAllVehicleCheckpoint();

                    LifeVehicle lifevehicle = Nova.v.GetVehicle(player.setup.driver.vehicle.VehicleDbId);

                    Nova.server.SendTextToServiceActivityType(Life.BizSystem.Activity.Type.LawEnforcement, $"<color={ErrorColors}>[HueGoFast]</color> Une voiture avec comme plaque : <color={PurpleColors}>{player.setup.driver.vehicle.Networkplate} </color>vient de comencer un Gofast et comme modéle : {VehiclesHelper.GetModelName(lifevehicle.modelId)} !");

                    await Task.Delay(1);

                    SecondPointVSB(player);

                }));
                player.CreateVehicleCheckpoint(Firstpoint);

                await Task.Delay(210000);

                PlayerInGoFast.Remove(player.GetFullName());

                player.setup.TargetDisableNavigation();

                player.DestroyAllVehicleCheckpoint();

            }
            else
            {
                player.SendText($"<color={ErrorColors}>[HueGofast]</color> Tu n'as pas {config.NombreDeCannabis.ToString()} feuilles de cannabis !");
            }
        }

        public async void StartGoFast(Player player)
        {
            if (player.setup.inventory.items[player.setup.inventory.GetItemSlotById(28)].number >= config.NombreDeCannabis)
            {
                PlayerInGoFast.Add(player.GetFullName());

                Vector3 FirstpointV3 = new Vector3(946.557f, 52.39577f, -661.5795f);

                player.setup.inventory.RemoveItem(28, config.NombreDeCannabis, true);

                player.setup.TargetSetGPSTarget(FirstpointV3);

                player.Notify("Gofast", "Gofast commencé avec succés !", NotificationManager.Type.Success);


                NVehicleCheckpoint Firstpoint = new NVehicleCheckpoint(player.netId, FirstpointV3, (Action<NVehicleCheckpoint, uint>)(async (checkpoint, someUint) =>
                {
                    player.DestroyAllVehicleCheckpoint();

                    LifeVehicle lifevehicle = Nova.v.GetVehicle(player.setup.driver.vehicle.VehicleDbId);

                     Nova.server.SendTextToServiceActivityType(Life.BizSystem.Activity.Type.LawEnforcement, $"<color={ErrorColors}>[HueGoFast]</color> Une voiture avec comme plaque : <color={PurpleColors}>{player.setup.driver.vehicle.Networkplate} </color>vient de comencer un Gofast et comme modéle : {VehiclesHelper.GetModelName(lifevehicle.modelId)} !");

                    await Task.Delay(1);

                    SecondPoint(player);

                }));
                player.CreateVehicleCheckpoint(Firstpoint);

                await Task.Delay(210000);

                player.DestroyAllVehicleCheckpoint();

                player.setup.TargetDisableNavigation();

            }
            else
            {
                player.SendText($"<color={ErrorColors}>[HueGofast]</color> Tu n'as pas {config.NombreDeCannabis} feuilles de cannabis !");
            }
        }

        public void SecondPointVSB(Player player)
        {
            Vector3 SecondpointV3 = new Vector3(362.0558f, 45.53201f, 716.9189f);


            player.setup.TargetSetGPSTarget(SecondpointV3);

            NVehicleCheckpoint Secondpoint = new NVehicleCheckpoint(player.netId, SecondpointV3, (Action<NVehicleCheckpoint, uint>)(async (checkpoint, someUint) =>
            {
                player.DestroyAllVehicleCheckpoint();

                await Task.Delay(1);

                ThirdPointVSB(player);

            }));
            player.CreateVehicleCheckpoint(Secondpoint);


        }



        public void SecondPoint(Player player)
        {
            Vector3 SecondpointV3 = new Vector3(513.6871f, 50.00305f, 103.5304f);


            player.setup.TargetSetGPSTarget(SecondpointV3);

            NVehicleCheckpoint Secondpoint = new NVehicleCheckpoint(player.netId, SecondpointV3, (Action<NVehicleCheckpoint, uint>)(async (checkpoint, someUint) =>
            {
                player.DestroyAllVehicleCheckpoint();

                await Task.Delay(1);

                ThirdPoint(player);

            }));
            player.CreateVehicleCheckpoint(Secondpoint);


        }

        public void ThirdPointVSB(Player player)
        {
            Vector3 ThirdpointV3 = new Vector3(117.5485f, 44.99786f, 359.7607f);


            player.setup.TargetSetGPSTarget(ThirdpointV3);

            NVehicleCheckpoint Thirdpoint = new NVehicleCheckpoint(player.netId, ThirdpointV3, (Action<NVehicleCheckpoint, uint>)(async (checkpoint, someUint) =>
            {
                player.DestroyAllVehicleCheckpoint();

                await Task.Delay(1);

                FinalPointVSB(player);

            }));
            player.CreateVehicleCheckpoint(Thirdpoint);



        }

        public void ThirdPoint(Player player)
        {
            Vector3 ThirdpointV3 = new Vector3(389.1335f, 49.99084f, 697.9017f);


            player.setup.TargetSetGPSTarget(ThirdpointV3);

            NVehicleCheckpoint Thirdpoint = new NVehicleCheckpoint(player.netId, ThirdpointV3, (Action<NVehicleCheckpoint, uint>)(async (checkpoint, someUint) =>
            {
                player.DestroyAllVehicleCheckpoint();

                await Task.Delay(1);

                FinalPoint(player);

            }));
            player.CreateVehicleCheckpoint(Thirdpoint);

        }

        public void FinalPointVSB(Player player)
        {

            Vector3 FinalpointV3 = new Vector3(390.7353f, 44.99786f, 466.5557f);


            player.setup.TargetSetGPSTarget(FinalpointV3);

            NVehicleCheckpoint Finalpoint = new NVehicleCheckpoint(player.netId, FinalpointV3, (Action<NVehicleCheckpoint, uint>)(async (checkpoint, someUint) =>
            {
                player.DestroyAllVehicleCheckpoint();

                player.setup.TargetShowCenterText($"<color={ErrorColors}>GO FAST</color>", "Tu as réussi le GoFast sans te faire attraper !", 5f);

                player.setup.NetworkisFreezed = true;

                await Task.Delay(5000);

                player.setup.NetworkisFreezed = false;

                PlayerInGoFast.Remove(player.GetFullName());

                player.AddMoney(config.Reward, "Gofast"); 

                player.SendText($"<color={SuccessColors}>GoFast Réussi avec succés tu as empoché {config.Reward} € !");

                player.setup.TargetDisableNavigation();


            }));
            player.CreateVehicleCheckpoint(Finalpoint);



        }

        public void FinalPoint(Player player)
        {

            Vector3 FinalpointV3 = new Vector3(707.7318f, 50.00305f, 991.9352f);


            player.setup.TargetSetGPSTarget(FinalpointV3);

            NVehicleCheckpoint Finalpoint = new NVehicleCheckpoint(player.netId, FinalpointV3, (Action<NVehicleCheckpoint, uint>)(async (checkpoint, someUint) =>
            {
                player.DestroyAllVehicleCheckpoint();

                player.setup.TargetShowCenterText($"<color={ErrorColors}>GO FAST</color>", "Tu as réussi le GoFast sans te faire attraper !", 5f);

                player.setup.NetworkisFreezed = true;

                await Task.Delay(5000);

                player.AddMoney(config.Reward, "Gofast");

                player.setup.NetworkisFreezed = false;

                player.SendText($"<color={SuccessColors}>GoFast Réussi avec succés tu as empoché {config.Reward} € !");

                PlayerInGoFast.Remove(player.GetFullName());

                player.setup.TargetDisableNavigation();


            }));
            player.CreateVehicleCheckpoint(Finalpoint);




        }

    }
}