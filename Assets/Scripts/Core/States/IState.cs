namespace CodeBlue
{
    public interface IState<T>
    {
        /// <summary>
        /// Runs once the state is initialized/changed
        /// </summary>
        void OnEnter(T owner);

        /// <summary>
        /// Runs every frame inside Update()
        /// </summary>
        void OnExecute(T owner);

        /// <summary>
        /// Runs during state change, before state is actually changed
        /// </summary>
        void OnExit(T owner);
    }
}
