using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/State")]
public class State : ScriptableObject
{

    public Action[] actions;
    public Transition[] transitions;
    public Color sceneGizmoColor = Color.grey;

    private bool decisionSucceded;

    public void UpdateState(EnemyStateMachine controller)
    {
        DoAction(controller);
        CheckTransitions(controller);
    }

    private void DoAction(EnemyStateMachine stateController)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].Act(stateController);
        }
    }

    private void CheckTransitions(EnemyStateMachine stateController)
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            decisionSucceded = transitions[i].decision.Decide(stateController);

            if (decisionSucceded)
            {
                stateController.TransitionToState(transitions[i].trueState);
            }
            else
            {
                stateController.TransitionToState(transitions[i].falseState);
            }
        }
    }
}
