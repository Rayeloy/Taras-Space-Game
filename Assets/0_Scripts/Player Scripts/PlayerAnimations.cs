using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public enum PlayerAnimationState
{
    None,
    Idle,
    Walking,
    Jumping,
    Falling,
    Landing
}

public class PlayerAnimations : MonoBehaviour
{
    private PlayerMovementCMF myPlayerMov;

    [Header("--- Spine Animations ---")]
    public SkeletonAnimation skeletonAnim;
    public AnimationReferenceAsset playerAnimIdle, playerAnimWalking, playerAnimJumping, playerAnimLanding, playerAnimWalkingBackwards, playerAnimFalling;
    public AnimationReferenceAsset playerAimingPose, playerNotAimingPose;
    public PlayerAnimationState animationSt = PlayerAnimationState.None;
    public float idleTimeScale = 1, walkingTimeScale = 1, walkingBackTimeScale = 1, jumpingTimeScale = 1, landingTimeScale = 1, fallingTimeScale = 1;
    public float timeToFloorToStartLandingAnim = 3;

    Spine.Bone weaponAimIKBone;


    public void KonoAwake()
    {
        myPlayerMov = GetComponent<PlayerMovementCMF>();
    }

    public void KonoStart()
    {
        weaponAimIKBone = skeletonAnim.skeleton.FindBone("AIM");
        skeletonAnim.UpdateLocal += SkeletonAnimation_UpdateLocal;
    }

    public void KonoUpdate()
    {
        ManageAnimations();

        ProcessAnimationStates();
    }

    void SkeletonAnimation_UpdateLocal(ISkeletonAnimation animated)
    {
        Vector3 localPositon = skeletonAnim.transform.InverseTransformPoint(myPlayerMov.myPlayerWeapon.aimTarget.position);
        weaponAimIKBone.SetLocalPosition(localPositon);
    }

    void ManageAnimations()
    {
        if (myPlayerMov.collCheck.below)
        {
            if (!myPlayerMov.collCheck.sliping)
            {
                //if (!myPlayerMov.collCheck.lastBelow)
                //{
                //    SetAnimationState(PlayerAnimationState.Landing);
                //}
                //else
                //{
                if (myPlayerMov.currentVel.x == 0 && (animationSt != PlayerAnimationState.Landing || (animationSt == PlayerAnimationState.Landing &&
                    skeletonAnim.state.GetCurrent(0).IsComplete))) SetAnimationState(PlayerAnimationState.Idle);
                else if (myPlayerMov.currentSpeed != 0) SetAnimationState(PlayerAnimationState.Walking);
            }
            else //Sliding
            {

            }
        }
        else if (myPlayerMov.vertMovSt == VerticalMovementState.Jumping) SetAnimationState(PlayerAnimationState.Jumping);
        else {
            if(myPlayerMov.currentVel.y < 0)
            {
                float timeToLand = myPlayerMov.collCheck.distanceToFloor / -myPlayerMov.currentVel.y;
            if (timeToLand <= timeToFloorToStartLandingAnim) SetAnimationState(PlayerAnimationState.Landing);
                else SetAnimationState(PlayerAnimationState.Falling);
            }
        }
    }

    public void SetAnimation(AnimationReferenceAsset animation, bool loop, float timeScale)
    {
        //Debug.LogWarning("ANIMATION CHANGED TO " + animation.Animation);
        skeletonAnim.state.SetAnimation(0, animation, loop).TimeScale = timeScale;
        //skeletonAnim.state.SetAnimation(1, animation, loop).TimeScale = timeScale;
    }

