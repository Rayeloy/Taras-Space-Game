//Property of Another Coffee Games S.L., Spain. Author: Carlos Eloy Jose Sanz
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CollisionsCheck2D : MonoBehaviour
{
    public PlayerMovementCMF myPlayerMov;
    public EnemyAI myEnemy;
    [HideInInspector]
    public Collider2D myCollider;
    [HideInInspector]
    public Rigidbody2D rb;

    public bool disableAllDebugs = true;
    public bool disableAllRays = true;
    public bool collideWithTriggers = false;
    QueryTriggerInteraction qTI;
    public LayerMask collisionMask;
    public float maxSlopeAngle = 60;//in the future, take the value from "mover" script
    public float FloorMaxDistanceCheck = 5;
    Color purple = new Color(0.749f, 0.380f, 1f);
    Color brown = new Color(0.615f, 0.329f, 0.047f);
    Color orange = new Color(0.945f, 0.501f, 0.117f);
    Color darkBrown = new Color(0.239f, 0.121f, 0f);
    Color darkYellow = new Color(0.815f, 0.780f, 0.043f);
    Color darkRed = new Color(0.533f, 0.031f, 0.027f);
    Color darkGreen = new Color(0.054f, 0.345f, 0.062f);
    RaycastHit2D[] hits;

    const float skinWidth = 0.1f;

    public CapsuleCollider2D coll;
    RaycastOrigins raycastOrigins;


    [Header(" -- Vertical Collisions -- ")]
    //public bool showVerticalRays;
    //public bool showVerticalLimits;
    public bool showDistanceCheckRays;
    //public bool showRoofRays;
    //public int verticalRows;
    //public int verticalRaysPerRow;
    //float verticalRowSpacing;
    //float verticalRaySpacing;
    [HideInInspector]
    public bool above, below, lastBelow, lastLastBelow, safeBelow, tooSteepSlope;
    [HideInInspector]
    public float distanceToFloor;
    [HideInInspector]
    public bool safeBelowStarted;
    float safeBelowTime, safeBelowMaxTime;
    [HideInInspector]
    public float slopeAngle;
    [HideInInspector]
    public bool lastSliping;
    public bool sliping { get { return below && tooSteepSlope; } }
    [HideInInspector]
    public bool onSlide
    {
        get
        {
            return (floor != null && floor.tag == "Slide");
        }
    }
    [HideInInspector]
    public GameObject floor;
    GameObject lastFloor;
    [HideInInspector]
    Vector3 groundContactPoint = Vector3.zero;
    bool justJumped = false;
    [Range(2,10)]
    public int roofCheckRayNumber = 3;
    public float maxRoofAngle = 150;


    public void KonoAwake(Collider2D _collider)
    {
        qTI = collideWithTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
        rb = GetComponent<Rigidbody2D>();
        hits = new RaycastHit2D[10];

        myCollider = _collider;
        floor = lastFloor = null;
        //wallNormal = Vector3.zero;
        above = below = lastBelow = lastLastBelow = safeBelow = false;
        distanceToFloor = float.MaxValue;
        safeBelowStarted = false;
        safeBelowTime = 0;
        safeBelowMaxTime = 0;

        myPlayerMov = GetComponent<PlayerMovementCMF>();
        myEnemy = GetComponent<EnemyAI>();
    }

    public void KonoStart()
    {
        //CalculateRaySpacing();
        //print("bounds.size.z = " + coll.bounds.size.z+"bounds.size.y = "+ coll.bounds.size.y);
    }

    #region FixedUpdate
    //RUN IN FIXED UPDATE
    public void UpdateCollisionVariables(Mover mover, VerticalMovementState jumpSt, bool forceFly = false)
    {
        mover.CheckForGround(platformMovement.magnitude > 0.0001f);
        if (jumpSt != VerticalMovementState.Jumping && jumpSt != VerticalMovementState.JumpBreaking)
            below = mover.IsGrounded();
        if (forceFly) below = false;
        SetSlopeAngle(mover.GetGroundNormal());
        floor = mover.GetGroundCollider() != null ? mover.GetGroundCollider().gameObject : null;

        if (below)
        {
            groundContactPoint = mover.GetGroundPoint();
        }

        if (mover.sensor.castType == Sensor2D.CastType.RaycastArray /*|| mover.sensor.castType == Sensor2D.CastType.RaycastArray2D*/)
        {
            if (!lastBelow && below)
            {
                //Debug.LogError("WE JUST LANDED");
                if(mover.sensor.castType == Sensor2D.CastType.RaycastArray)mover.sensor.RecalibrateRaycastArrayPositions();
                //if (mover.sensor.castType == Sensor2D.CastType.RaycastArray2D) mover.sensor.RecalibrateRaycastArrayPositions2D();
            }
            else if (lastBelow && !below)
            {
                //Debug.LogError("WE JUST TOOK OFF");
                if(mover.sensor.castType == Sensor2D.CastType.RaycastArray)mover.sensor.RecalibrateRaycastArrayPositions(0.1f);
                //if (mover.sensor.castType == Sensor2D.CastType.RaycastArray2D) mover.sensor.RecalibrateRaycastArrayPositions2D();

            }
        }
    }

    /// <summary>
    /// MAIN FUNCTION OF CollisionsCheck
    /// </summary>
    /// <param name="vel"></param>
    public void UpdateCollisionChecks(Vector3 vel)
    {
        //ChangePositionWithPlatform();

        UpdateRaycastOrigins();

        VerticalCollisionsDistanceCheck(ref vel);

        RoofCollisionCheck(ref vel);

        UpdateSafeBelow();

        //SavePlatformPoint();
    }

    public void ResetVariables()
    {
        lastSliping = sliping;

        lastLastBelow = lastBelow;

        lastBelow = justJumped ? true : below;
        below = false;
        justJumped = false;

        lastFloor = floor;
        floor = null;
        //wallNormal = groundContactPoint = Vector3.zero;
        above = below = safeBelow = safeBelowStarted = tooSteepSlope = false;
        distanceToFloor = float.MaxValue;
        safeBelowTime = 0;
        slopeAngle = -500;

        ////Horizontal
        //wall = null;
        //wallAngle = wallSlopeAngle = 0;
        //wallNormal = Vector3.zero;

    }

    public bool StopHorizontalOnSteepSlope(Vector3 floorNormal, Vector3 vel)
    {
        if (!sliping) return false;
        float signedSlopeAngle = Vector2.SignedAngle(floorNormal, Vector3.up);
        //Debug.Log("signedSlopeAngle = " + signedSlopeAngle + "; vel.x = " + vel.x);
        if (vel.x < 0 && signedSlopeAngle > 0) return true;
        if (vel.x > 0 && signedSlopeAngle < 0) return true;
        return false;
    }
    #endregion

    #region Update
    //public void MoveWithPlatform()
    //{
    //    ChangePositionWithPlatform();
    //    SavePlatformPoint();
    //}
    #endregion

    public void SetSlopeAngle(Vector3 floorNormal)
    {
        slopeAngle = Vector3.Angle(floorNormal, Vector3.up);
        tooSteepSlope = slopeAngle > maxSlopeAngle;
    }

    #region --- COLLISIONS (RAYCASTS FUNCTIONS) --- 

    #region --- VERTICAL COLLISIONS ---

    void VerticalCollisionsDistanceCheck(ref Vector3 vel)
    {
        if (vel.y < 0)
        {
            float rayLength = FloorMaxDistanceCheck;
            //Vector3 rowsOrigin = raycastOrigins.BottomLFCornerReal;
            //Vector3 rowOrigin = rowsOrigin;
            //print("----------NEW SET OF RAYS------------");
            //for (int i = 0; i < verticalRows; i++)
            //{
            //    //rowOrigin.z = rowsOrigin.z - (verticalRowSpacing * i);
            //    for (int j = 0; j < verticalRaysPerRow; j++)
            //    {
            //if (i % 2 == 0 && j % 2 == 0)// Every even number throw a ray. This is to reduce raycasts to half since not so many are needed.
            //{
            Vector3 rayOrigin = raycastOrigins.BottomCentre;
            //rayOrigin += Vector3.up * skinWidth;
            Vector3 rayDir = vel.normalized;
            RaycastHit2D hit;
            if (showDistanceCheckRays && !disableAllRays)
            {
                Debug.DrawRay(rayOrigin, rayDir * rayLength, Color.red);
            }


            if (ThrowRaycast2D(rayOrigin, rayDir, out hit, rayLength, collisionMask, qTI))
            {
                if (CanCollide(hit))
                {
                    //print("Vertical Hit");
                    //if (hit.distance < collisions.distanceToFloor)
                    //{
                    distanceToFloor = hit.distance;
                    //}
                }
            }
            //}
            //    }
            //}
            //collisions.distanceToFloor -= skinWidth;
        }
    }

    void RoofCollisionCheck(ref Vector3 vel)
    {
        
        float rayLength = coll.bounds.extents.y + 0.1f;
        Vector3 rayOrigin = raycastOrigins.CenterLeft;
        Vector3 rayDir = Vector3.up;
        float raySpacing = (raycastOrigins.CenterRight - raycastOrigins.CenterLeft).magnitude/(roofCheckRayNumber-1);
        RaycastHit2D hit;


        if (vel.y > 0)
        {
            float[] angles = new float[roofCheckRayNumber];
            bool atLeast1Hit = false;
            for (int i = 0; i < roofCheckRayNumber; i++)
            {
                angles[i] = -1;
                Vector3 currentRayOrigin = rayOrigin + (Vector3.right *raySpacing * i);
                if (!disableAllRays)
                {
                    Debug.DrawRay(currentRayOrigin, rayDir * rayLength, Color.red);
                }
                if (ThrowRaycast2D(currentRayOrigin, rayDir, out hit, rayLength, collisionMask, qTI))
                {
                    if (CanCollide(hit))
                    {
                        if (hit.collider.CompareTag("Floor"))
                        {
                            float roofAngle = Vector3.Angle(hit.normal, Vector3.up);
                            angles[i] = roofAngle;
                            //Debug.Log("hitting roof with angle: " + roofAngle);
                            if (!disableAllRays) Debug.DrawRay(hit.point, Vector2.Perpendicular(hit.normal).normalized * 0.1f, Color.green);
                            if(!atLeast1Hit) atLeast1Hit = true;
                        }
                    }
                }
            }
            if (atLeast1Hit)
            {
                float averageAngle = 0;
                int hits = 0;
                for (int i = 0; i < angles.Length; i++)
                {
                    if(angles[i] > 0)
                    {
                        hits++;
                        averageAngle += angles[i];
                    }
                }
                averageAngle /= hits;
                //Debug.Log("ROOF AVERAGE ANGLE = " + averageAngle);
                if(averageAngle>maxRoofAngle)
                    above = true;
            }

        }
    }

    #endregion

    #endregion


    #region --- MOVING PLATFORMS ---
    Vector3 platformMovement;
    Vector3 platformOldWorldPoint;
    Vector3 platformOldLocalPoint;
    Vector3 platformNewWorldPoint;
    //Only for deciding if we calculate new platform movement, not used to decide if we need to save the current position and platform
    bool onMovingPlatform
    {
        get
        {
            if (!disableAllDebugs) Debug.Log("onMovingPlatform-> below = " + below + "; lastBelow = " + lastBelow + "; floor = " + floor + "; lastFloor = " + lastFloor);
            return below && lastBelow && !sliping && floor != null && lastFloor == floor; /*&& collisions.lastBelow && collisions.lastFloor != null && collisions.lastFloor == collisions.floor*/
        }
    }

    public void SavePlatformPoint()
    {
        if (below && !sliping && floor != null)
        {
            platformOldWorldPoint = groundContactPoint;
            platformOldLocalPoint = floor.transform.InverseTransformPoint(platformOldWorldPoint);
            /*if (!disableAllDebugs) */
            if (!disableAllDebugs) Debug.Log("OnMovingPlatform true && Save Platform Point: Local = " + platformOldLocalPoint.ToString("F4") + "; world = " + platformOldWorldPoint.ToString("F4"));
        }
        else
        {
            platformOldWorldPoint = Vector3.zero;
        }
    }

    void CalculatePlatformPointMovement()
    {
        if (onMovingPlatform && platformOldWorldPoint != Vector3.zero)
        {
            /*if (!disableAllDebugs)*/
            //Debug.Log("CALCULATE PLATFORM POINT MOVEMENT");
            platformNewWorldPoint = floor.transform.TransformPoint(platformOldLocalPoint);
            platformMovement = platformNewWorldPoint - platformOldWorldPoint;
            /*if (!disableAllDebugs)*/
            if (!disableAllDebugs) Debug.Log("platformOldWorldPoint = " + platformOldWorldPoint.ToString("F4") + "; New Platform Point = "
                + platformNewWorldPoint.ToString("F4") + "; platformMovement = " + platformMovement.ToString("F8"));
            /*if (!disableAllRays)*/
            Debug.DrawLine(platformOldWorldPoint, platformNewWorldPoint, Color.red, 1f);
        }
    }

    public Vector3 ChangePositionWithPlatform(bool instantMode)
    {
        platformMovement = Vector3.zero;
        CalculatePlatformPointMovement();

        //transform.Translate(platformMovement, Space.World);
        if (onMovingPlatform && platformMovement.magnitude > 0.0001f)
        {
            if (platformMovement.magnitude > 1) Debug.LogError("ERROR: MOVIMIENTO EXCESIVO");
            //GetComponent<Rigidbody>().velocity = myPlayerMov.currentVel + platformMovement * (1 / Time.fixedDeltaTime) * 100000;
            //transform.position += platformMovement;
            //rb.MovePosition(rb.position + platformMovement);
            if (instantMode)
            {
                Vector3 auxPlatformMovement = platformMovement;
                auxPlatformMovement.y = 0;
                rb.position += (Vector2)auxPlatformMovement;
            }
            //Debug.LogWarning("platformMovement = " + platformMovement.ToString("F8")+"; newPos = "+platformNewWorldPoint.ToString("F8") + "; current pos = "+rb.position.ToString("F8"));
        }
        return platformMovement;
    }
    #endregion

    #region --- AUXILIAR ---
    bool ThrowRaycast2D(Vector2 origin, Vector2 direction, out RaycastHit2D hit, float maxDist, int layerMask, QueryTriggerInteraction qTI)
    {
        Ray auxRay = new Ray(origin, direction);
        int length = Physics2D.RaycastNonAlloc(origin, direction, hits, maxDist, layerMask);
        bool raycastHasHit = false;
        hit = new RaycastHit2D();
        for (int i = 0; i < length && !raycastHasHit; i++)
        {
            if (CanCollide(hits[i]))
            {
                raycastHasHit = true;
                hit = hits[i];
            }
        }
        return raycastHasHit;
    }

    /// <summary>
    /// //Funcion que calcula el angulo de un vector respecto a otro que se toma como referencia de "foward"
    /// </summary>
    /// <param name="referenceForward"></param>
    /// <param name="newDirection"></param>
    /// <returns></returns>
    float SignedRelativeAngle(Vector3 referenceForward, Vector3 newDirection, Vector3 referenceUp)
    {
        // the vector perpendicular to referenceForward (90 degrees clockwise)
        // (used to determine if angle is positive or negative)
        Vector3 referenceRight = Vector3.Cross(referenceUp, referenceForward);
        // Get the angle in degrees between 0 and 180
        float angle = Vector3.Angle(newDirection, referenceForward);
        // Determine if the degree value should be negative.  Here, a positive value
        // from the dot product means that our vector is on the right of the reference vector   
        // whereas a negative value means we're on the left.
        float sign = Mathf.Sign(Vector3.Dot(newDirection, referenceRight));
        return (sign * angle);//final angle
    }

    /// <summary>
    /// Auxiliar function that draws a plane given it's normal, a point in it, and a color. The normal will always be drawn red.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normal"></param>
    /// <param name="planeColor"></param>
    void DrawPlane(Vector3 position, Vector3 normal, Color planeColor, float size = 1f)
    {
        //if (!disableAllDebugs) Debug.Log("DRAWING PLANE WITH NORMAL " + normal.ToString("F4") + "; planeColor = " + planeColor + "; size = " + size);
        if (normal == Vector3.zero) Debug.LogError("ERROR WHILE DRAWING PLANE: normal = (0,0,0) ");
        Vector3 v3;
        normal = (normal.normalized) * 2 * size;
        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
        else
            v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude; ;

        Vector3 corner0 = position + v3;
        Vector3 corner2 = position - v3;
        Quaternion q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        Vector3 corner1 = position + v3;
        Vector3 corner3 = position - v3;

        Debug.DrawLine(corner0, corner2, planeColor);
        Debug.DrawLine(corner1, corner3, planeColor);
        Debug.DrawLine(corner0, corner1, planeColor);
        Debug.DrawLine(corner1, corner2, planeColor);
        Debug.DrawLine(corner2, corner3, planeColor);
        Debug.DrawLine(corner3, corner0, planeColor);
        Debug.DrawRay(position, normal, Color.blue);
    }

    #endregion

    #region --- SAFE BELOW ---
    public void StartSafeBelow()
    {
        if (!safeBelowStarted)
        {
            safeBelowStarted = true;
            safeBelowMaxTime = 0.14f;
            safeBelowTime = 0;
        }
    }

    public void ProcessSafeBelow()
    {
        if (!lastBelow && below)
        {
            safeBelow = true;
            if (safeBelowStarted)
            {
                safeBelowStarted = false;
            }
        }

        if (safeBelowStarted)
        {
            safeBelowTime += Time.deltaTime;
            //print("safeBelowTime = " + safeBelowTime);
            if (safeBelowTime >= safeBelowMaxTime)
            {
                EndSafeBelow();
            }
        }
    }

    void EndSafeBelow()
    {
        safeBelowStarted = false;
        safeBelow = false;
        safeBelowTime = 0;
    }
    #endregion

    #region --- JUMP ---
    public void StartJump()
    {
        below = false;
        justJumped = true;
    }
    #endregion

    bool CanCollide(RaycastHit2D hit)
    {
        if (hit.collider.isTrigger)
        {
            return false;
        }
        else
        {
            if (hit.collider.tag == "PlayerCollider")
            {
                PlayerMovementCMF otherPlayer = hit.collider.GetComponentInParent<PlayerMovementCMF>();
                //Debug.LogWarning("otherPlayer = "+ otherPlayer);
                if (otherPlayer != null && otherPlayer != myPlayerMov)
                {
                    //Debug.LogWarning("COLLIDED WITH TRIGGER BUT IS ANOTHER PLAYER! otherPlayer = "+ otherPlayer + "; hit.transform = " + hit.collider);
                    return true;
                }
                else
                {
                    //Debug.LogWarning("COLLIDED WITH TRIGGER (MYSELF)");
                    return false;
                }
            }
            else if (hit.collider.tag == "Enemy")
            {

                EnemyAI otherEnemy = hit.collider.GetComponentInParent<EnemyAI>();
                //Debug.LogWarning("otherPlayer = "+ otherPlayer);
                if (otherEnemy != null && otherEnemy != myEnemy)
                {
                    //Debug.LogWarning("COLLIDED WITH TRIGGER BUT IS ANOTHER PLAYER! otherPlayer = "+ otherPlayer + "; hit.transform = " + hit.collider);
                    return true;
                }
                else
                {
                    //Debug.LogWarning("COLLIDED WITH TRIGGER (MYSELF)");
                    return false;
                }
            }
            else return true;

        }
    }

    void UpdateSafeBelow()
    {
        if (!below && lastBelow)
        {
            StartSafeBelow();
        }
        ProcessSafeBelow();
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = coll.bounds;

        raycastOrigins.BottomLFCornerReal = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        raycastOrigins.BottomRFCornerReal = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        raycastOrigins.BottomLBCornerReal = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
        raycastOrigins.BottomRBCornerReal = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);

        raycastOrigins.TopLFCornerReal = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        raycastOrigins.TopRFCornerReal = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);
        raycastOrigins.TopLBCornerReal = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        raycastOrigins.TopRBCornerReal = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);

        raycastOrigins.BottomCentre = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        //--------------------------------- BOUNDS REDUCED BY SKINWIDTH ---------------------------------
        bounds.Expand(skinWidth * -2);

        raycastOrigins.BottomEnd = new Vector3(bounds.center.x, bounds.min.y, bounds.max.z);

        raycastOrigins.Center = bounds.center;
        raycastOrigins.CenterLeft = new Vector2(bounds.min.x, bounds.center.y);
        raycastOrigins.CenterRight = new Vector2(bounds.max.x, bounds.center.y);
        raycastOrigins.AroundRadius = bounds.size.z / 2;

    }

    struct RaycastOrigins
    {
        public Vector3 BottomEnd;//TopEnd= center x, min y, max z
        public Vector3 BottomCentre;

        public Vector3 BottomLFCornerReal, BottomRFCornerReal, BottomLBCornerReal, BottomRBCornerReal;
        public Vector3 TopLFCornerReal, TopRFCornerReal, TopLBCornerReal, TopRBCornerReal;

        public Vector3 Center,CenterLeft, CenterRight;
        public float AroundRadius;
    }

    public enum SpherePosition
    {
        None,
        Top,
        Middle,
        Bottom
    }

    public struct WallCollisionHit
    {
        public SpherePosition spherePos;
        public Collider collider;
        public Vector3 point;
        public RaycastHit hit;
        public float slopeAngle;

        public Vector3 rayOrigin;
        public Vector3 rayDir;
        public float rayLength;

        public WallCollisionHit(SpherePosition _spherePos, RaycastHit _hit, Vector3 _rayOrigin, Vector3 _rayDir, float _rayLength = 0, float _slopeAngle = 0)
        {
            spherePos = _spherePos;
            collider = _hit.collider;
            point = _hit.point;
            hit = _hit;
            slopeAngle = _slopeAngle;
            rayOrigin = _rayOrigin;
            rayDir = _rayDir;
            rayLength = _rayLength;
        }
    }
}
