using System.Linq;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [System.Serializable]
    private class References
    {
        public Player Player;
        public Animator Anim;
    }

    [System.Serializable]
    private class PlayerAnimationStateMapper
    {
        public Player.PlayerState PlayerState;
        public string AnimatorState;
        public string BlockingState;
        public string Trigger;
    }

    [SerializeField]
    private References _references;

    [SerializeField]
    private PlayerAnimationStateMapper[] _playerAnimationStateMapper;


    void Start()
    {
        
    }

    void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (_references.Anim.IsInTransition(0))
            return;

        AnimatorStateInfo currentState = _references.Anim.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextState = _references.Anim.GetNextAnimatorStateInfo(0);

        PlayerAnimationStateMapper currentStateMapper = _playerAnimationStateMapper.FirstOrDefault(m => m.PlayerState == _references.Player.State.CurrentState);
    
        if (currentStateMapper != null 
            && !currentState.IsName(currentStateMapper.AnimatorState) 
            && !nextState.IsName(currentStateMapper.AnimatorState) 
            && !currentState.IsName(currentStateMapper.BlockingState))
        {
            _references.Anim.SetTrigger(currentStateMapper.Trigger);
        }
        
        switch(_references.Player.State.CurrentState)
        {
            case Player.PlayerState.Idle:
            case Player.PlayerState.Moving:
                _references.Anim.SetFloat("move", _references.Player.State.HorizontalVelocity.magnitude);
                break;
        }
    }
}