    public void SetAnimationState(PlayerAnimationState state)
    {
        if (state == animationSt) return;
        animationSt = state;
        switch (animationSt)
        {
            case PlayerAnimationState.Idle:
                SetAnimation(playerAnimIdle, true, idleTimeScale);
                break;
            case PlayerAnimationState.Walking:
                float movementSpeedPercent = myPlayerMov.currentSpeed/myPlayerMov.maxMoveSpeed;
                float walkingAnimTimeScale = walkingTimeScale * movementSpeedPercent;
                //Debug.Log("WALKING ANIMATION: currentSpeed = " + myPlayerMov.currentSpeed + "; maxMoveSpeed = " + myPlayerMov.maxMoveSpeed + "; movementSpeedPercent = " + movementSpeedPercent+
                //    "; walkingAnimTimeScale = " + walkingAnimTimeScale);
                //Debug.Log(";player vel.x = " + myPlayerMov.currentVel.x + "; char rotation = " + myPlayerMov.rotateObj.localRotation.eulerAngles.y);
                if ((myPlayerMov.currentVel.x<0 && myPlayerMov.rotateObj.localRotation.eulerAngles.y==0) || (myPlayerMov.currentVel.x >0 && myPlayerMov.rotateObj.localRotation.eulerAngles.y == 180))
                {
                    walkingAnimTimeScale = walkingBackTimeScale * movementSpeedPercent;
                    SetAnimation(playerAnimWalkingBackwards, true, walkingAnimTimeScale);
                }
                else
                {
                    SetAnimation(playerAnimWalking, true, walkingAnimTimeScale);
                }
                break;
            case PlayerAnimationState.Jumping:
                SetAnimation(playerAnimJumping, false, jumpingTimeScale);
                break;
            case PlayerAnimationState.Landing:
                SetAnimation(playerAnimLanding, false, landingTimeScale);
                break;
            case PlayerAnimationState.Falling:
                SetAnimation(playerAnimJumping, false, 0);
                Spine.TrackEntry currentAnimation = skeletonAnim.state.GetCurrent(0);
                currentAnimation.TrackTime = skeletonAnim.Skeleton.Data.FindAnimation("walk").Duration;
                //SetAnimation(playerAnimFalling, false, fallingTimeScale); 
                break;
        }
    }

    void ProcessAnimationStates()
    {
        switch (animationSt)
        {
            case PlayerAnimationState.Walking:
                float movementSpeedPercent = myPlayerMov.currentSpeed / myPlayerMov.currentMaxMoveSpeed;
                float walkingAnimTimeScale = 0;
                //Debug.Log("myPlayerMov.currentSpeed = " + myPlayerMov.currentSpeed + "; myPlayerMov.currentMaxMoveSpeed = " + myPlayerMov.currentMaxMoveSpeed);
                if (skeletonAnim.state.GetCurrent(0).Animation == playerAnimWalking.Animation)
                {
                    if ((myPlayerMov.currentVel.x < 0 && myPlayerMov.rotateObj.localRotation.eulerAngles.y == 0) || (myPlayerMov.currentVel.x > 0 && myPlayerMov.rotateObj.localRotation.eulerAngles.y == 180))
                    {
                        walkingAnimTimeScale = walkingBackTimeScale * movementSpeedPercent;
                        SetAnimation(playerAnimWalkingBackwards, true, walkingAnimTimeScale);
                    }
                    else
                    {
                        walkingAnimTimeScale = walkingTimeScale * movementSpeedPercent;
                    }
                }
                else
                {
                    if ((myPlayerMov.currentVel.x < 0 && myPlayerMov.rotateObj.localRotation.eulerAngles.y == 180) || (myPlayerMov.currentVel.x > 0 && myPlayerMov.rotateObj.localRotation.eulerAngles.y == 0))
                    {
                        walkingAnimTimeScale = walkingTimeScale * movementSpeedPercent;
                        SetAnimation(playerAnimWalking, true, walkingAnimTimeScale);
                    }
                    else
                    {
                        walkingAnimTimeScale = walkingBackTimeScale * movementSpeedPercent;
                    }
                }
                skeletonAnim.state.GetCurrent(0).TimeScale = walkingAnimTimeScale;
                break;
        }
    }

    public void SetAimingPose()
    {
        skeletonAnim.state.SetAnimation(1, playerAimingPose, false).TimeScale = 1;
    }
    public void SetNotAimingPose()
    {
        skeletonAnim.state.SetAnimation(1, playerNotAimingPose, false).TimeScale = 1;
    }
    public void SetNoWeaponPose()
    {
        skeletonAnim.state.SetEmptyAnimation(1, 0.2f);
    }

    public void ActivateAttatchment(string slotName, string attatchmentName)
    {
        skeletonAnim.Skeleton.SetAttachment(slotName, attatchmentName);
    }
}
