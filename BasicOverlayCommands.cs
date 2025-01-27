using BetterCommands;
using BetterCommands.Permissions;
using PluginAPI.Core;
using SmartOverlays;
using static SmartOverlays.OverlayManager;

namespace BasicOverlay {
    public static class BasicOverlayCommands {
        private static Overlay templateOverlay = new OverlayTemplate();

        [Command("toggletemplate", CommandType.RemoteAdmin)]
        [CommandAliases("toggletem")]
        [Description("Enable/disable Template Overlay")]
        [Permission(PermissionLevel.Lowest)]
        public static string ToggleTemplateCmd(Player sender) {
            var hub = sender.ReferenceHub;
            if (hub.IsOverlayRegistered(templateOverlay)) {
                hub.UnregisterOverlay(templateOverlay);
                return $"You were succesfully Unregistered from Teplate Overlay";
            }
            hub.RegisterOverlay(templateOverlay);
            return $"You were succesfully Registered to Teplate Overlay";
        }

        [Command("toggleinventory", CommandType.RemoteAdmin)]
        [CommandAliases("toggleinv", "tinv")]
        [Description("Toggles Inventory overlay")]
        [Permission(PermissionLevel.Lowest)]
        public static string ToggleInvCmd(Player sender) {
            var hub = sender.ReferenceHub;
            if (!BasicOverlayLogic.overwatchOverlays.ContainsKey(hub.netId)) return $"Player {sender.Nickname} is not registered!";
            if (hub.IsOverlayRegistered(BasicOverlayLogic.overwatchOverlays[hub.netId])) {
                hub.UnregisterOverlay(BasicOverlayLogic.overwatchOverlays[hub.netId]);
                return "Inventory view succesfully hidden.";
            }
            hub.RegisterOverlay(BasicOverlayLogic.overwatchOverlays[hub.netId]);
            return "Inventory view Succesfully resored.";
        }

        /*
        [BetterCommands.Command("showobject", CommandType.RemoteAdmin)]
        [Description("Show object that you are looking at")]
        public static string ShowObject(Player sender) {
            
            GameObject playerObject = sender.GameObject;
            float distance = 5f;
            RaycastHit hit;
            string message = $"{playerObject.name}: ";
            if (Physics.Raycast(playerObject.transform.position, playerObject.transform.forward, out hit, distance)) {
                message += hit.collider.gameObject.name;
            } else {
                message += "nothing";
            }
            return message;
        }

        [BetterCommands.Command("test", CommandType.RemoteAdmin)]
        [Description("test")]
        public static string TestCmd(Player sender) {
            RoomIdentifier roomIdentifier = RoomIdUtils.RoomAtPosition(sender.ReferenceHub.transform.position);
            Plugin.Info(roomIdentifier.ToJson());
            return roomIdentifier.ToJson();
        }


        [BetterCommands.Command("res", CommandType.RemoteAdmin)]
        [Description("resolution")]
        public static string ResolutionCmd(Player sender) {

            var sync = sender.ReferenceHub.aspectRatioSync;
            sync.UpdateAspectRatio();
            return $"{sync._savedWidth} x {sync._savedHeight} ({sync.AspectRatio} = {sync.XplusY}, {sync.XScreenEdge})";
        }
        */



        /*
        [Command("showhint", CommandType.RemoteAdmin)]
        [Description("Show code of hint")]
        public static string ShowHintCmd(Player sender) {
            return "";
        }
        */
    }
}
