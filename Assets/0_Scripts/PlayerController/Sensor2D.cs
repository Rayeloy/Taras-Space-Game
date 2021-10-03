using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This script is responsible for casting rays and spherecasts;
//It is instantiated by the 'Mover' component at runtime;
[System.Serializable]
public class Sensor2D {

    //Slope Max Angle
    public float maxSlopeAngle=55;

	//Basic raycast parameters;
	public float castLength = 1f;
	public float sphereCastRadius = 0.2f;

	//Starting point of (ray-)cast;
	private Vector2 origin = Vector2.zero;

    Color darkRed = new Color(0.533f, 0.031f, 0.027f);

	//Enum describing local transform axes used as directions for raycasting;
	public enum CastDirection
	{
		Forward,
		Right,
		Up,
		Backward, 
		Left,
		Down
	}

	private CastDirection castDirection;

	//Raycast hit information variables;
	private bool hasDetectedHit;
	private Vector3 hitPosition;
	private Vector2 hitNormal;
	private float hitDistance;
	private List<Collider2D> hitColliders = new List<Collider2D>();
	private List<Transform> hitTransforms = new List<Transform>();

	//Backup normal used for specific edge cases when using spherecasts;
	private Vector2 backupNormal;

	//References to attached components;
	private Transform tr;
	public Collider2D col;

	//Enum describing different types of ground detection methods;
	[SerializeField]
	public enum CastType
	{
		Raycast,
		RaycastArray,
        RaycastArray2D,
		Spherecast
	}

	public CastType castType = CastType.Raycast;
	public LayerMask layermask = 255;

	//Layer number for 'Ignore Raycast' layer;
	int ignoreRaycastLayer;

	//Spherecast settings;

	//Cast an additional ray to get the true surface normal;
	public bool calculateRealSurfaceNormal = false;
	//Cast an additional ray to get the true distance to the ground;
	public bool calculateRealDistance = false;

	//Array raycast settings;

	//Number of rays in every row;
	public int arrayRayCount = 9;
	//Number of rows around the central ray;
	public int ArrayRows = 3;
	//Whether or not to offset every other row;
	public bool offsetArrayRows = false;

    //Array raycast 2D settings;

    //Number of rays in every row;
    public int arrayRayCount2D = 4;


    //Array containing all array raycast start positions (in local coordinates);
    private Vector2[] raycastArrayStartPositions;

	//Optional list of colliders to ignore when raycasting;
	private Collider2D[] ignoreList;

	//Whether to draw debug information (hit positions, hit normals...) in the editor;
	public bool isInDebugMode = false;

	List<Vector2> arrayNormals = new List<Vector2>();
	List<Vector2> arrayPoints = new List<Vector2>();
	List<float> arrayDist = new List<float>();

    //Constructor;
    public Sensor2D (Transform _transform, Collider2D _collider)
	{
		tr = _transform;

		if(_collider == null)
			return;

		ignoreList = new Collider2D[1];

		//Add collider to ignore list;
		ignoreList[0] = _collider;

		//Store "Ignore Raycast" layer number for later;
		ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
	}

	//Reset all variables related to storing information on raycast hits;
	private void ResetFlags()
	{
		hasDetectedHit = false;
		hitPosition = Vector2.zero;
		hitNormal = -GetCastDirection();
		hitDistance = 0f;

		if(hitColliders.Count > 0)
			hitColliders.Clear();
		if(hitTransforms.Count > 0)
			hitTransforms.Clear();
	}

	//Returns an array containing the starting positions of all array rays (in local coordinates) based on the input arguments;
	public static Vector2[] GetRaycastStartPositions(int sensorRows, int sensorRayCount, bool offsetRows, float sensorRadius)
	{
		//Initialize list used to store the positions;
		List<Vector2> _positions = new List<Vector2>();

		//Add central start position to the list;
		Vector2 _startPosition = Vector2.zero;
		_positions.Add(_startPosition);

		for(int i = 0; i < sensorRows; i++)
		{
			//Calculate radius for all positions on this row;
			float _rowRadius = (float)(i+1)/sensorRows; 

			for(int j = 0; j < sensorRayCount * (i + 1); j++)
			{
				//Calculate angle (in degrees) for this individual position;
				float _angle = (360f/(sensorRayCount * (i + 1))) * j;	

				//If 'offsetRows' is set to 'true', every other row is offset;
				if(offsetRows && i % 2 == 0)	
					_angle += (360f/(sensorRayCount * (i + 1)))/2f;

				//Combine radius and angle into one position and add it to the list;
				float _x = _rowRadius * Mathf.Cos(Mathf.Deg2Rad * _angle);
				float _y = _rowRadius * Mathf.Sin(Mathf.Deg2Rad * _angle);

				_positions.Add(new Vector2(_x, _y) * sensorRadius);
			}
		}
		//Convert list to array and return array;
		return _positions.ToArray();
	}

