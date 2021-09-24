﻿using UnityEngine;
using System.Collections;

//This script handles all physics, collision detection and ground detection;
//It expects a movement velocity (via 'SetVelocity') every 'FixedUpdate' frame from an external script (like a controller script) to work;
//It also provides several getter methods for important information (whether the mover is grounded, the current surface normal [...]);
[ExecuteInEditMode]
public class Mover : MonoBehaviour {
    public bool debugModeOn = true;
    public bool movementDebugsOn = true;
    [Tooltip("if false, it will be 'smoothen', or moved little by little in every Update frame in between fixed updates")]
    public bool instantGroundAdjustment = true;
    [Tooltip("if false, it will be 'smoothen', or moved little by little in every Update frame in between fixed updates")]
    public bool instantPlatformMovement = true;

	//Collider variables;
	[Range(0f, 1f)] public float stepHeightRatio = 0.25f;
	[SerializeField] public float colliderHeight = 2f;
	[SerializeField] public float colliderThickness = 1f;
	[SerializeField] public Vector3 colliderOffset = Vector3.zero;

	//References to attached collider(s);
	BoxCollider2D boxCollider;
    [HideInInspector]
	public CapsuleCollider2D capsuleCollider;

	//Sensor variables;
	public Sensor2D.CastType sensorType = Sensor2D.CastType.Raycast;
	private float sensorRadiusModifier = 0.8f;
	public LayerMask sensorLayermask  = ~0;
	public int sensorArrayRows = 1;
	public int sensorArrayRayCount = 6;
	public bool sensorArrayRowsAreOffset = false;

    //Ground detection variables;
    public float maxSlopeAngle = 60f;
	bool isGrounded = false;
    [HideInInspector]
    public bool stickToGround = true;
    [HideInInspector]
    public bool isGroundTooSteep = false;

    //Moving Platforms
    [HideInInspector]
    public bool onMovingPlatform = false; 

	//Sensor range variables;
	bool IsUsingExtendedSensorRange  = true;
	float baseSensorRange = 0f;

	//Current upwards (or downwards) velocity necessary to keep the correct distance to the ground;
	Vector3 currentGroundAdjustmentVelocity = Vector3.zero;

    //Current velocity necessary to follow a platform's movement
    Vector3 currentPlatformMovement = Vector3.zero;

	//References to attached components;
	Collider2D col;
	Rigidbody2D rig;
	Transform tr;
    [HideInInspector]
	public Sensor2D sensor;

	void Awake()
	{
		Setup();

		//Initialize sensor;
		sensor = new Sensor2D(this.tr, col);
        sensor.maxSlopeAngle = maxSlopeAngle;
		RecalculateColliderDimensions();
		RecalibrateSensor();
	}

	void Reset () {
		Setup();
	}

	//Setup references to components;
	void Setup()
	{
		tr = GetComponent<Transform>();
		col = GetComponent<Collider2D>();

		//If no collider is attached to this gameobject, add a collider;
		if(col == null)
		{
			tr.gameObject.AddComponent<CapsuleCollider2D>();
			col = GetComponent<Collider2D>();
		}

		rig = GetComponent<Rigidbody2D>();

		//If no rigidbody is attached to this gameobject, add a rigidbody;
		if(rig == null)
		{
			tr.gameObject.AddComponent<Rigidbody2D>();
			rig = GetComponent<Rigidbody2D>();
		}

		boxCollider = GetComponent<BoxCollider2D>();
		capsuleCollider = GetComponent<CapsuleCollider2D>();

		//Freeze rigidbody rotation and disable rigidbody gravity;
		rig.freezeRotation = true;
		rig.gravityScale =0;
	}

	//Recalculate collider height/width/thickness;
	public void RecalculateColliderDimensions()
	{
		//Check if a collider is attached to this gameobject;
		if(col == null)
		{
			//Try to get a reference to the attached collider by calling Setup();
			Setup();

			//Check again;
			if(col == null)
			{
				Debug.LogWarning("There is no collider attached to " + this.gameObject.name + "!");
				return;
			}				
		}

		//Set collider dimensions based on collider variables;
		if(boxCollider)
		{
			Vector3 _size = Vector3.zero;
			_size.x = colliderThickness;
			_size.z = colliderThickness;

			boxCollider.offset = colliderOffset * colliderHeight;

			_size.y = colliderHeight * (1f - stepHeightRatio);
			boxCollider.size = _size;

			boxCollider.offset = boxCollider.offset + new Vector2(0f, stepHeightRatio * colliderHeight/2f);
		}
		else if (capsuleCollider) {
	
			capsuleCollider.size = new Vector2(colliderThickness / 2f, colliderHeight);
			capsuleCollider.offset = colliderOffset * colliderHeight;

			capsuleCollider.offset = capsuleCollider.offset + new Vector2(0f, stepHeightRatio * capsuleCollider.size.y/2f);
        capsuleCollider.size = new Vector2(capsuleCollider.size.x, capsuleCollider.size.y * (1f - stepHeightRatio));

			if(capsuleCollider.size.y/2f < capsuleCollider.size.x)
				capsuleCollider.size = new Vector2(capsuleCollider.size.y/ 2f, capsuleCollider.size.y);
		}

		//Recalibrate sensor variables to fit new collider dimensions;
		RecalibrateSensor();
	}

