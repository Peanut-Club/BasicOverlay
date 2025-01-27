using CentralAuth;
using Compendium;
using Compendium.Updating;
using MapGeneration;
using PlayerRoles;
using PluginAPI.Core;
using System.Collections.Generic;
using System.Linq;
using System;


namespace BasicOverlay {
    public class RoundEndUtils {
        public static int AnnouncementDuration = 7;

        private static uint lastNetId = uint.MaxValue;
        private static FacilityZone lastZone = FacilityZone.None;
        private static DateTime lastAnnouceTime = DateTime.MinValue;

        [Update(Delay = 2000, IsUnity = true, PauseRestarting = true, PauseWaiting = true)]
        private static void checkForLast() {
            if (Round.IsLocked) return;

            var hub = GetLastHub();
            if (hub != null) {
                var zone = hub.Zone();
                var now = DateTime.Now;
                if (hub.netId == lastNetId && (zone == lastZone || zone == FacilityZone.None || hub.Room() is null) && lastAnnouceTime.AddMinutes(1) > now) return;
                announceLast(hub);
                lastNetId = hub.netId;
                lastZone = zone;
                lastAnnouceTime = now;
                return;
            }
            lastNetId = uint.MaxValue;
        }

        private static void announceLast(ReferenceHub hub) {
            if (hub == null) return;
            string nick = hub.Nick();
            nick = nick.Length > 22 ? nick.Substring(0,20) + "..." : nick;
            var room = hub.Room();
            string zone_name = "Neznámé";
            if (room != null)
                zone_name = room.Name == RoomName.Pocket ? "v Kapesní dimenzi" : GetCzechZone(room.Zone);
            string content = $"[Poslední] <color={hub.GetRoleColorHexPrefixed()}>{nick}</color> {zone_name}";
            World.Broadcast(content, AnnouncementDuration);
            Plugin.Info($"[LAST] {nick} at {zone_name}");
        }

        public static string GetCzechZone(FacilityZone zone) {
            switch (zone) {
                case FacilityZone.LightContainment: return "v Light Containment Zóně";
                case FacilityZone.HeavyContainment: return "v Heavy Containment Zóně";
                case FacilityZone.Entrance: return "v Entrance Zóně";
                case FacilityZone.Surface: return "na Povrchu";
                default: return "v |REDACTED|";
            }
        }

        public static ReferenceHub GetLastHub(bool humanPriority = true) {
            if (!RoundSummary.RoundInProgress()) return null;

            LastAliveStats stats = getStats();
            if (!stats.IsBlockingPlayer)
                return null;

            if (stats.LastFacilityForces)
                return GetHubByFaction(Faction.FoundationStaff);

            if (stats.LastChaosForces)
                return GetHubByFaction(Faction.FoundationEnemy);
#if FLAMINGOS
            if (stats.LastFlamingos)
                return GetHubByFaction(Faction.Flamingos);
#endif

            if (stats.LastAnomalies)
                return GetHubByFaction(Faction.SCP);

            if (stats.LastExtraTargets)
                return GetHubByFaction(Faction.Unclassified);

            return null;
        }

        public static ReferenceHub GetHubByFaction(Faction faction) {
            foreach (ReferenceHub hub in ReferenceHub.AllHubs) {
                if (hub.IsPlayer() && hub.GetFaction() == faction) return hub;
            }
            return null;
        }

        public static ReferenceHub GetHubByTeam(Team team) {
            foreach (ReferenceHub hub in ReferenceHub.AllHubs) {
                if (hub.IsPlayer() && hub.GetTeam() == team) return hub;
            }
            return null;
        }

        public static List<ReferenceHub> GetAllHubsByTeam(Team team) {
            return ReferenceHub.AllHubs.Where(hub => hub.IsPlayer() && hub.GetTeam() == team).ToList();
        }

        private static LastAliveStats getStats() {
            LastAliveStats newList = default(LastAliveStats);
            newList.extraTargets = RoundSummary.singleton?.ExtraTargets ?? 0;
            foreach (ReferenceHub hub in ReferenceHub.AllHubs) {
                if (hub.Mode != ClientInstanceMode.ReadyClient) continue;
                switch (hub.GetTeam()) {
                    case Team.ClassD:
                    case Team.ChaosInsurgency:
                        newList.chaosInsurgency++;
                        break;
                    case Team.FoundationForces:
                    case Team.Scientists:
                        newList.facilityForces++;
                        break;
                    case Team.SCPs:
                        newList.anomalies++;
                        break;
#if FLAMINGOS
                    case Team.Flamingos:
                        newList.flamingos++;
                        break;
#endif
                }
            }

            if (RoundSummary._singletonSet)
                //newList.chaosTargets = RoundSummary.singleton.ChaosTargetCount;
                newList.extraTargets = RoundSummary.singleton.ExtraTargets;
            return newList;
        }
    }

    public struct LastAliveStats {
        public int anomalies;
        public int facilityForces;
        public int chaosInsurgency;
        public int extraTargets;
#if FLAMINGOS
        public int flamingos;
#endif
        public bool IsBlockingPlayer {
            get {
                int teams = 0;
                if (anomalies > 0) teams++;
                if (facilityForces > 0) teams++;
                if (chaosInsurgency > 0) teams++;
                if (extraTargets > 0) teams++;
#if FLAMINGOS
                if (flamingos > 0) teams++;
#endif
                return teams == 2 && AnyLastPlayer;
            }
        }

        /*
        public Faction DeadFaction {
            get {
                if (anomalies <= 0) return Faction.SCP;
                if (facilityForces <= 0) return Faction.FoundationStaff;
                return Faction.FoundationEnemy;
            }
        }*/

        public bool AnyLastPlayer => LastAnomalies || LastFacilityForces || LastChaosForces ||
#if FLAMINGOS
        LastFlamingos ||
#endif
        LastExtraTargets;
        public bool LastAnomalies => anomalies == 1;
        public bool LastFacilityForces => facilityForces == 1;
        public bool LastChaosForces => chaosInsurgency == 1;
        public bool LastExtraTargets => extraTargets == 1;
#if FLAMINGOS
        public bool LastFlamingos => flamingos == 1;
#endif
    }
}

// by Mallifrey
