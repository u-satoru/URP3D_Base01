using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Commands.Definitions;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Player.Commands
{
    public class MoveCommand : ICommand
    {
        private readonly DetailedPlayerStateMachine _stateMachine;
        private readonly Vector2 _direction;

        public bool CanUndo => false;

        public MoveCommand(DetailedPlayerStateMachine stateMachine, MoveCommandDefinition definition)
        {
            _stateMachine = stateMachine;
            _direction = new Vector2(definition.direction.x, definition.direction.z);
        }

        public void Execute()
        {
            // StateMachineに移動入力を渡す
            _stateMachine.HandleInput(_direction, false);
        }

        public void Undo()
        {
            // MoveコマンドのUndoは通常何もしないか、反対方向に移動するなど
            // ここでは何もしない
        }
    }
}

