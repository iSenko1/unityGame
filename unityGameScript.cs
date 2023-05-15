using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrola : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    private int currentWaypointIndex = 0;

    void Start()
    {
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;  // Start at the first waypoint
        }
    }

    void Update()
    {
        if (waypoints.Length == 0) return; // No waypoints to patrol

        // Move towards the current waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = targetWaypoint.position - transform.position;
        if (direction.magnitude < 0.1f)  // If we're close enough to the waypoint
        {
            // Move on to the next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        else
        {
            // Move towards the waypoint
            transform.position += direction.normalized * speed * Time.deltaTime;
        }
    }
}


public class MouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float SensitivityHorizontal = 9.0f;
    public float SensitivityVertical = 9.0f;
    private float rotationX = 0;
    public float minVert = -45.0f;
    public float maxVert = 45.0f;

    public GameObject bridgeTarget;  // the BridgeTarget object
    public GameObject bridge;  // the Bridge object
    public float detectionRange = 50f;  // the maximum range of detection

    void Start()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.freezeRotation = true;
        }
    }

    void Update()
    {
        if (axes == RotationAxes.MouseX)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * SensitivityHorizontal * Time.deltaTime, 0);
        }
        else if (axes == RotationAxes.MouseY)
        {
            rotationX -= Input.GetAxis("Mouse Y") * SensitivityVertical * Time.deltaTime;
            rotationX = Mathf.Clamp(rotationX, minVert, maxVert);
            float rotationY = transform.localEulerAngles.y;
            transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
        }
        else
        {
            rotationX -= Input.GetAxis("Mouse Y") * SensitivityVertical;
            rotationX = Mathf.Clamp(rotationX, minVert, maxVert);

            float rotationY = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * SensitivityHorizontal;
            transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
        }

        // Create a ray from the camera through the center of the screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Check if that ray hits anything
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, detectionRange))
        {
            // Check if the hit object is the BridgeTarget
            if (hitInfo.collider.gameObject == bridgeTarget)
            {
                // If it is, trigger the bridge to lower
                BridgeController bridgeController = bridge.GetComponent<BridgeController>();
                if (bridgeController != null)
                {
                    bridgeController.LowerBridge();
                }
            }
        }
    }
}

public class FreeView : MonoBehaviour
{
    private Camera _camera;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _camera= GetComponent<Camera>();
    }

    void Update()
    {

    }


    private void OnGUI()
    {
        int size = 12;
        float posX = _camera.pixelWidth/2 - size/4;
        float posY = _camera.pixelHeight/2 - size/2;
        GUI.Label(new Rect(posX, posY, size, size), "*");
    }
}

public class FPSInput : MonoBehaviour
{
    public float speed = 6.0f;
    public float gravity = -10f;
    private CharacterController charController;
    void Start()
    {
        charController= GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float deltaX = Input.GetAxis("Horizontal") * speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = Vector3.ClampMagnitude(movement, speed);
        movement.y = gravity;
        movement *= Time.deltaTime;

        // Move traži globalni vektor pomaka, a mi trenutno imamo lokalni.
        movement = transform.TransformDirection(movement);

        charController.Move(movement);
    }
}

public class DogController : MonoBehaviour
{
    public float dogSpeed = 10f;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))  // 1 is for right mouse button
        {
            // We create a ray from the camera through the mouse cursor
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Then we check if that ray hits anything in the world
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                // If it did, that point is our new target
                targetPosition = hitInfo.point;
            }
        }

        // Calculate the direction and move towards the target
        Vector3 direction = targetPosition - transform.position;
        if (direction.magnitude > 0.1f)  // If we're not already there
        {
            transform.position += direction.normalized * dogSpeed * Time.deltaTime;
        }
    }
}

public class BridgeController : MonoBehaviour
{
    public float loweringSpeed = 15f;  // the speed of lowering
    public float loweringAngle = 90f;  // the angle to lower

    private float targetRotation;
    private bool isLowering = false;

    void Start()
    {
        targetRotation = transform.eulerAngles.x;
    }

    void Update()
    {
        // If the bridge is lowering, smoothly rotate the bridge towards the target rotation
        if (isLowering)
        {
            float x = Mathf.MoveTowardsAngle(transform.eulerAngles.x, targetRotation, loweringSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(x, transform.eulerAngles.y, transform.eulerAngles.z);

            if (Mathf.Abs(transform.eulerAngles.x - targetRotation) < 0.01f)
            {
                isLowering = false;
            }
        }
    }

    public void LowerBridge()
    {
        targetRotation += loweringAngle;
        isLowering = true;
    }
}

// l system

using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public class Lsystem : MonoBehaviour
{
    // Početni niz simbola
    public string Axiom;
    // Produkcijska pravila zapisana stringovima; pravilo a->abc se zapisuje kao aabc
    public List<string> Rules = new();
    // Broj iteracija (koliko puta će se pravila primijeniti na niz simbola)
    [Range(0, 5)]
    public int Iterations;
            
    // Trenutni niz simbola nakon primijenjenih pravila
    public string Current { get; private set; }
    // Interna reprezentacija produkcijskih pravila
    Dictionary<char, string> _rules = new ();
    
    void Awake()
    {
        Current = Axiom;
        foreach (var rule in Rules)
        {
            _rules[rule[0]] = rule.Substring(1);
        }
        
        for (int i = 0; i < Iterations; i++)
        {
            Current = ApplyRules(Current);
            Debug.Log(Current);
        }
    }
   
    string ApplyRules(string str)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char x in str)
        {
            if (_rules.ContainsKey(x))
            {
                sb.Append(_rules[x]);
            } else { 
                sb.Append(x);
            }
        }
        return sb.ToString();
    }

    // Iscrta praznu kocku u Scene viewu da se lakše vidi objekt
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
    }

}