    //Cast a ray (or sphere or array of rays) to check for colliders;
    public void Cast()
	{
		ResetFlags();

		//Calculate origin and direction of ray in world coordinates;
		Vector2 _worldDirection = GetCastDirection();
		Vector2 _worldOrigin = tr.TransformPoint(origin);

		//(Temporarily) move all objects in ignore list to 'Ignore Raycast' layer;
		int[] _oldLayers = new int[ignoreList.Length];
		for(int i = 0; i < ignoreList.Length; i++)
		{
			_oldLayers[i] = ignoreList[i].gameObject.layer;
			ignoreList[i].gameObject.layer = ignoreRaycastLayer;
		}

		//Depending on the chosen mode of detection, call different functions to check for colliders;
		switch (castType)
		{
			case CastType.Raycast:
				CastRay(_worldOrigin, _worldDirection);
				break;
			case CastType.Spherecast:
				CastSphere(_worldOrigin, _worldDirection);
				break;
            case CastType.RaycastArray:
                CastRayArray(_worldOrigin, _worldDirection);
                break;
            case CastType.RaycastArray2D:                
                CastRayArray2D(_worldOrigin, _worldDirection);
                break;
            default:
				hasDetectedHit = false;
				break;
		}

		//If debug mode is enabled, draw lines to show where the ray hit a collider and the resulting surface normal;
		if(hasDetectedHit && isInDebugMode)
		{
			Debug.DrawRay(hitPosition, hitNormal, Color.red, Time.deltaTime);
			float _markerSize = 0.2f;
			Debug.DrawLine(hitPosition + Vector3.up * _markerSize, hitPosition - Vector3.up * _markerSize, Color.green, Time.deltaTime);
			Debug.DrawLine(hitPosition + Vector3.right * _markerSize, hitPosition - Vector3.right * _markerSize, Color.green, Time.deltaTime);
		}

		//Reset collider layers in ignoreList;
		for(int i = 0; i < ignoreList.Length; i++)
		{
			ignoreList[i].gameObject.layer = _oldLayers[i];
		}
	}

	//Cast an array of rays into '_direction' and centered around '_origin';
	private void CastRayArray(Vector3 _origin, Vector2 _direction)
	{
        //Calculate origin and direction of ray in world coordinates;
        Vector2 _rayStartPosition = Vector2.zero;
        Vector2 rayDirection = GetCastDirection();

		//Clear results from last frame;
		arrayNormals.Clear();
		arrayPoints.Clear();

        //Cast array;
        for (int i = 0; i < raycastArrayStartPositions.Length; i++)
		{
			//Calculate ray start position;
			_rayStartPosition = _origin + tr.TransformDirection(raycastArrayStartPositions[i]);
            if (isInDebugMode)
                Debug.DrawRay(_rayStartPosition, rayDirection, darkRed, Time.fixedDeltaTime * 1.01f);
            RaycastHit2D _hit = Physics2D.Raycast(_rayStartPosition, rayDirection, castLength,layermask);
            if (_hit.collider != null)
            {
                    if (isInDebugMode)
                        Debug.DrawRay(_hit.point, _hit.normal, Color.red, Time.fixedDeltaTime * 1.01f);

                    hitColliders.Add(_hit.collider);
                    hitTransforms.Add(_hit.transform);
                    arrayNormals.Add(_hit.normal);
                    arrayPoints.Add(_hit.point);
            }

		}

        //Evaluate results;
        List<Vector2> auxArrayPoints = new List<Vector2>();

        for (int i = 0; i < arrayPoints.Count; i++)
        {
            float slopeAngle = Vector2.Angle(arrayNormals[i], Vector2.up);
            if (slopeAngle <= maxSlopeAngle)
            {
                auxArrayPoints.Add(arrayPoints[i]);
            }
        }
        //arrayPoints.Clear();
        arrayPoints = auxArrayPoints;

        hasDetectedHit = (arrayPoints.Count > 0);

		if(hasDetectedHit)
		{
            //Calculate average surface normal;
            Vector2 _averageNormal = Vector2.zero;
			foreach(Vector2 v in arrayNormals)
			{
				_averageNormal += v;
			}

			_averageNormal.Normalize();

            //Calculate average surface point;
            Vector2 _averagePoint = Vector2.zero;
            for (int i = 0; i < arrayPoints.Count; i++)
            {
                    _averagePoint += arrayPoints[i];
            }
            //foreach (Vector3 v in arrayPoints)
            //{
            //    _averagePoint += v;
            //}

            _averagePoint /= arrayPoints.Count;
			
			hitPosition = _averagePoint;
			hitNormal = _averageNormal;
			hitDistance = VectorMath.ExtractDotVector2D(_origin - hitPosition, _direction).magnitude;
		}
	}

