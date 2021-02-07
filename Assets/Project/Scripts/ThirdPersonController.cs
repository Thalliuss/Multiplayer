using Mirror;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : NetworkBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Animator _animator;

    [SerializeField] private float _walkSpeed = 0;
    [SerializeField] private float _jumpSpeed = 0;
    [SerializeField] private float _force = 0;
    [SerializeField] private float _gravity = 0;

    private bool _jump;
    private Vector2 _input;
    private Vector3 _direction = Vector3.zero;
    private CharacterController _controller;
    private CollisionFlags _flags;
    private bool rotating = false;

    private void Start()
    {
        if (!isLocalPlayer) return;

        _controller = GetComponent<CharacterController>();
        _camera.gameObject.SetActive(true);

        Physics.IgnoreLayerCollision(8, 8);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (!_jump && _controller.isGrounded) 
            _jump = Input.GetKeyDown(KeyCode.Space);
       
        if (Input.GetKey(KeyCode.Mouse1) || rotating)
            StartCoroutine(Rotate());
    }

    private IEnumerator Rotate() 
    {
        rotating = true;

        transform.rotation = Quaternion.Euler(0, Mathf.LerpAngle(transform.eulerAngles.y, _camera.transform.eulerAngles.y, 20 * Time.deltaTime), 0);

        yield return new WaitUntil(() => Quaternion.Angle(Quaternion.Euler(0, transform.eulerAngles.y, 0), Quaternion.Euler(0, _camera.transform.eulerAngles.y, 0)) <= .1f);

        rotating = false;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        float t_speed;
        GetInput(out t_speed);

        Vector3 t_move = transform.forward * _input.y + transform.right * _input.x;

        Physics.SphereCast(transform.position, _controller.radius, Vector3.down, out RaycastHit t_hitInfo, _controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Collide);
        t_move = Vector3.ProjectOnPlane(t_move, t_hitInfo.normal).normalized;
        
        _direction.x = t_move.x * t_speed;
        _direction.z = t_move.z * t_speed;

        _animator.SetBool("Jump", !_controller.isGrounded);

        if (_controller.isGrounded)
        {
            _direction.y = -_force;

            if (_jump)
            {
                _direction.y = _jumpSpeed;
                _jump = false;
            }
        }
        else _direction += Physics.gravity * _gravity * Time.fixedDeltaTime;

        _flags = _controller.Move(_direction * Time.fixedDeltaTime);
    }

    private void GetInput(out float p_speed)
    {
        float t_horizontal = Input.GetAxisRaw("Horizontal");
        float t_vertical = Input.GetAxisRaw("Vertical");

        _animator.SetFloat("Horizontal", t_horizontal);
        _animator.SetFloat("Vertical", t_vertical);

        p_speed = _walkSpeed;
        _input = new Vector2(t_horizontal, t_vertical);

        if (_input.sqrMagnitude > .1)
            _input.Normalize();
    }


    private void OnControllerColliderHit(ControllerColliderHit p_hit)
    {
        if (_flags == CollisionFlags.Below) return;

        Rigidbody t_rigidbody = p_hit.collider.attachedRigidbody;

        if (t_rigidbody == null || t_rigidbody.isKinematic) return;

        t_rigidbody.AddForceAtPosition(_controller.velocity * 0.1f, p_hit.point, ForceMode.Impulse);
    } 
}

