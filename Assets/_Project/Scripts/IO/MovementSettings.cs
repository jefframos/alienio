using UnityEngine;

namespace ZombieGame.Player
{
    [CreateAssetMenu(fileName = "MovementSettings", menuName = "Player/MovementSettings")]
    public class MovementSettings : ScriptableObject
    {
        public float MinSpeed => _minSpeed;
        public float MaxSpeed => _maxSpeed;

        [SerializeField]
        private float _minSpeed = 2;

        [SerializeField]
        private float _maxSpeed = 10;
    }
}
