using RoR2;
using UnityEngine;

namespace TooManyItems.Handlers
{
    class EquipmentTargetHandler : MonoBehaviour
    {
        public GameObject obj;
        public Indicator indicator;
        public BullseyeSearch search;

        public void Awake()
        {
            indicator = new Indicator(gameObject, null);
        }

        public void Invalidate()
        {
            obj = null;
            indicator.targetTransform = null;
        }

        public void ConfigureTargetFinder(EquipmentSlot slot)
        {
            search ??= new BullseyeSearch();

            search.teamMaskFilter = TeamMask.allButNeutral;
            search.teamMaskFilter.RemoveTeam(slot.characterBody.teamComponent.teamIndex);
            search.sortMode = BullseyeSearch.SortMode.Angle;
            search.filterByLoS = true;

            Ray ray = CameraRigController.ModifyAimRayIfApplicable(slot.GetAimRay(), slot.gameObject, out _);
            search.searchOrigin = ray.origin;
            search.searchDirection = ray.direction;
            search.maxDistanceFilter = 240f;
            search.maxAngleFilter = 10f;
            search.viewer = slot.characterBody;
        }

        public void ConfigureTargetFinderForEnemies(EquipmentSlot slot)
        {
            ConfigureTargetFinder(slot);
            search.teamMaskFilter = TeamMask.GetUnprotectedTeams(slot.characterBody.teamComponent.teamIndex);
            search.RefreshCandidates();
            search.FilterOutGameObject(slot.gameObject);
        }

        public void ConfigureTargetFinderForFriendlies(EquipmentSlot slot)
        {
            ConfigureTargetFinder(slot);
            search.teamMaskFilter = TeamMask.none;
            search.teamMaskFilter.AddTeam(slot.characterBody.teamComponent.teamIndex);
            search.RefreshCandidates();
            search.FilterOutGameObject(slot.gameObject);
        }
    }
}
