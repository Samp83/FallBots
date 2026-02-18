using System.Linq;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [System.Serializable]
    private class References
    {
        public Animator Anim;
        public Player Player;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (_references.Anim.IsInTransition(0))
            return;
        AnimatorStateInfo currentstate = _references.Anim.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextstate = _references.Anim.GetNextAnimatorStateInfo(0);
        PlayerAnimationStateMapper currentStateMapper = _playerAnimationStateMapper.FirstOrDefault(m => m.PlayerState == _references.Player.State.CurrentState);
        if (currentStateMapper != null &&
            !currentstate.IsName(currentStateMapper.AnimatorState) && 
            !nextstate.IsName(currentStateMapper.AnimatorState) && 
            !currentstate.IsName(currentStateMapper.BlockingState))
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
