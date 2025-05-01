using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Player_Touchscreen_Logic: MonoBehaviour {
    public InputActionAsset inputActions;

    void OnEnable() {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var jumpAction = inputActions.FindAction("Jump");
        var dashAction = inputActions.FindAction("Dash");
        var MoveHorizontal = inputActions.FindAction("Horizontal");

        root.Q<Button>("Move_Jump").clicked += () => jumpAction?.PerformInteractiveRebinding();
        root.Q<Button>("Move_Dash").clicked += () => dashAction?.PerformInteractiveRebinding();
        root.Q<Button>("Move_Left").clicked -= () => MoveHorizontal?.PerformInteractiveRebinding();
        root.Q<Button>("Move_Rigth").clicked += () => MoveHorizontal?.PerformInteractiveRebinding();
    }
}