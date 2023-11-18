using System.Collections;
using UnityEngine;

namespace MatchForThree
{
    [RequireComponent(typeof(Animator))]
    public class Clearable : MonoBehaviour
    {
        [SerializeField] private AnimationClip _clearAnimation;

        private int _clearAnimationHash;
        private float _clearAnimationDuration;
        private WaitForSeconds _waitForSeconds;
        private Animator _animator;

        protected GamePiece Piece;

        public bool IsBeingCleared { get; private set; }
        public AnimationClip ClearAnimation => _clearAnimation;

        private void Awake()
        {
            Piece = GetComponent<GamePiece>();
            _animator = GetComponent<Animator>();
            _clearAnimationDuration = ClearAnimation.length;
            _waitForSeconds = new WaitForSeconds(_clearAnimationDuration);
            _clearAnimationHash = Animator.StringToHash(ClearAnimation.name);
        }

        public virtual void Clear()
        {
            Piece.GameGridRef.Level.OnPieceCleared(Piece);
            IsBeingCleared = true;
            StartCoroutine(ClearCoroutine());
        }

        private IEnumerator ClearCoroutine()
        {
            _animator.Play(_clearAnimationHash);
            yield return _waitForSeconds;
            Destroy(gameObject);
        }
    }
}
