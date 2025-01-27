using System.Collections.Generic;
using SmartOverlays;
using RemoteAdmin.Interfaces;

using Overlay = SmartOverlays.OverlayManager.Overlay;
using EffectClass = CustomPlayerEffects.StatusEffectBase.EffectClassification;
using Compendium;
using Compendium.Features;

namespace BasicOverlay {
    public class OverlayEffects : Overlay {

        public static int startingLine = 4;
        public static MessageAlign messagesAlign = MessageAlign.FullLeft;
        public static int Limit = 6;


        private Message mainMessage;
        private List<Message> messages;
        private ReferenceHub hub;

        public OverlayEffects(ReferenceHub hub) : base("Effects") {
            this.hub = hub;
            mainMessage = new Message("");
            AddMessage(mainMessage, (float)startingLine + 0.75f, messagesAlign);
            messages = new List<Message>();
        }

        public override void UpdateMessages() {
            int lastMessageIndex = 0;
            foreach (var effect in hub.playerEffectsController.AllEffects) {
                if (effect.Intensity <= 0) continue;

                Message message = getOrAddMessage(lastMessageIndex);

                if (lastMessageIndex >= Limit) {
                    message.Content = $"<size=45%>A dalších {Limit - lastMessageIndex + 1}!</size>"; //And more!
                    lastMessageIndex++;
                    break;
                }
                string name = effect.name;
                if (effect is ICustomRADisplay effectRA && !string.IsNullOrWhiteSpace(effectRA.DisplayName)) {
                    name = effectRA.DisplayName;
                }

                string intensity = "";
                if (effect.Intensity != 1) {
                    intensity = " [" + effect.Intensity + "]";
                }

                string duration = "ꝏ";
                if (effect.TimeLeft > 0) {
                    duration = formatSecondsToString((int)effect.TimeLeft);
                }

                string color = "#F8AEF8";
                switch(effect.Classification) {
                    case EffectClass.Positive:
                        color = "#B4FFB4";
                        break;
                    case EffectClass.Negative:
                        color = "#FF7A7A";
                        break;
                }

                message.Content = $"<size=55%><color=" + color + ">" + name + "</color>" + intensity + ": " + duration + "</size>";

                lastMessageIndex++;
            }

            if (lastMessageIndex > 0)
                mainMessage.Content = $"<size=70%>Aktivní efekty:</size>"; //Active Effects:
            else
                mainMessage.Content = $"";

            for (int lastIndex = messages.Count - 1; lastIndex >= lastMessageIndex; lastIndex--) {
                RemoveMessage(messages[lastIndex]);
                messages.RemoveAt(lastIndex);
            }

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

        public static string formatSecondsToString(int totalSeconds) {
            int minutes = (int)totalSeconds / 60;
            int seconds = (int)totalSeconds % 60;
            return $"{minutes}m:{seconds}s";
        }
    }
}
