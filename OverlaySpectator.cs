using Compendium.Features;
using helpers.Patching;
using MapGeneration.Distributors;
using Mirror;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using Respawning;
using SmartOverlays;

using PlayerRoles.PlayableScps.Scp049;
using Compendium.Events;
using Compendium;
using PluginAPI.Events;

using CentralAuth;
using PluginAPI.Core;
using Respawning.Waves;
using FacilitySoundtrack;

namespace BasicOverlay {
    public class OverlaySpectator : OverlayManager.Overlay {
        public static int RessurectInfoVOffset = -4;
        private static Scp049ResurrectAbility _resurrectAbility = null;

        private Message _respawnTimeMessage;
        private Message _ntfTicketsMessage;
        private Message _chaosTicketsMessage;

        private Message _warheadStatusMessage;
        private Message _generatorsStatusMessage;

        private Message _deadCountMessage;
        private Message _peopleCountMessage;
        private Message _scpCountMessage;

        public OverlaySpectator() : base("Spectator Overlay"/*, 15*/) {
            _respawnTimeMessage = new Message("");
            AddMessage(_respawnTimeMessage, 14.5f, MessageAlign.FullLeft);

            _ntfTicketsMessage = new Message("");
            AddMessage(_ntfTicketsMessage, 13.5f, MessageAlign.FullLeft);

            _chaosTicketsMessage = new Message("");
            AddMessage(_chaosTicketsMessage, 12.5f, MessageAlign.FullLeft);


            _warheadStatusMessage = new Message("");
            AddMessage(_warheadStatusMessage, -14, MessageAlign.Right);

            _generatorsStatusMessage = new Message("");
            AddMessage(_generatorsStatusMessage, -15, MessageAlign.Right);



            _deadCountMessage = new Message("");
            AddMessage(_deadCountMessage, -11, MessageAlign.Right);

            _peopleCountMessage = new Message("");
            AddMessage(_peopleCountMessage, -12, MessageAlign.Right);

            _scpCountMessage = new Message("");
            AddMessage(_scpCountMessage, -13, MessageAlign.Right);
        }

        [Event]
        private static void AlertRevive(Scp049StartResurrectingBodyEvent ev) {
            Scp049Role doctor;
            if (!(_resurrectAbility is null) || !ev.CanResurrct || (doctor = ev.Player.RoleBase as Scp049Role) is null || !doctor.SubroutineModule.TryGetSubroutine<Scp049ResurrectAbility>(out _resurrectAbility))
                return;

            var tempHint = ev.Target.ReferenceHub.AddTempHint("<color=#FF0000>SCP-049</color> <color=#33FFA5>tě oživuje!</color>", _resurrectAbility.Duration, RessurectInfoVOffset);
            Calls.OnFalse(
                delegate {
                    tempHint.SetExpired();
                    _resurrectAbility = null;
                }, () => _resurrectAbility.IsInProgress
            );
        } 

