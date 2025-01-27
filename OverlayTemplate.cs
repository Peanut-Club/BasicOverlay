using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartOverlays;

namespace BasicOverlay {
    public class OverlayTemplate : OverlayManager.Overlay {

        public OverlayTemplate() : base("Template"/*, 100*/) {
            AddMessage(new Message("Top Center"), 15);
            AddMessage(new Message("Top Full Left"), 15, MessageAlign.FullLeft);
            AddMessage(new Message("Top Left"), 15, MessageAlign.Left);
            AddMessage(new Message("Top Right"), 15, MessageAlign.Right);

            AddMessage(new Message("Center Center"), 0);
            AddMessage(new Message("Center Full Left"), 0, MessageAlign.FullLeft);
            AddMessage(new Message("Center Left"), 0, MessageAlign.Left);
            AddMessage(new Message("Center Right"), 0, MessageAlign.Right);

            AddMessage(new Message("Bottom Center"), -15);
            AddMessage(new Message("Bottom Full Left"), -15, MessageAlign.FullLeft);
            AddMessage(new Message("Bottom Left"), -15, MessageAlign.Left);
            AddMessage(new Message("Bottom Right"), -15, MessageAlign.Right);

            for (int i = 14; i >= -14; i--) {
                if (i == 0) continue;
                AddMessage(new Message(i.ToString()), i, MessageAlign.Center);
            }
        }
    }
}