	//Recalibrate sensor variables;
	void RecalibrateSensor()
	{
		//Set sensor ray origin and direction;
		sensor.SetCastOrigin(GetColliderCenter());
		sensor.SetCastDirection(Sensor2D.CastDirection.Down);

		//Make sure that the selected layermask does not include the 'Ignore Raycast' layer;
		sensorLayermask ^= (1 << LayerMask.NameToLayer("Ignore Raycast"));

		//Set sensor cast type and layermask;
		sensor.castType = sensorType;
		sensor.layermask = sensorLayermask;

		//Calculate sensor radius/width;
		float _radius = colliderThickness/2f * sensorRadiusModifier;

		//Multiply all sensor lengths with '_safetyDistanceFactor' to compensate for floating point errors;
		float _safetyDistanceFactor = 0.001f;

		//Fit collider height to sensor radius;
		if(boxCollider)
			_radius = Mathf.Clamp(_radius, _safetyDistanceFactor, (boxCollider.size.y/2f) * (1f - _safetyDistanceFactor));
		else if(capsuleCollider)
			_radius = Mathf.Clamp(_radius, _safetyDistanceFactor, (capsuleCollider.size.y/2f) * (1f - _safetyDistanceFactor));

		//Set sensor variables;

		//Set sensor radius;
		sensor.sphereCastRadius = _radius;

		//Calculate and set sensor length;
		float _length = 0f;
		_length += (colliderHeight * (1f - stepHeightRatio)) * 0.5f;
		_length += colliderHeight * stepHeightRatio;
		baseSensorRange = _length * (1f + _safetyDistanceFactor);
		sensor.castLength = _length;

		//Set sensor array variables;
		sensor.ArrayRows = sensorArrayRows;
		sensor.arrayRayCount = sensorArrayRayCount;
		sensor.offsetArrayRows = sensorArrayRowsAreOffset;
		sensor.isInDebugMode = debugModeOn;

		//Set sensor spherecast variables;
		sensor.calculateRealDistance = true;
		sensor.calculateRealSurfaceNormal = true;

		//Recalibrate sensor to the new values;
		sensor.RecalibrateRaycastArrayPositions();
	}

	//Returns the collider's center in world coordinates;
	Vector3 GetColliderCenter()
	{
		if(col == null)
			Setup();

		return col.bounds.center;
	}

	//Check if mover is grounded;
	//Store all relevant collision information for later;
	//Calculate necessary adjustment velocity to keep the correct distance to the ground;
	void Check()
	{
		//Reset ground adjustment velocity;
		currentGroundAdjustmentVelocity = Vector3.zero;

        //Set sensor length;
        if (IsUsingExtendedSensorRange)
			sensor.castLength = baseSensorRange + colliderHeight * stepHeightRatio;
		else
			sensor.castLength = baseSensorRange;
		
		sensor.Cast();

		//If sensor has not detected anything, set flags and return;
		if(!sensor.HasDetectedHit())
		{
			isGrounded = false;
			return;
		}

		//Set flags for ground detection;
		isGrounded = true;

		//Get distance that sensor ray reached;
		float _distance = sensor.GetDistance();

		//Calculate how much mover needs to be moved up or down;
		float _upperLimit = (colliderHeight * (1f - stepHeightRatio)) * 0.5f;
		float _middle = _upperLimit + colliderHeight * stepHeightRatio;
		float _distanceToGo = _middle - _distance;

        //Set new ground adjustment velocity for the next frame;
        //Eloy's addition:
        float slopeAngle = Vector3.Angle(GetGroundNormal(), Vector3.up);
        isGroundTooSteep = isGrounded && slopeAngle > maxSlopeAngle;

        //Stick to ground again
        //if (!stickToGround && isGrounded && slopeAngle <= 60) stickToGround = true;

        //if (stickToGround && isGrounded && slopeAngle <= 60)
            currentGroundAdjustmentVelocity = instantGroundAdjustment && onMovingPlatform? tr.up * _distanceToGo : tr.up * (_distanceToGo/Time.fixedDeltaTime);
        if((!stickToGround && currentGroundAdjustmentVelocity.y < 0) || isGroundTooSteep)
        {
            currentGroundAdjustmentVelocity.y = 0;
        }
	}

	//Check if mover is grounded;
	public void CheckForGround(bool _onMovingPlatform = false)
	{
        //ChangePositionWithPlatform();
        onMovingPlatform = _onMovingPlatform;

        Check();

        //SavePlatformPoint();
	}

