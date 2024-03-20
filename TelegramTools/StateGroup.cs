namespace TelegramTools
{
    public enum State { Start, Finish }
    public class StateGroup
    {
        public State? CurrentState
        {
            get; private set;
        } = null;
        State FirstState => (State)State.GetValues(typeof(State)).Cast<int>().Min();
        State LastState => (State)State.GetValues(typeof(State)).Cast<int>().Max();

        public void Finish()
        {
            CurrentState = null;
        }
        public void Next()
        {
            if (CurrentState is null)
            {
                CurrentState = FirstState;
            }
            else if (CurrentState == LastState)
            {
                this.Finish();
            }
            else
            {
                CurrentState = (State)(CurrentState.Value + 1);
            }
        }

        public void Previous()
        {
            if (CurrentState is not null)
            {
                if (CurrentState == FirstState)
                {
                    this.Finish();
                }
                else
                {
                    CurrentState = (State)(CurrentState.Value - 1);
                }
            }
        }
    }
}
