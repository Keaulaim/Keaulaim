using HarmonyLib;
using Database;

namespace KFastPodNotification
{
    public class Patches
    {

        [HarmonyPatch(typeof(BuildingStatusItems))]
        [HarmonyPatch("CreateStatusItems")]
        public class BuildingStatusItems_CreateStatusItems_Patch
        {


            public static void Postfix(BuildingStatusItems __instance)
            {

                __instance.NewDuplicantsAvailable.notificationClickCallback = (Notification.ClickCallback)(data =>
                {
                    int idx1 = ClusterManager.Instance.activeWorld.id;

                    for (int idx2 = 0; idx2 < Components.Telepads.Items.Count; ++idx2)
                    {
                        if (Components.Telepads[idx2].GetComponent<Telepad>().GetMyWorldId() == idx1)
                        {
                            // SelectTool.Instance.Select(Components.Telepads[idx2].GetComponent<KSelectable>());

                            ImmigrantScreen.InitializeImmigrantScreen(Components.Telepads[idx2].GetComponent<Telepad>());
                            Game.Instance.Trigger(288942073, (object)null);

                            break;
                        }
                    }


                });


            }

        }
    }
}
