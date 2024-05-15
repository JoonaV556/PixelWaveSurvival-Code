using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeAttack : MonoBehaviour {

    [SerializeField]
    private GameObject WeaponObject;
    [SerializeField]
    private Animator WeaponAnimator;
    [SerializeField]
    private GameObject AnimatedWeapon;
    [SerializeField]
    private SpriteRenderer AttackSprite;
    [SerializeField]
    private BoxCollider2D SwordCollider;
    [SerializeField]
    private GameObject SwordSprite;

    private SwordAnimationController swController;
    private Transform weaponTransform;
    private Vector3 mousePosition;
    private bool IsAttacking = false;

    private void Start() {
        AttackSprite.enabled = false;
        SwordCollider.enabled = false;
    }

    private void TryToAttack() {
        // Prevent spamming attack when one is already happening
        if (IsAttacking) return;

        // Activate weapon & get reference to transform
        AttackSprite.enabled = true;
        SwordCollider.enabled = true;
        weaponTransform = WeaponObject.transform;
        swController = AnimatedWeapon.GetComponent<SwordAnimationController>();
        // Get lookat vector to mouse position
        Vector3 LookVector = new Vector3(mousePosition.x - weaponTransform.position.x, mousePosition.y - weaponTransform.position.y, 0);
        // Rotate weapon towards mouse position
        weaponTransform.up = LookVector;
        // Reset sword rotation
        AnimatedWeapon.transform.Rotate(0, 0, 0);
        // Disable cosmetic weapon sprite during attack
        SwordSprite.SetActive(false);
        // Trigger swing animation
        WeaponAnimator.SetTrigger("SwingTrigger");
        // Toggle bool to prevent spamming attack
        IsAttacking = true;
    }

    // Triggered when swing animation ends - Attack is driven by sword swing animation
    public void OnAttackEnded() {
        // Reset rotation of weapon
        AnimatedWeapon.transform.Rotate(0, 0, 0);
        // Disable weapon object
        AttackSprite.enabled = false;
        SwordCollider.enabled = false;
        // Enable cosmetic weapon sprite during attack
        SwordSprite.SetActive(true);
        // Toggle bool to prevent spamming attack
        IsAttacking = false;
    }

    #region Input

    // Get mouse world position for pointing the weapon at the cursor
    public void OnLook(InputValue value) {
        Vector2 MouseScreenPosition = value.Get<Vector2>();
        // Get world position of mouse
        mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(MouseScreenPosition.x, MouseScreenPosition.y, 10f));
    }

    // Triggered when attack input is received
    public void OnFire(InputValue value) {
        TryToAttack();
    }

    #endregion 
}
