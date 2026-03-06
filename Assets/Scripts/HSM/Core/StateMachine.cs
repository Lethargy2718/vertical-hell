using System.Collections.Generic;

public class StateMachine {
    public readonly State Root;
    public readonly TransitionSequencer Sequencer;
    bool started;

    public StateMachine(State root) {
        Root = root;
        Sequencer = new TransitionSequencer(this);
    }

    public void Start() {
        if (started) return;
            
        started = true;
        Root.Enter();
    }

    public void Tick(float deltaTime)
    {
        if (!started) Start();
        Sequencer.Tick(deltaTime);
    }
    public void FixedTick(float fixedDeltaTime)
    {
        if (!started) return;
        Root.FixedUpdate(fixedDeltaTime);
    }

    internal void InternalTick(float deltaTime) => Root.Update(deltaTime);

    // Perform the actual switch from 'from' to 'to' by exiting up to the shared ancestor, then entering down to the target.
    public void ChangeState(State from, State to, bool fromLeaf = true)
    {
        if (from == to || from == null || to == null) return;

        State origin = fromLeaf ? from.Leaf() : from;
        State lca = TransitionSequencer.Lca(origin, to);

        for (State s = origin; s != lca; s = s.Parent) s.Exit();

        var stack = new Stack<State>();
        for (State s = to; s != lca; s = s.Parent) stack.Push(s);
        while (stack.Count > 0) stack.Pop().Enter();
    }
}