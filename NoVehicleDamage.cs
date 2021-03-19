using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Rocket.Unturned.Permissions;

namespace NoVehicleDamage
{
    class NoVehicleDamage : RocketPlugin
    {
        private List<CSteamID> messageCooldown = new List<CSteamID>();
        private List<Info> damagedOwners = new List<Info>();

        protected override void Load()
        {
            VehicleManager.onDamageVehicleRequested += new DamageVehicleRequestHandler(OnVehicleDamage);
            UnturnedPermissions.OnJoinRequested += new UnturnedPermissions.JoinRequested(OnPlayerConnect);
            StartCoroutine(ClearList());
        }

        IEnumerator ClearList()
        {
            messageCooldown.Clear();
            yield return new WaitForSeconds(2f);
        }

        protected override void Unload()
        {
            VehicleManager.onDamageVehicleRequested -= new DamageVehicleRequestHandler(OnVehicleDamage);
            UnturnedPermissions.OnJoinRequested -= new UnturnedPermissions.JoinRequested(OnPlayerConnect);
            StopCoroutine(ClearList());
        }

        public void OnPlayerConnect(CSteamID Player, ref ESteamRejection? rejection)
        {
            Info info = damagedOwners.FirstOrDefault(x => x.vehicleOwner == Player);
            while (info != null)
            {
                damagedOwners.Remove(info);
                UnturnedChat.Say(Player, "Your vehicle has been damaged by " + info.attacker + ".");
                info = damagedOwners.FirstOrDefault(x => x.vehicleOwner == Player);
            }
        }
        
        public void OnVehicleDamage(CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalDamage, ref bool canRepair, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if(vehicle.lockedOwner.m_SteamID > 0 && UnturnedPlayer.FromCSteamID(vehicle.lockedOwner) == null)
            {
                if(!messageCooldown.Contains(instigatorSteamID))
                {
                    UnturnedChat.Say(instigatorSteamID, "You cannot harm this vehicle when its owner is offline.");
                    messageCooldown.Add(instigatorSteamID);
                }
                
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
