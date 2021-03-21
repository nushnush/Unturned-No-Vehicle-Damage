using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace NoVehicleDamage
{
    class NoVehicleDamage : RocketPlugin
    {
        private List<ulong> messageCooldown = new List<ulong>();
        private List<Info> damagedOwners = new List<Info>();
        private float timeRemaining = 2;

        protected override void Load()
        {
            VehicleManager.onDamageVehicleRequested += new DamageVehicleRequestHandler(OnVehicleDamage);
            VehicleManager.onDamageTireRequested += new DamageTireRequestHandler(OnTireDamage);
            Provider.onServerConnected += new Provider.ServerConnected(onServerConnected);
        }

        protected override void Unload()
        {
            VehicleManager.onDamageVehicleRequested -= new DamageVehicleRequestHandler(OnVehicleDamage);
            VehicleManager.onDamageTireRequested -= new DamageTireRequestHandler(OnTireDamage);
            Provider.onServerConnected -= new Provider.ServerConnected(onServerConnected);
        }

        void Update()
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                messageCooldown.Clear();
                timeRemaining = 2;
            }
        }

        private void onServerConnected(CSteamID steamID)
        {
            Info info = damagedOwners.FirstOrDefault(x => x.vehicleOwner == steamID);
            while (info != null)
            {
                damagedOwners.Remove(info);
                UnturnedChat.Say(steamID, info.attacker + " Tried to damage your vehicle.");
                info = damagedOwners.FirstOrDefault(x => x.vehicleOwner == steamID);
            }
        }

        public void OnVehicleDamage(CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalDamage, ref bool canRepair, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (vehicle.isLocked && PlayerTool.getPlayer(vehicle.lockedOwner) == null)
            {
                if (!messageCooldown.Contains(instigatorSteamID.m_SteamID))
                {
                    UnturnedChat.Say(instigatorSteamID, "You cannot harm this vehicle when its owner is offline.");
                    messageCooldown.Add(instigatorSteamID.m_SteamID);
                }

                Info info = new Info { attacker = UnturnedPlayer.FromCSteamID(instigatorSteamID).CharacterName, vehicleOwner = vehicle.lockedOwner };
                if (!damagedOwners.Any(x => x.attacker == info.attacker))
                {
                    damagedOwners.Add(info);
                }

                shouldAllow = false;
            }
        }

        public void OnTireDamage(CSteamID instigatorSteamID, InteractableVehicle vehicle, int tireIndex, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (vehicle.isLocked && PlayerTool.getPlayer(vehicle.lockedOwner) == null)
            {
                shouldAllow = false;
            }
        }
    }

    public class Info
    {
        public string attacker;
        public CSteamID vehicleOwner;
    }
}