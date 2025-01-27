using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using helpers;
using Compendium;
using Compendium.Events;
using Compendium.Features;
using Hints;
using PlayerRoles;
using PlayerRoles.Spectating;
using PluginAPI.Core;
using PluginAPI.Events;

using SmartOverlays;
using static SmartOverlays.OverlayManager;

using System.Linq;

//using System.Data;

namespace BasicOverlay {
    public static class BasicOverlayLogic {
        public static OverlaySpectator spectatorOverlay;
        public static OverlaySCP scpOverlay;

        public static Dictionary<uint, OverlayOverwatch> overwatchOverlays;
        public static Dictionary<uint, OverlayEffects> effectsOverlays;
        //public static Dictionary<Player, OverlayWatchers> watchersOverlays; //TODO: mazat overlaye


        //private static bool _enableForAll = false;
        private static Action _updateMessages = new Action(BasicOverlayLogic.UpdateMessages);

        /*
        //, 76561198928711263@steam", "76561198018462506@steam
        [ConfigAttribute(Name = "TestMembers", Description = "A list of Test Members, which an see Overlay")]
        public static HashSet<string> TestList { get; set; } = new HashSet<string>() { "76561198179927773@steam" };

        [ConfigAttribute(Name = "EnableForAll", Description = "Enable Overlays to All players")]
        public static bool EnableForAll {
            get => _enableForAll;
            set {
                if (_enableForAll == value) return;
                _enableForAll = value;
                if (!_enableForAll) UnregisterAllPlayers();
            }
        }
        */

        public static void Load() {
            //EventRegistry.RegisterEvents(Reflection.Method(typeof(BasicOverlayLogic), nameof(BasicOverlayLogic.ChangeRole)), true);
            //EventRegistry.RegisterEvents(Reflection.Method(typeof(BasicOverlayLogic), nameof(BasicOverlayLogic.ChangeSpectator)), true);
            //EventRegistry.RegisterEvents(Reflection.Method(typeof(BasicOverlayLogic), nameof(BasicOverlayLogic.PlayerLeft)), true);
            OverlayManager.RegisterEvents();
            OverlayManager.PreDisplayEvent.Register(_updateMessages);
            Reload();
        }

        public static void Unload() {
            //EventRegistry.UnregisterEvents(Reflection.Method(typeof(BasicOverlayLogic), nameof(BasicOverlayLogic.ChangeSpectator)), true);
            //EventRegistry.UnregisterEvents(Reflection.Method(typeof(BasicOverlayLogic), nameof(BasicOverlayLogic.ChangeRole)), true);
            //EventRegistry.UnregisterEvents(Reflection.Method(typeof(BasicOverlayLogic), nameof(BasicOverlayLogic.PlayerLeft)), true);
            OverlayManager.UnregisterEvents();
            OverlayManager.PreDisplayEvent.Unregister(_updateMessages);
            OverlayManager.ClearHints();
        }

        public static void Reload() {
            ResetOverlays();
            OverlayManager.ResetManager();

            foreach (var hub in ReferenceHub.AllHubs.Where(hub => hub.IsPlayer())) {
                RegisterOverlays(hub, hub.roleManager.CurrentRole.RoleTypeId);
                hub.ForceRefreshHints();
            }
        }

        public static void ResetOverlays() {
            if (overwatchOverlays is null) overwatchOverlays = new Dictionary<uint, OverlayOverwatch>();
            else overwatchOverlays.Clear();
            if (effectsOverlays is null) effectsOverlays = new Dictionary<uint, OverlayEffects>();
            else effectsOverlays.Clear();
            //watchersOverlays = new Dictionary<Player, OverlayWatchers>();
            if (spectatorOverlay is null) spectatorOverlay = new OverlaySpectator();
            if (scpOverlay is null) scpOverlay = new OverlaySCP();
        }

        public static void UpdateMessages() {
            OverlayWatchers.UpdateAllMessages();
            spectatorOverlay.UpdateMessages();
            scpOverlay.UpdateMessages();

            foreach (var pair in overwatchOverlays) {
                pair.Value.UpdateMessages();
            }
            foreach (var pair in effectsOverlays) {
                pair.Value.UpdateMessages();
            }
        }

        [Event]
        public static void ChangeRole(PlayerChangeRoleEvent ev) {
            if (ev.OldRole.Equals(ev.NewRole)) return;
            RegisterOverlays(ev.Player.ReferenceHub, ev.NewRole);
            ev.Player.ReferenceHub.ForceRefreshHints();
        }

        public static void RegisterOverlays(ReferenceHub hub, RoleTypeId newRole) {
            //if (!TestList.Contains(ev.Player.UserId) && !EnableForAll) return;
            if (OverlayManager.debugInfo)
                FLog.Info($"ChangeRole: {hub.Nick()} ({hub.UserId()})");

            hub.hints.Show(new TextHint(String.Empty, new HintParameter[] { new StringHintParameter(String.Empty) }, null, 1f));

            hub.UnregisterOverlay(spectatorOverlay);
            hub.UnregisterOverlay(scpOverlay);
            if (overwatchOverlays.ContainsKey(hub.netId)) {
                hub.UnregisterOverlay(overwatchOverlays[hub.netId]);
                overwatchOverlays.Remove(hub.netId);
            }
            if (effectsOverlays.ContainsKey(hub.netId)) {
                hub.UnregisterOverlay(effectsOverlays[hub.netId]);
                effectsOverlays.Remove(hub.netId);
            }

            if (newRole.GetTeam() is Team.Dead) {
                OverlayWatchers.RemoveTarget(hub);
                if (newRole is RoleTypeId.Spectator) {
                    hub.RegisterOverlay(spectatorOverlay);
                } else if (newRole is RoleTypeId.Overwatch) {
                    hub.RegisterOverlay(spectatorOverlay);
                    RegisterToOw(hub);
                }
            } else { // is alive
                OverlayWatchers.RemoveWatcher(hub);
                OverlayWatchers.RegisterTarget(hub);
                if (IsRoleSCP(newRole)) {
                    hub.RegisterOverlay(scpOverlay);
                } else { // is not SCP
                    OverlayEffects effectOverlay = new OverlayEffects(hub);
                    hub.RegisterOverlay(effectOverlay);
                    effectsOverlays.Add(hub.netId, effectOverlay);
                }
                /* else if (ev.NewRole is RoleTypeId.NtfCaptain) { //TODO: smazat
                    spectatorOverlay.RegisterPlayer(ev.Player);
                    OverlayOverwatch overwatchOverlay = new OverlayOverwatch();
                    overwatchOverlay.player = ev.Player;
                    overwatchOverlay.RegisterPlayer(ev.Player);
                    overwatchOverlays.Add(ev.Player, overwatchOverlay);
                }*/
            }
            //FLog.Info(_classInfoMessage.message);
        }

