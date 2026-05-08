namespace CodeBlue
{
    public class StateMachine<T>
    {
        private IState<T> _currentState;
        private IState<T> _prevState;

        public IState<T> CurrentState => _currentState;

        public void SetState(IState<T> state, T owner)
        {
            if (_currentState != null)
            {
                _currentState.OnExit(owner);
                _prevState = _currentState;
            }

            _currentState = state;
            _currentState.OnEnter(owner);
        }

        public void UpdateState(T owner)
        {
            _currentState?.OnExecute(owner);
        }
    }
}