/*
 Klasa za iscrtavanje osnovne varijante L-sustava pomoću linija.
 Simboli: 
          F naprijed
          + okret ulijevo
          - okret udesno
 Parametri:
          Angle: kut za koji se kornjača okrene kod simbola + i -
          ForwardDistance: put koji kornjača prijeđe kod simbola F
 */
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Lsystem))]
public class LSystemRenderer : MonoBehaviour
{
    [Range(-120f, 120f)]
    public float Angle = 90;
    public float ForwardDistance = 1;

    LineRenderer lr;
    Lsystem ls;
    Vector3 _currentPosition = Vector3.zero;
    float _currentAngle = 90.0f;
    List<Vector3> _points = new();
    void Start()
    {
        _currentPosition = transform.position;
        lr = GetComponent<LineRenderer>();
        ls = GetComponent<Lsystem>();
        Draw();
    }
    public void Draw()
    {
        _points.Add(_currentPosition);
        foreach (char c in ls.Current)
        {
            if (c == 'F')
            {
                _currentPosition.x += Mathf.Cos(Mathf.Deg2Rad * _currentAngle)*ForwardDistance;
                _currentPosition.y += Mathf.Sin(Mathf.Deg2Rad * _currentAngle)*ForwardDistance;
                _points.Add(_currentPosition);
                
            }
            if (c == '+')
            {
                _currentAngle += Angle;
            }
            if (c == '-')
            {
                _currentAngle -= Angle;
            }
        }
        lr.positionCount = _points.Count;
        lr.SetPositions(_points.ToArray());
    }    
}


 Klasa za iscrtavanje osnovne varijante L-sustava pomoću linija.
 Simboli: 
           F naprijed
           + okret ulijevo
           - okret udesno
           [ gurni stanje na stog (push)
           ] uzmi stanje sa stoga (pop)
 Parametri:
           Angle: kut za koji se kornjača okrene kod simbola + i -
           ForwardDistance: put koji kornjača prijeđe kod simbola F
           LineMaterial: materijal kojim se crta linija
           LineWidth: debljina linije

 Komentar: Unityjev LineRenderer uvijek crta neprekinutu liniju. Kako sada
           možemo imati i prekinute linije jer se možemo vratiti na neke
           prethodne pozicije, za svaki segment instanciramo svoj LineRenderer
 */
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Lsystem))]
public class LSystemRendererLines : MonoBehaviour
{
    [Range(-120f, 120f)]
    public float Angle = 90;
    public float ForwardDistance = 1;
    public Material LineMaterial;
    [Range(0.01f, 0.5f)]
    public float LineWidth = 0.1f;

    Lsystem ls;
    LineRenderer lr;
    // Stog stanja. Stanje čini par (tuple) pozicije (Vector3) i kuta (float)
    Stack<Tuple<Vector3, float>> stateStack = new();
    
    Vector3 _currentPosition = Vector3.zero;
    float _currentAngle = 90.0f;
    List<Vector3> _points = new();
    List<LineRenderer> _lineSegments = new();

    void Start()
    {
        _currentPosition = transform.position;
        ls = GetComponent<Lsystem>();
        // stvori novi LineRenderer za prvi segment
        lr = NewSegment();
        Draw();
    }

    LineRenderer NewSegment()
    {
        LineRenderer lr = new GameObject().AddComponent<LineRenderer>();
        lr.transform.SetParent(transform);
        lr.material = LineMaterial;
        lr.startWidth = LineWidth;
        lr.endWidth = LineWidth;
        _lineSegments.Add(lr);
        return lr;
    }

    public void Draw()
    {
        _points.Add(_currentPosition);
        foreach (char c in ls.Current)
        {
            if (c == 'F')
            {
                _currentPosition.x += Mathf.Cos(Mathf.Deg2Rad * _currentAngle)*ForwardDistance;
                _currentPosition.y += Mathf.Sin(Mathf.Deg2Rad * _currentAngle)*ForwardDistance;
                _points.Add(_currentPosition);

            }
            if (c == '+')
            {
                _currentAngle += Angle;
            }
            if (c == '-')
            {
                _currentAngle -= Angle;
            }
            if (c == '[')
            {
                stateStack.Push(new Tuple<Vector3, float>(_currentPosition, _currentAngle));
            }
            if (c == ']')
            {
//                _points.Add(_currentPosition);
                // Iscrtaj dosadašnji segment linije
                lr.positionCount = _points.Count;
                lr.SetPositions(_points.ToArray());
                
                // Dohvati poziciju i kut sa stoga
                (_currentPosition, _currentAngle) = stateStack.Pop();
                // Inicijaliziraj novi objekt i njegov LineRenderer za
                // sljedeći segment.
                lr = NewSegment();

                // Započni novi segment linije
                _points.Clear();
                _points.Add(_currentPosition);
            }
        }
        lr.positionCount = _points.Count;
        lr.SetPositions(_points.ToArray());
    }
}
