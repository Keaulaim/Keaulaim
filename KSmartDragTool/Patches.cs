using HarmonyLib;
using STRINGS;
using System;
using UnityEngine;



namespace KSmartDragTool
{
    public class Patches
    {


        public static bool _panLeft;
        public static bool _panRight;
        public static bool _panUp;
        public static bool _panDown;
        public static bool _isMousePanning;


        [HarmonyPatch(typeof(DragTool))]
        [HarmonyPatch("OnMouseMove")]
        public class DragTool_OnMouseMove_Patch
        {


            public static void Postfix(Vector3 ___previousCursorPos, Vector3 ___downPos, Guid ___areaVisualizerText, bool ___dragging)
            {

                
                if (!___dragging)
                    return;

                // --- Rippage de la cam ---

                //Position de la souris
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(KInputManager.GetMousePos());
                mousePos = Vector3.Max((Vector3)ClusterManager.Instance.activeWorld.minimumBounds, mousePos);
                mousePos = Vector3.Min((Vector3)ClusterManager.Instance.activeWorld.maximumBounds, mousePos);
                mousePos = Camera.main.WorldToViewportPoint(mousePos);


                Patches._panRight = false;
                Patches._panLeft = false;
                Patches._panUp = false;
                Patches._panDown = false;


                if (mousePos.x > 0.95f)
                {
                    Patches._panRight = true;
                }
                else if (mousePos.x < 0.05f)
                {
                    Patches._panLeft = true;
                }

                if (mousePos.y > 0.95f)
                {
                    Patches._panUp = true;
                }
                else if (mousePos.y < 0.05f)
                {
                    Patches._panDown = true;
                }

                Patches._isMousePanning = Patches._panRight || Patches._panLeft || Patches._panUp || Patches._panDown ;




                // --- Recadrage du texte ---

                if (___areaVisualizerText != Guid.Empty)
                {

                    // Changement de la police
                    LocText component = NameDisplayScreen.Instance.GetWorldText(___areaVisualizerText).GetComponent<LocText>();


                    // Si le cadre est complêtement visible, aucune action
                    if (CameraController.Instance.IsVisiblePos(___downPos))
                        return;

                    //On récupère le cadre de la cam
                    Vector2 maxCamPoint = (Vector2)Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, Camera.main.transform.GetPosition().z));
                    Vector2 minCamPoint = (Vector2)Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, Camera.main.transform.GetPosition().z));

                    //On récupère le cadre de l'outils
                    Vector2 maxAreaPoint = (Vector2)Vector3.Max(___downPos, ___previousCursorPos);
                    Vector2 minAreaPoint = (Vector2)Vector3.Min(___downPos, ___previousCursorPos);

                    //On calcul l'intersection
                    Vector2 maxVisiblePoint = Vector2.Min(maxAreaPoint, maxCamPoint);
                    Vector2 minVisiblePoint = Vector2.Max(minAreaPoint, minCamPoint);


                    //Clamp to the world
                    maxVisiblePoint.x = Mathf.Clamp(maxVisiblePoint.x, ClusterManager.Instance.activeWorld.minimumBounds.x, ClusterManager.Instance.activeWorld.maximumBounds.x);
                    maxVisiblePoint.y = Mathf.Clamp(maxVisiblePoint.y, ClusterManager.Instance.activeWorld.minimumBounds.y, ClusterManager.Instance.activeWorld.maximumBounds.y);

                    minVisiblePoint.x = Mathf.Clamp(minVisiblePoint.x, ClusterManager.Instance.activeWorld.minimumBounds.x, ClusterManager.Instance.activeWorld.maximumBounds.x);
                    minVisiblePoint.y = Mathf.Clamp(minVisiblePoint.y, ClusterManager.Instance.activeWorld.minimumBounds.y, ClusterManager.Instance.activeWorld.maximumBounds.y);

                    //Regularized to cell
                    Vector3 vector3 = new Vector3(Grid.HalfCellSizeInMeters, Grid.HalfCellSizeInMeters, 0.0f);
                    maxVisiblePoint = (Vector2)(Grid.CellToPosCCC(Grid.PosToCell(maxVisiblePoint), Grid.SceneLayer.Background) + vector3);
                    minVisiblePoint = (Vector2)(Grid.CellToPosCCC(Grid.PosToCell(minVisiblePoint), Grid.SceneLayer.Background) - vector3);

                    //Calcul du milieu
                    Vector2 input = (maxVisiblePoint + minVisiblePoint) * 0.5f;


                    //Recalage du texte
                    
                    component.transform.SetPosition((Vector3)input);



                }


            }
        }

        [HarmonyPatch(typeof(DragTool))]
        [HarmonyPatch("OnLeftClickUp")]
        public class DragTool_OnLeftClickUp_Patch
        {

            public static void Postfix()
            {

                Patches._panRight = false;
                Patches._panLeft = false;
                Patches._panUp = false;
                Patches._panDown = false;
                Patches._isMousePanning = false;

            }

        }

        [HarmonyPatch(typeof(DragTool))]
        [HarmonyPatch("OnRightClickUp")]
        public class DragTool_OnRightClickUp_Patch
        {

            public static void Postfix()
            {

                Patches._panRight = false;
                Patches._panLeft = false;
                Patches._panUp = false;
                Patches._panDown = false;
                Patches._isMousePanning = false;

            }

        }




        [HarmonyPatch(typeof(CameraController))]
        [HarmonyPatch("NormalCamUpdate")]
        public class CameraController_NormalCamUpdate_Patch
        {

            public struct oldPanning
            {

                public bool r;
                public bool l;
                public bool u;
                public bool d;

            };

            public static void Prefix(ref bool ___panLeft, ref bool ___panRight, ref bool ___panUp, ref bool ___panDown, out oldPanning __state)
            {

                    __state.r = ___panRight;
                    __state.l = ___panLeft;
                    __state.u = ___panUp;
                    __state.d = ___panDown;

                if (Patches._isMousePanning)
                {
                               
                    ___panLeft = Patches._panLeft;
                    ___panRight = Patches._panRight;
                    ___panUp = Patches._panUp;
                    ___panDown =  Patches._panDown;

                }

            }


            public static void Postfix(ref bool ___panLeft, ref bool ___panRight, ref bool ___panUp, ref bool ___panDown, oldPanning __state)
            {
                if (Patches._isMousePanning)
                {
                    ___panLeft = __state.l;
                    ___panRight = __state.r;
                    ___panUp = __state.u;
                    ___panDown = __state.d;

                }

            }

        }


    }


}