    //Cast an array of rays into '_direction' and centered around '_origin';
    private void CastRayArray2D(Vector3 _origin, Vector2 _direction)
    {
        
        float thickness = 0.0f;
        //Calculate origin and direction of ray in world coordinates;
        Vector2 _rayStartPosition = _origin+(Vector3.left*col.bounds.extents.x)+Vector3.right*thickness;
        Vector2 rayDirection = GetCastDirection();

        //Clear results from last frame;
        arrayNormals.Clear();
        arrayPoints.Clear();
        arrayDist.Clear();

        float spacing = (col.bounds.size.x- (thickness*2)) / (arrayRayCount2D - 1);
        //Cast array;
        for (int i = 0; i < arrayRayCount2D; i++)
        {
            //Calculate ray start position;
            Vector3 currentRayStartPosition = _rayStartPosition + Vector2.right * spacing * i;

            RaycastHit2D _hit = Physics2D.Raycast(currentRayStartPosition, rayDirection, castLength, layermask);
            if (_hit.collider != null)
            {
                if (isInDebugMode)
                {
                    Debug.DrawRay(currentRayStartPosition, Vector2.left * 0.1f, Color.yellow);
                    Debug.DrawRay(currentRayStartPosition, rayDirection.normalized* castLength, Color.red);
                    Debug.DrawRay(_hit.point, Vector2.Perpendicular(_hit.normal).normalized*0.1f, Color.blue);

                }
                //Debug.Log("HIT with " + _hit.collider.gameObject.name);
                hitColliders.Add(_hit.collider);
                hitTransforms.Add(_hit.transform);
                arrayNormals.Add(_hit.normal);
                arrayPoints.Add(_hit.point);
                arrayDist.Add(_hit.distance);
            }
            else
            {
                if (isInDebugMode)
                {
                    Debug.DrawRay(currentRayStartPosition, rayDirection.normalized * castLength, darkRed);
                }
            }

        }

        //Evaluate results;
        List<Vector2> auxArrayPoints = new List<Vector2>();

        for (int i = 0; i < arrayPoints.Count; i++)
        {
            //float slopeAngle = Vector2.Angle(arrayNormals[i], Vector2.up);
            if (/*slopeAngle <= maxSlopeAngle &&*/ arrayDist[i] > 0.05f)
            {
                //Debug.Log("arrayDist[i]  = " + arrayDist[i] + "; slopeAngle = " + slopeAngle);
                auxArrayPoints.Add(arrayPoints[i]);
            }
        }
        //arrayPoints.Clear();
        arrayPoints = auxArrayPoints;

        hasDetectedHit = (arrayPoints.Count > 0);

        if (hasDetectedHit)
        {
            //Calculate average surface normal;
            Vector2 _averageNormal = Vector2.zero;
            foreach (Vector2 v in arrayNormals)
            {
                _averageNormal += v;
            }

            _averageNormal.Normalize();

            //Calculate average surface point;
            Vector2 _averagePoint = Vector2.zero;
            for (int i = 0; i < arrayPoints.Count; i++)
            {
                _averagePoint += arrayPoints[i];
            }
            //foreach (Vector3 v in arrayPoints)
            //{
            //    _averagePoint += v;
            //}

            _averagePoint /= arrayPoints.Count;

            hitPosition = _averagePoint;
            hitNormal = _averageNormal;
            hitDistance = VectorMath.ExtractDotVector2D(_origin - hitPosition, _direction).magnitude;
            //if (hitDistance > 0) Debug.Log("hitDistance = "+ hitDistance + "; _origin = "+ _origin + "; hitPosition = " + hitPosition);
        }
    }

    //Cast a single ray into '_direction' from '_origin';
    private void CastRay(Vector2 _origin, Vector2 _direction)
	{
		RaycastHit2D _hit;
        _hit = Physics2D.Raycast(_origin, _direction, castLength, layermask);
        hasDetectedHit = _hit;
            if (hasDetectedHit)
		{
			hitPosition = _hit.point;
			hitNormal = _hit.normal;

			hitColliders.Add(_hit.collider);
			hitTransforms.Add(_hit.transform);

			hitDistance = _hit.distance;
		}
	}

