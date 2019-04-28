using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public class NotificationNode : Node<NotificationNode> {

        public int UndefinedNotificationCount;
        public int InventoryNotificationCount;
        public int BlueprintsNotificationCount;
        public int BuilderNotificationCount;
        public int CraftTreeNotificationCount;
        public int LogNotificationCount;
        public int GalleryNotificationCount;
        public int EncyclopediaNotificationCount;

        public bool PDANotification;
        public bool CraftingNotification;

        internal NotificationNode(string json) : base(json) {
            UndefinedNotificationCount = GetInt("undefined_notification_count");
            InventoryNotificationCount = GetInt("inventory_notification_count");
            BlueprintsNotificationCount = GetInt("blueprints_notification_count");
            BuilderNotificationCount = GetInt("builder_notification_count");
            CraftTreeNotificationCount = GetInt("craft_tree_notification_count");
            LogNotificationCount = GetInt("log_notification_count");
            GalleryNotificationCount = GetInt("gallery_notification_count");
            EncyclopediaNotificationCount = GetInt("encyclopedia_notification_count");

            if (InventoryNotificationCount >= 1
                || BlueprintsNotificationCount >= 1
                || BuilderNotificationCount >= 1
                || LogNotificationCount >= 1
                || GalleryNotificationCount >= 1
                || EncyclopediaNotificationCount >= 1)
            {
                PDANotification = true;
            }
            else
            {
                PDANotification = false;
            }

            if (CraftTreeNotificationCount >= 1)
                CraftingNotification = true;
        }
    }
}
