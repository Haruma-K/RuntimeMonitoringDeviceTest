using System;
using Baracuda.Monitoring;
using RuntimeMonitoringDeviceTest.Scripts;
using UnityEngine;
using UnityEngine.UI;

public sealed class Player : MonoBehaviour
{
    [SerializeField] private ClickingDetector _leftArrowClickingDetector;
    [SerializeField] private ClickingDetector _rightArrowClickingDetector;
    [SerializeField] private ClickingDetector _upArrowClickingDetector;
    [SerializeField] private Button _jumpButton;

    [SerializeField] private float _speed = 0.1f;
    [SerializeField] private float _rotateSpeed = 5.0f;
    [SerializeField] private float _jumpPower = 300.0f;

    [Monitor] private Vector3 _direction = Vector3.forward;

    private Rigidbody _rigidbody;

    [Monitor] private State _state = State.Idle;

    [Monitor]
    [MUpdateEvent(nameof(Moved))]
    private Vector3 Position => transform.position;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _jumpButton.onClick.AddListener(OnJumpButtonClicked);
        this.RegisterMonitor();
    }

    private void Update()
    {
        if (_leftArrowClickingDetector.IsClicking && _state != State.Jump)
        {
            _direction = Quaternion.AngleAxis(-_rotateSpeed, Vector3.up) * _direction;
            transform.forward = _direction;
        }

        if (_rightArrowClickingDetector.IsClicking && _state != State.Jump)
        {
            _direction = Quaternion.AngleAxis(_rotateSpeed, Vector3.up) * _direction;
            transform.forward = _direction;
        }

        if (_upArrowClickingDetector.IsClicking && _state != State.Jump)
        {
            transform.position += _direction * _speed;
            _state = State.Move;
            Moved?.Invoke();
            return;
        }

        if (_state != State.Jump)
            _state = State.Idle;
    }

    private void OnDestroy()
    {
        this.UnregisterMonitor();
        _jumpButton.onClick.RemoveListener(OnJumpButtonClicked);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_state == State.Jump)
            if (other.gameObject.CompareTag("Ground"))
                _state = State.Idle;
    }

    private void OnJumpButtonClicked()
    {
        if (_state == State.Jump)
            return;

        _rigidbody.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
        _state = State.Jump;
        Jumped?.Invoke();
    }

    private event Action Moved;

    [Monitor] public event Action Jumped;

    private enum State
    {
        Idle,
        Move,
        Jump
    }
}
