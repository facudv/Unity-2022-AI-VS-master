using System;
using System.Collections.Generic;

namespace MyFSM
{
    public class State<T>
    {
        private string stateName;
        
        public string Name { get { return stateName; } }

        private Dictionary<T, Transition<T>> transitions;

        public event Action<T> OnEnter = delegate { };
        public event Action OnUpdate = delegate { };
        public event Action<T> OnExit = delegate { };

        public State(string name)
        {
            stateName = name;
        }

        public State<T> Configure(Dictionary<T, Transition<T>> transitions)
        {
            this.transitions = transitions;
            return this;
        }

        public Transition<T> GetTransition(T input)
        {
            return transitions[input];
        }

        public bool CheckInput(T input, out State<T> next)
        {
            if(transitions.ContainsKey(input))
            {
                Transition<T> transition = transitions[input];
                transition.OnTransitionExecute(input);
                next = transition.TargetState;
                return true;
            }

            next = this;
            return false;
        }

        public void Enter(T input)
        {
            OnEnter(input);
        }

        public void Update()
        {
            OnUpdate();
        }

        public void Exit(T input)
        {
            OnExit(input);
        }
    }
}