        public override void UpdateMessages() {
            /*
            TimeBasedWave nextWave = null;
            foreach (TimeBasedWave wave in WaveManager.Waves) {
                if (wave == null) continue;
                if (nextWave == null || nextWave.Timer.TimeLeft > wave.Timer.TimeLeft)
                    nextWave = wave;
            }

            int minutes = (int)nextWave.Timer.TimeLeft / 60;
            int seconds = (int)nextWave.Timer.TimeLeft % 60;
            var knownFaction = nextWave.TargetFaction;

            string respawning = $" <size=60%>Oživování! limit: {nextWave.MaxWaveSize} hráčů</size>";

            _respawnTimeMessage.Content = string.Format("<size=75%><color=#BCBCBC>Čas do oživení:</color> {0:00}:{1:00}</size>", minutes, seconds); //Respawn time
            _ntfTicketsMessage.Content = string.Format("<size=75%><color=#3286D2>NTF Tikety:</color> {0:0.00}%{1}</size>", Respawn.NtfTickets * 100, //NTF Tickets
                knownFaction == Faction.FoundationStaff ? respawning : ""); //Spawning
            _chaosTicketsMessage.Content = string.Format("<size=75%><color=#2AAE39>Chaos Tikety:</color> {0:0.00}%{1}</size>", Respawn.ChaosTickets * 100, //Chaos Tickets
                knownFaction == Faction.FoundationEnemy ? respawning : ""); //Spawning
            */
            int activating = 0,
                engaged = 0;
            foreach (Scp079Generator scp079Generator in Scp079Recontainer.AllGenerators) {
                if (scp079Generator.Activating) {
                    activating++;
                } else if (scp079Generator.Engaged) {
                    engaged++;
                }
            }

            string genStatus;
            if (OverchargeDetect.overchargeHappen) {
                genStatus = $"Přetížené"; //Overcharged
            } else {
                string activatingText = activating != 0 ? $", Zapínání: {activating}" : ""; //Activating
                genStatus = $"{engaged}/3 zapnuty{activatingText}"; //engaged
            }
            _generatorsStatusMessage.Content = $"<size=70%>Generátory: {genStatus}</size>"; //Generators

            string warheadMessage;

            if (AlphaWarheadController.Detonated) {
                warheadMessage = "<color=yellow>Detonováno</color>"; //Detonated
            } else if (AlphaWarheadController.InProgress) {
                warheadMessage = $"<color=#FF9500>Detonace za {(int)AlphaWarheadController.TimeUntilDetonation} s</color>"; //Detonating in 
            } else if (!AlphaWarheadOutsitePanel.nukeside.enabled) {
                warheadMessage = "<color=red>Vypnuto</color>"; //Disabled
            } else if (AlphaWarheadController.Singleton.CooldownEndTime > NetworkTime.time) {
                warheadMessage = "<color=blue>Restartování</color>"; //Restarting
            } else {
                warheadMessage = "<color=green>Zapnuto</color>"; //Enabled
            }

            _warheadStatusMessage.Content = $"<size=70%>Warhead: <b>{warheadMessage}</b></size>";


            int deadCount = 0;
            int chaosCount = 0;
            int scpCount = 0;
            int ntfCount = 0;
            int flamingos = 0;
            foreach (var hub in ReferenceHub.AllHubs) {
                if (hub.Mode != ClientInstanceMode.ReadyClient) continue;

                Team team = hub.roleManager.CurrentRole.Team;
                if (team is Team.Dead) {
                    deadCount++;
                } else if (team is Team.ChaosInsurgency || team is Team.ClassD) {
                    chaosCount++;
                } else if (team is Team.SCPs) {
                    scpCount++;
                } else if (team is Team.FoundationForces || team is Team.Scientists) {
                    ntfCount++;
                }
#if FLAMINGOS
                else if (team is Team.Flamingos) flamingos++;
#endif
            }
            int scpTargets = ntfCount + chaosCount + flamingos + RoundSummary.singleton.ExtraTargets;
            scpTargets += flamingos;
            if (scpCount == 0) scpTargets = 0;

            _deadCountMessage.Content = $"<size=55%><color=#858585><b>Diváků</b></color>:</size><size=70%> {deadCount}</size>"; //Spectators
            _peopleCountMessage.Content = $"<size=70%><color=#3286D2><b>NTF</b></color>: {ntfCount}, <color=#2AAE39><b>Chaos</b></color>: {chaosCount}</size>";
            _scpCountMessage.Content = $"<size=70%><color=red><b>SCP</b></color>: {scpCount}, </size><size=50%><b>SCP Cíle</b>:</size><size=70%> {scpTargets}</size>"; // Targets
            //<pos=86%><pos=85%><pos=79%>
        }
    }

    public static class OverchargeDetect {
        public static bool overchargeHappen = false;


        [Patch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.BeginOvercharge), PatchType.Postfix, "Overcharge Detect")]
        public static void BeginOvercharge() {
            FLog.Info("Overcharged");
            overchargeHappen = true;
        }
    }
}
