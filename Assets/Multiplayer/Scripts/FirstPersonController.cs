using Mirror;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : NetworkBehaviour
{
    [SerializeField] private bool m_IsWalking = false;
    [SerializeField] private float m_WalkSpeed = 0;
    [SerializeField] private float m_RunSpeed = 0;
    [SerializeField] private float m_JumpSpeed = 0;
    [SerializeField] private float m_StickToGroundForce = 0;
    [SerializeField] private float m_GravityMultiplier = 0;

    [SerializeField] private Camera _camera;
    private bool m_Jump;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;

    // Use this for initialization
    private void Start()
    {
        if (!isLocalPlayer) return;

        m_CharacterController = GetComponent<CharacterController>();
        _camera.gameObject.SetActive(true);
    }


    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer) return;

        if (Time.timeScale != 0)
            LookRotation();

        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump && m_CharacterController.isGrounded)
        {
            m_Jump = Input.GetButtonDown("Jump");
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                m_Jump = false;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

    }

    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // set the desired speed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }
    }

    public void LookRotation()
    {
        if (!Input.GetKey(KeyCode.Mouse1)) 
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        float yRot = Input.GetAxis("Mouse X") * 3;
        float xRot = Input.GetAxis("Mouse Y") * 3;

        transform.rotation *= Quaternion.Euler(0f, yRot, 0f);
        Camera.main.transform.rotation *= Quaternion.Euler(-xRot, 0f, 0f);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}