	//Cast a sphere into '_direction' from '_origin';
	private void CastSphere(Vector3 _origin, Vector3 _direction)
	{
        RaycastHit2D _hit;
        _hit = Physics2D.CircleCast(_origin, sphereCastRadius, _direction, castLength - sphereCastRadius, layermask);
        hasDetectedHit = _hit;
        /*out _hit, castLength - sphereCastRadius, layermask, QueryTriggerInteraction.Ignore*/

        if (hasDetectedHit)
		{
			hitPosition = _hit.point;
			hitNormal = _hit.normal;
			hitColliders.Add(_hit.collider);
			hitTransforms.Add(_hit.transform);

			hitDistance = _hit.distance;

			hitDistance += sphereCastRadius;

			//Calculate real distance;
			if(calculateRealDistance)
			{
				hitDistance = VectorMath.ExtractDotVector2D(_origin - hitPosition, _direction).magnitude;
			}

			Collider2D _col = hitColliders[0];
            RaycastHit2D[] _hits = new RaycastHit2D[10];

            //Calculate real surface normal by casting an additional raycast;
            if (calculateRealSurfaceNormal)
			{
                int hitsNumber = col.Raycast(_direction, _hits, 1.5f,layermask);

                if (hitsNumber>0)/*_col.Raycast(new Ray2D(hitPosition - _direction, _direction), out _hit, 1.5f)*/
				{
					if(Vector2.Angle(_hits[0].normal, -_direction) >= 89f)
						hitNormal = backupNormal;
					else
						hitNormal = _hits[0].normal;
				}
				else
					hitNormal = backupNormal;
				
				backupNormal = hitNormal;
			}
		}
	}

    //Calculate a direction in world coordinates based on the local axes of this gameobject's transform component;
    Vector2 GetCastDirection()
	{
		switch(castDirection)
		{
		case CastDirection.Forward:
			return tr.forward;

		case CastDirection.Right:
			return tr.right;

		case CastDirection.Up:
			return tr.up;

		case CastDirection.Backward:
			return -tr.forward;

		case CastDirection.Left:
			return -tr.right;

		case CastDirection.Down:
			return -tr.up;
		default:
			return Vector2.one;
		}
	}

	//Getters;

	//Returns whether the sensor has hit something;
	public bool HasDetectedHit()
	{
		return hasDetectedHit;
	}

	//Returns how far the raycast reached before hitting a collider;
	public float GetDistance()
	{
		return hitDistance;
	}

	//Returns the surface normal of the collider the raycast has hit;
	public Vector2 GetNormal()
	{
		return hitNormal;
	}

	//Returns the position in world coordinates where the raycast has hit a collider;
	public Vector2 GetPosition()
	{
		return hitPosition;
	}

	//Returns a reference to the collider that was hit by the raycast;
	public Collider2D GetCollider()
	{
        if (hitColliders.Count > 0)
            return hitColliders[0];
        else
            return null;
	}

	//Returns a reference to the transform component attached to the collider that was hit by the raycast;
	public Transform GetTransform()
	{
		return hitTransforms[0];
	}

	//Setters;

	//Set the position for the raycast to start from;
	//The input vector '_origin' is converted to local coordinates;
	public void SetCastOrigin(Vector2 _origin)
	{
		if(tr == null)
			return;
		origin = tr.InverseTransformPoint(_origin);
	}

	//Set which axis of this gameobject's transform will be used as the direction for the raycast;
	public void SetCastDirection(CastDirection _direction)
	{
		if(tr == null)
			return;

		castDirection = _direction;
	}

	//Recalculate start positions for the raycast array;
	public void RecalibrateRaycastArrayPositions()
	{
		raycastArrayStartPositions = GetRaycastStartPositions(ArrayRows, arrayRayCount, offsetArrayRows, sphereCastRadius);
	}

    public void RecalibrateRaycastArrayPositions(float radiusOffset)
    {
        raycastArrayStartPositions = GetRaycastStartPositions(ArrayRows, arrayRayCount, offsetArrayRows, sphereCastRadius - radiusOffset);

    }

    ////Recalculate start positions for the raycast array 2D;
    //public void RecalibrateRaycastArrayPositions2D()
    //{
    //    raycastArrayStartPositions = GetRaycastStartPositions2D(ArrayRows, arrayRayCount, offsetArrayRows, sphereCastRadius);
    //}
}