	//Set mover velocity;
	public void SetVelocity(Vector3 _velocity, Vector3 platformMovement)
	{
        if (!instantPlatformMovement) currentPlatformMovement = platformMovement * (1 / Time.fixedDeltaTime);
        else currentPlatformMovement = Vector3.zero;

        if (currentGroundAdjustmentVelocity.magnitude != 0) currentPlatformMovement.y = 0;

        rig.velocity = _velocity + currentPlatformMovement;
        if (instantGroundAdjustment && onMovingPlatform) rig.position += (Vector2)currentGroundAdjustmentVelocity;
        else rig.velocity += (Vector2)currentGroundAdjustmentVelocity;

//        if (debugModeOn && movementDebugsOn) Debug.LogWarning("Velocity = " + _velocity.ToString("F6") + "; currentPlatformMovement = " + currentPlatformMovement.ToString("F6") +
//"; currentGroundAdjustmentVelocity = " + currentGroundAdjustmentVelocity.ToString("F8"));
    }

    //Returns 'true' if mover is touching ground and the angle between hte 'up' vector and ground normal is not too steep (e.g., angle < slope_limit);
    public bool IsGrounded()
	{
		return isGrounded;
	}

    #region --- MOVING PLATFORMS ---
    //Vector3 platformMovement;
    //Vector3 platformOldWorldPoint;
    //Vector3 platformOldLocalPoint;
    //Vector3 platformNewWorldPoint;
    //bool onMovingPlatform
    //{
    //    get
    //    {
    //        if (!disableAllDebugs) Debug.Log("onMovingPlatform-> isGrounded = " + isGrounded + "; isGroundTooSteep = "+ isGroundTooSteep + "; GetGroundCollider() = " + GetGroundCollider());
    //        return isGrounded && !isGroundTooSteep && GetGroundCollider() != null /*&& collisions.lastBelow && collisions.lastFloor != null && collisions.lastFloor == collisions.floor*/;
    //    }
    //}

    //void SavePlatformPoint()
    //{
    //    if (isGrounded && !isGroundTooSteep && GetGroundCollider() != null)
    //    {
    //        platformOldWorldPoint = GetGroundPoint();
    //        platformOldLocalPoint = GetGroundCollider().transform.InverseTransformPoint(platformOldWorldPoint);
    //        if (!disableAllDebugs) Debug.Log("OnMovingPlatform true && Save Platform Point: Local = " + platformOldLocalPoint.ToString("F4") + "; world = " + platformOldWorldPoint.ToString("F4"));
    //    }
    //}

    //void CalculatePlatformPointMovement()
    //{
    //    if (onMovingPlatform)
    //    {
    //        if (!disableAllDebugs) Debug.LogWarning("CALCULATE PLATFORM POINT MOVEMENT");
    //        platformNewWorldPoint = GetGroundCollider().transform.TransformPoint(platformOldLocalPoint);
    //        platformMovement = platformNewWorldPoint - platformOldWorldPoint;
    //        if (!disableAllDebugs) Debug.Log("platformOldWorldPoint = " + platformOldWorldPoint.ToString("F4") + "; New Platform Point = " + platformNewWorldPoint.ToString("F4") + "; platformMovement = " + platformMovement.ToString("F4"));
    //        if (!disableAllDebugs) Debug.DrawLine(platformOldWorldPoint, platformNewWorldPoint, Color.red);
    //    }
    //}

    //void ChangePositionWithPlatform()
    //{
    //    //Reset platform's movement velocity
    //    currentPlatformMovement = Vector3.zero;

    //    CalculatePlatformPointMovement();

    //    //transform.Translate(platformMovement, Space.World);
    //    if (onMovingPlatform)
    //        currentPlatformMovement = platformMovement * (1/Time.fixedDeltaTime);
    //    //transform.position += platformMovement;

    //}
    #endregion

    //Setters;

    //Set whether sensor range should be extended;
    public void SetExtendSensorRange(bool _isExtended)
	{
		IsUsingExtendedSensorRange = _isExtended;
	}

	//Set height of collider;
	public void SetColliderHeight(float _newColliderHeight)
	{
		if(colliderHeight == _newColliderHeight)
			return;

		colliderHeight = _newColliderHeight;
		RecalculateColliderDimensions();
	}

	//Set acceptable step height;
	public void SetStepHeightRatio(float _newStepHeightRatio)
	{
		_newStepHeightRatio = Mathf.Clamp(_newStepHeightRatio, 0f, 1f);
		stepHeightRatio = _newStepHeightRatio;
		RecalculateColliderDimensions();
	}

	//Getters;

	public Vector3 GetGroundNormal()
	{
		return sensor.GetNormal();
	}

	public Vector3 GetGroundPoint()
	{
		return sensor.GetPosition();
	}

	public Collider2D GetGroundCollider()
	{
		return sensor.GetCollider();
	}
	
}
