using Compendium.Features;
using SmartOverlays;
using Compendium;

namespace BasicOverlay {
    public class BasicOverlayFeature : ConfigFeatureBase {

        public override string Name => "BasicOverlay";

        public override bool IsPatch => true;

        public override void Load() {
            base.Load();
            BasicOverlayLogic.Load();
            FLog.Info("Basic Overlay Loaded");
        }

        public override void Unload() {
            base.Unload();
            BasicOverlayLogic.Unload();
        }

        public override void Reload() {
            base.Reload();
            BasicOverlayLogic.Reload();
        }

        public override void OnWaiting() {
            base.OnWaiting();
            BasicOverlayLogic.OnWaiting();
        }

        public override void Restart() {
            base.Restart();
            OverlayManager.ResetManager();
        }
    }
}
