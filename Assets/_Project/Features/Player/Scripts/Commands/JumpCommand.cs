using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Player.States;

namespace asterivo.Unity60.Player.Commands
{
    public class JumpCommand : ICommand
    {
        private readonly DetailedPlayerStateMachine _stateMachine;

        public bool CanUndo => false;

        public JumpCommand(DetailedPlayerStateMachine stateMachine, JumpCommandDefinition definition)
        {
            _stateMachine = stateMachine;
            // definition is unused for now but required for the factory
        }

        public void Execute()
        {
            // StateMachineにジャンプ入力を渡す
            _stateMachine.HandleInput(Vector2.zero, true);
        }

        public void Undo()
        {
            // JumpコマンドのUndoは通常サポートされない
        }
    }
}
