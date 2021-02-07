using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point pos);
    private Point _cursor = new Point();

    [SerializeField] private Transform cameraTarget; 
    [SerializeField] private float maxViewDistance = 25.0f;
    [SerializeField] private float minViewDistance = 1.0f;
    [SerializeField] private int zoomRate = 30;
    [SerializeField] private float cameraTargetHeight = 1.0f;

    private float _x = 0.0f;
    private float _y = 0.0f;

    private float _distance = 3.0f;
    private float _desiredDistance;
    private float _correctedDistance;
    private float _currentDistance;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        _x += angles.x;
        _y -= angles.y;

        _currentDistance = _distance;
        _desiredDistance = _distance;
        _correctedDistance = _distance;
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse0)) GetCursorPos(out _cursor);
        if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse0))
        {
            SetCursorPos(_cursor.X, _cursor.Y);
            _x += Input.GetAxis("Mouse X") * 3;
            _y -= Input.GetAxis("Mouse Y") * 3;

            Cursor.visible = false;
        }
        else Cursor.visible = true;

        _y = ClampAngle(_y, -50, 50);

        Quaternion t_rotation = Quaternion.Euler(_y, _x, 0);


        _desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(_desiredDistance);
        _desiredDistance = Mathf.Clamp(_desiredDistance, minViewDistance, maxViewDistance);
        _correctedDistance = _desiredDistance;

        Vector3 t_position = cameraTarget.position - (t_rotation * Vector3.forward * _desiredDistance);

        bool isCorrected = false;
        RaycastHit collisionHit;
        Vector3 cameraTargetPosition = new Vector3(cameraTarget.position.x, cameraTarget.position.y + cameraTargetHeight, cameraTarget.position.z);
        if (Physics.Linecast(cameraTargetPosition, t_position, out collisionHit)) 
        {
            if (!collisionHit.transform.CompareTag("Player"))
            {
                t_position = collisionHit.point;
                _correctedDistance = Vector3.Distance(cameraTargetPosition, t_position);
                isCorrected = true;
            }
        }

        _currentDistance = !isCorrected || _correctedDistance > _currentDistance ? Mathf.Lerp(_currentDistance, _correctedDistance, Time.deltaTime * zoomRate) : _correctedDistance;

        t_position = cameraTarget.position - (t_rotation * Vector3.forward * _currentDistance + new Vector3(0, -cameraTargetHeight, 0));

        transform.rotation = t_rotation;
        transform.position = t_position;
    }

    private static float ClampAngle(float p_angle, float p_min, float p_max)
    {
        if (p_angle < -360)
            p_angle += 360;

        if (p_angle > 360)
            p_angle -= 360;

        return Mathf.Clamp(p_angle, p_min, p_max);
    }
}