        public static void RegisterToOw(ReferenceHub hub) {
            OverlayOverwatch overwatchOverlay = new OverlayOverwatch();
            SpectatorRole spectatorRole = hub.roleManager.CurrentRole as SpectatorRole;
            if (GetSpectatedPlayer(spectatorRole, out Player specPlayer))
                overwatchOverlay.TargetHub = specPlayer.ReferenceHub;
            hub.RegisterOverlay(overwatchOverlay);
            overwatchOverlays.Add(hub.netId, overwatchOverlay);
        }

        private static bool GetSpectatedPlayer(SpectatorRole specRole, out Player player) {
            ReferenceHub referenceHub;
            if (specRole == null || specRole.SyncedSpectatedNetId == 0 || !ReferenceHub.TryGetHubNetID(specRole.SyncedSpectatedNetId, out referenceHub)) {
                player = null;
                return false;
            }
            return Player.TryGet(referenceHub, out player);
        }

        // first target:  Mallifrey ＜3 (76561198179927773@steam): null -> fricksandals ＜3
        // manual switch: Mallifrey ＜3 (76561198179927773@steam): Mendy -> Marwim
        // target zemřel: Mallifrey ＜3 (76561198179927773@steam): Marwim -> null
        [Event]
        public static void ChangeSpectator(PlayerChangeSpectatorEvent ev) {
            if (!(ev.OldTarget is null) && ev.OldTarget.Equals(ev.NewTarget)) return;
            var hub = ev.Player.ReferenceHub;
            if (OverlayManager.debugInfo) {
                string oldt = ev.OldTarget is null ? "null" : ev.OldTarget.Nickname;
                string newt = ev.NewTarget is null ? "null" : ev.NewTarget.Nickname;
                FLog.Info($"ChangeSpectating: {ev.Player.DisplayNickname} ({ev.Player.UserId}): {oldt} -> {newt}");
            }
            ReferenceHub targetHub = null;
            if (ev.NewTarget is null) {
                OverlayWatchers.RemoveWatcher(hub); // asi nemusí být, pořeší to RemoveTarget při ChangeRole
            } else {
                targetHub = ev.NewTarget.ReferenceHub;
                OverlayWatchers.ChangeTarget(hub, targetHub);
            }
            if (hub.GetRoleId() is RoleTypeId.Overwatch && overwatchOverlays.ContainsKey(hub.netId))
                overwatchOverlays[hub.netId].TargetHub = targetHub;
        }

        [Event]
        public static void PlayerLeft(PlayerLeftEvent ev) {
            if (OverlayManager.debugInfo)
                FLog.Info($"Leave Player: {ev.Player.DisplayNickname} ({ev.Player.UserId})");
            OverlayWatchers.RemoveWatcher(ev.Player.ReferenceHub);
            UnregisterPlayer(ev.Player);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsRoleSCP(RoleTypeId role) {
            /*return role is PlayerRoles.RoleTypeId.Scp173 ||
                role is PlayerRoles.RoleTypeId.Scp106 ||
                role is PlayerRoles.RoleTypeId.Scp049 ||
                role is PlayerRoles.RoleTypeId.Scp079 ||
                role is PlayerRoles.RoleTypeId.Scp096 ||
                role is PlayerRoles.RoleTypeId.Scp0492 ||
                role is PlayerRoles.RoleTypeId.Scp939 ||
                role is PlayerRoles.RoleTypeId.Scp3114;*/
            return role.GetTeam() is Team.SCPs;
        }


        public static void OnWaiting() {
            OverlayManager.ResetManager();
            OverchargeDetect.overchargeHappen = false;
        }

        public static void UnregisterPlayer(Player player) {
            var hub = player.ReferenceHub;
            hub.UnregisterAllOverlays();
            if (overwatchOverlays.ContainsKey(hub.netId)) {
                overwatchOverlays.Remove(hub.netId);
            }
            if (effectsOverlays.ContainsKey(hub.netId)) {
                effectsOverlays.Remove(hub.netId);
            }
        }
    }
}

/*
[RoundStateChanged(RoundState.WaitingForPlayers)]
private static void PrintInfo() {
    FLog.Info("Printing #&######################################");

    List<GameObject> list = new List<GameObject>(NetworkClient.prefabs.Values);
    list.Sort(delegate (GameObject a, GameObject b) {
        return a.name.CompareTo(b.name);
    });

    foreach (var o in list) {
        FLog.Info(o.name + " " + o.gameObject.name);
    }
}
*/

//ev.Player.ReferenceHub.characterClassManager.TargetChangeCmdBinding(ev.Player.ReferenceHub.networkIdentity.connectionToClient, UnityEngine.KeyCode.J, "/noclip");

