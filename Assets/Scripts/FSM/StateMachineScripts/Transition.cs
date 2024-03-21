using System;

namespace MyFSM
{
    public class Transition <T>
    {
		public event Action<T> OnTransition = delegate { };

		T input;
		public T Input { get { return input; } }
		State<T> targetState;
		public State<T> TargetState { get { return targetState; } }

		public void OnTransitionExecute(T input)
		{
			OnTransition(input);
		}

		public Transition(T input, State<T> targetState)
		{
			this.input = input;
			this.targetState = targetState;
		}
	
	}
}

