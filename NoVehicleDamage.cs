using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;

namespace NoVehicleDamage
{
    class NoVehicleDamage : RocketPlugin
    {
        protected override void Load()
        {
            VehicleManager.onDamageVehicleRequested += new DamageVehicleRequestHandler(OnVehicleDamage);
        }

        protected override void Unload()
        {
            VehicleManager.onDamageVehicleRequested -= new DamageVehicleRequestHandler(OnVehicleDamage);
        }

        public void OnVehicleDamage(CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalDamage, ref bool canRepair, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if(UnturnedPlayer.FromCSteamID(vehicle.lockedOwner) == null)
            {
                UnturnedChat.Say(instigatorSteamID, "You cannot harm this vehicle when its owner is offline.");
                shouldAllow = false;
            }
        }
    }
}
