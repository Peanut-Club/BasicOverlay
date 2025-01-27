using System.Collections.Generic;
using SmartOverlays;
using Compendium;
using PlayerRoles;

using Overlay = SmartOverlays.OverlayManager.Overlay;
using Compendium.Features;

namespace BasicOverlay {
    public class OverlayWatchers : Overlay {
        public static Dictionary<uint, uint> Targets = new Dictionary<uint, uint>();
        public static Dictionary<uint, HashSet<ReferenceHub>> Watchers = new Dictionary<uint, HashSet<ReferenceHub>>();
        private static Dictionary<uint, OverlayWatchers> overlays = new Dictionary<uint, OverlayWatchers>();

        public static int startingLine = -2;
        public static MessageAlign messagesAlign = MessageAlign.FullLeft;
        public static int Limit = 13;

        private Message mainMessage;
        private List<Message> messages;
        private ReferenceHub hub;

        public OverlayWatchers(ReferenceHub hub) : base("Watchers") {
            this.hub = hub;
            mainMessage = new Message(""); //👥
            AddMessage(mainMessage, (float)startingLine + 0.75f, messagesAlign);
            messages = new List<Message>();
        }

        public static void RegisterTarget(ReferenceHub target) {
            uint targetId = target.netId;
            if (overlays.ContainsKey(targetId)) return;
            OverlayWatchers overlay = new OverlayWatchers(target);
            overlays.Add(targetId, overlay);
            Watchers.Add(targetId, new HashSet<ReferenceHub>());
            target.RegisterOverlay(overlay);
        }

        public static void ChangeTarget(ReferenceHub watcher, ReferenceHub newTarget) {
            uint watcherID = watcher.netId;
            uint targetID = newTarget.netId;
            if (Targets.ContainsKey(watcherID)) { // Remove watcher from old target
                Watchers[Targets[watcherID]].Remove(watcher);
            }
            Targets[watcherID] = targetID; // Change target

            //Add watcher to new target
            if (Watchers.ContainsKey(targetID)) {
                Watchers[targetID].Add(watcher);
            } else {
                Watchers.Add(targetID, new HashSet<ReferenceHub> { watcher });
            }
        }

        public static void RemoveWatcher(ReferenceHub watcher) {
            if (!Targets.ContainsKey(watcher.netId)) {
                if (OverlayManager.debugInfo)
                    FLog.Info($"Not in Targets: {watcher.Nick()} ({watcher.UserId()})");
                return;
            }
            Watchers[Targets[watcher.netId]].Remove(watcher);
            Targets.Remove(watcher.netId);
        }

        public static void RemoveTarget(ReferenceHub target) {
            uint targetId = target.netId;
            if (Watchers.ContainsKey(targetId)) {
                foreach (var hub in Watchers[targetId]) {
                    Targets.Remove(hub.netId);
                }
            }
            if (overlays.ContainsKey(targetId)) {
                target.UnregisterOverlay(overlays[targetId]);
            }
            Watchers.Remove(targetId);
            overlays.Remove(targetId);
        }

        public static void UpdateAllMessages() {
            foreach(var overlay in overlays.Values) {
                overlay.UpdateMessages();
            }
        }

        public override void UpdateMessages() {

            //var spectatorHub = ReferenceHub.AllHubs.First(hub => hub.IsPlayer());
            int lastMessageIndex = 0;
            foreach (var spectatorHub in Watchers[hub.netId]) {
                //for (int i = 0; i < count; i++) {
                if (spectatorHub.GetRoleId() is RoleTypeId.Overwatch) continue;

                Message message = getOrAddMessage(lastMessageIndex);

                if (lastMessageIndex >= Limit) {
                    message.Content = $"<size=45%>A {Watchers[hub.netId].Count - lastMessageIndex} dalších!</size>"; // and more
                    lastMessageIndex++;
                    break;
                }
                message.Content = $"<size=55%>{spectatorHub.Nick()}</size>";

                lastMessageIndex++;
            }

            if (lastMessageIndex > 0)
                mainMessage.Content = $"<size=70%>Diváci: {lastMessageIndex}</size>"; //Spectating players
            else
                mainMessage.Content = $"";

            for (int lastIndex = messages.Count - 1; lastIndex >= lastMessageIndex; lastIndex--) {
                RemoveMessage(messages[lastIndex]);
                messages.RemoveAt(lastIndex);
            }
        }

        ~OverlayWatchers() {
            overlays.Remove(hub.netId);
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
