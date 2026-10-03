using UnityEngine;

[CreateAssetMenu(menuName = "Items/Cardboard Box")]
public class CardboardBox : PassiveItem, Jacob.Utilities.IIcon
{
    public override void OnEquip(Controller controller)
    {
        GameObject go = Instantiate(prefab);
        go.transform.parent = controller.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        // The box model doubles as a world pickup; strip that so the equipped copy doesn't re-pick itself up
        foreach (PickableItem pickup in go.GetComponentsInChildren<PickableItem>())
            Destroy(pickup);

        controller.storedObject = go;
        controller.animator.gameObject.SetActive(false);
        controller.controllerState = Controller.ControllerState.cardboardBox;
        controller.boxAnimator = go.GetComponentInChildren<Animator>();
    }

    public override void OnUnEquip(Controller controller)
    {
        if (controller.storedObject != null)
            Destroy(controller.storedObject);

        controller.animator.gameObject.SetActive(true);
        controller.controllerState = Controller.ControllerState.normal;
    }
}
