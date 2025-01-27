using Compendium;
using helpers.Extensions;
using PlayerRoles;
using SmartOverlays;
using System.Collections.Generic;
using System.Linq;

using PlayerRoles.PlayableScps.Scp079;
using helpers;
using Compendium.Events;
using CentralAuth;
using PlayerRoles.PlayableScps.Scp049;
using PluginAPI.Events;

namespace BasicOverlay {
    public class OverlaySCP : OverlayManager.Overlay {
        public static int New096TargetInfoVOffset = -4;
        public static float New096TargetInfoTime = 5;

        private List<Message> messages;
        private int zombies;

        //public int generatorCount = 3;
        //public int generatorEngaged = 0;
        //public int generatorEngaging = 0;

        public int scpListLimit = 6; // max 6 SCP kvůli Compendium.Fix
        public int startingLine = 4;
        public MessageAlign messagesAlign = MessageAlign.FullLeft;

        public OverlaySCP() : base("SCP overlay"/*, 5*/) {
            messages = new List<Message>();
            zombies = 0;
            AddMessage(new Message("<size=70%><color=red>SCP subjekty:</color></size>"), (float)startingLine + 0.75f, MessageAlign.FullLeft); //SCP subjects:
        }


        [Event]
        private static void LookAtScp096(Scp096AddingTargetEvent ev) {
            ev.Target.ReferenceHub.AddTempHint("<color=#FF0000>SCP-096</color> <color=#33FFA5>tě chce zabít!</color>", New096TargetInfoTime, New096TargetInfoVOffset);
        }

        public override void UpdateMessages() {
            int zombieCount = 0;
            int lastMessageIndex = 0;
            var hubs = ReferenceHub.AllHubs.Where(h => h.Mode == ClientInstanceMode.ReadyClient && h.roleManager.CurrentRole.Team == Team.SCPs);
            bool is049Alive = false;

            if (!hubs.IsEmpty()) {
                //ReferenceHub hub = hubs.First(); for (int i = 0; i < 7; i++) {
                foreach (var hub in hubs) {
                    RoleTypeId playerRole = hub.GetRoleId();
                    if (playerRole == RoleTypeId.Scp0492) {
                        zombieCount++;
                        continue;
                    }

                    Message message = getOrAddMessage(lastMessageIndex);

                    if (lastMessageIndex >= scpListLimit) {
                        message.Content = $"<size=45%>A dalších {scpListLimit - lastMessageIndex + 1}!</size>"; // Too many SCPs to display!
                        lastMessageIndex++;
                        break;
                    }

                    string msgContent = $"<size=55%><color=red>{playerRole.ToString().SpaceByPascalCase().Replace(' ', '-')}:</color> ";
                    if (playerRole == RoleTypeId.Scp079) {
                        if (hub.roleManager.CurrentRole is Scp079Role scp079) {
                            //Scp079AuxManager auxManager;
                            //scp079.SubroutineModule.TryGetSubroutine<Scp079AuxManager>(out auxManager);
                            // string XP = tierManager.AccessTierLevel != 5 ? $"({tierManager.RelativeExp}/{tierManager.NextLevelThreshold} XP), " : "";
                            // string stats = $"({auxManager.CurrentAuxFloored}/{auxManager.MaxAux} AP), {XP}Tier: {tierManager.AccessTierLevel}";
                            // , Gens: {generatorEngaged}{engaging}/{generatorCount}
                            //string stats = $"({auxManager.CurrentAuxFloored}/{auxManager.MaxAux} AP), {XP}Tier: {tierManager.AccessTierLevel}";
                            //string XP = tierManager.AccessTierLevel != 5 ? $"({tierManager.RelativeExp}/{tierManager.NextLevelThreshold} XP), " : "";
                            //string engaging = generatorEngaging != 0 ? "+{generatorEngaging}" : "";
                            if (scp079.SubroutineModule.TryGetSubroutine(out Scp079TierManager tierManager))
                                msgContent += $"Level: {tierManager.AccessTierLevel}, "; //tier
                            msgContent += $"Zóna: {scp079.CurrentCamera.Room.Zone.ToString().SpaceByPascalCase()}"; //Zone
                        } else
                            msgContent += "REDACTED data";
                    } else if (playerRole == RoleTypeId.Scp049) {
                        is049Alive = true;
                        msgContent += $"({((int)hub.Health())}/{(int)hub.MaxHealth()} HP), Zombíků: {zombies}"; //zombies
                    } else {
                        msgContent += $"({((int)hub.Health())}/{(int)hub.MaxHealth()} HP)";
                    }
                    msgContent += "</size>";

                    message.Content = msgContent;

                    lastMessageIndex++;
                }
                zombies = zombieCount;
            }

            if (!is049Alive && zombieCount != 0) {
                getOrAddMessage(lastMessageIndex).Content = $"<size=55%>zbývá {zombieCount} zombíků</size>"; // zombies left
                lastMessageIndex++;
            }

            for (int lastIndex = messages.Count - 1; lastIndex >= lastMessageIndex; lastIndex--) {
                RemoveMessage(messages[lastIndex]);
                messages.RemoveAt(lastIndex);
            }
            //FLog.Info(messages.Count);
        }

        private Message getOrAddMessage(int index) {
            if (index + 1 > messages.Count) {
                Message message = new Message();
                messages.Add(message);
                AddMessage(message, (float)startingLine - (0.7f * (float)index), messagesAlign);
                return message;
            }
            return messages[index];
        }
    }
}
