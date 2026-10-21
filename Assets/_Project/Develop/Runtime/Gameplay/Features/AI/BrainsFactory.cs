using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using VContainer;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI
{
    public class BrainsFactory
    {
        private readonly IObjectResolver _container;
        private readonly AIBrainsContext _brainsContext;

        public BrainsFactory(IObjectResolver container)
        {
            _container = container;
            _brainsContext = _container.Resolve<AIBrainsContext>();
        }

        public StateMachineBrain CreateBomberBrain(Entity entity)
        {
            AIStateMachine stateMachine = CreateMoveToTargetStateMachine(entity);
            StateMachineBrain brain = new StateMachineBrain(stateMachine);

            _brainsContext.SetFor(entity, brain);

            return brain;
        }

        private AIStateMachine CreateMoveToTargetStateMachine(Entity entity)
        {
            MoveToTargetState moveToTargetState = new MoveToTargetState(entity);

            AIStateMachine stateMachine = new AIStateMachine();
            stateMachine.AddState(moveToTargetState);

            return stateMachine;
        }
    }
}
