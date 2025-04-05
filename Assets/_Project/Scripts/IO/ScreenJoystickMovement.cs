using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class ScreenJoystickMovement : MonoBehaviour
    {
        public bool IsMoving => MoveInput.magnitude > 0.01f;

        public Vector2 MoveInput => _moveInput;

        [SerializeField]
        private RectTransform _joystickBG;

        [SerializeField]
        private RectTransform _joystickHandle;

        [SerializeField]
        private float _joystickRadius = 1.0f;

        private bool _isTouching;
        private Vector2 _moveInput;
        private Vector2 _touchPosition;

        private PlayerActionInput _inputActions;

        void OnEnable()
        {
            _inputActions = new PlayerActionInput();
            _inputActions.Enable(); // Enable the input actions

            _inputActions.Player.PointerPress.performed += OnPointerBegin;
            _inputActions.Player.PointerPress.canceled += OnPointerEnd;
            _inputActions.Player.PointerPosition.performed += OnPointerDrag;

            ResetJoystick();
        }

        void OnDisable()
        {
            _inputActions.Player.PointerPress.performed -= OnPointerBegin;
            _inputActions.Player.PointerPress.canceled -= OnPointerEnd;
            _inputActions.Player.PointerPosition.performed -= OnPointerDrag;
            _inputActions.Disable();
        }

        void Update()
        {
            // Debug.Log(_isTouching);
        }

        private void OnPointerBegin(InputAction.CallbackContext context)
        {
            _isTouching = true;
        }

        private void OnPointerDrag(InputAction.CallbackContext context)
        {
            if (!_isTouching)
            {
                return;
            }

            if (!_joystickBG.gameObject.activeInHierarchy)
            {
                _touchPosition = context.ReadValue<Vector2>();
                _joystickBG.position = _touchPosition;
                _joystickBG.gameObject.SetActive(true);
                return;
            }

            Vector2 newPosition = context.ReadValue<Vector2>();
            Vector2 direction = Vector2.ClampMagnitude(
                newPosition - _touchPosition,
                _joystickRadius
            );

            _joystickHandle.localPosition = direction;
            _moveInput = direction / _joystickRadius;
        }

        private void OnPointerEnd(InputAction.CallbackContext context)
        {
            _moveInput = Vector2.zero;
            ResetJoystick();
            _isTouching = false;
        }

        private void ResetJoystick()
        {
            _joystickHandle.localPosition = Vector2.zero;
            _joystickBG.gameObject.SetActive(false);
        }
    }
}
