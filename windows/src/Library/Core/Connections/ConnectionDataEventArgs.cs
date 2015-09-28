using System;

namespace Communicate
{
    public class ConnectionDataEventArgs : EventArgs
    {
        public ConnectionDataEventArgs()
        {
        }

        public ConnectionDataEventArgs(CommunicationData data, DataComponent dataComponent, ActionState actionState, float progress)
        {
            Data = data;
            DataComponent = dataComponent;
            ActionState = actionState;
            Progress = progress;
        }

        public CommunicationData Data { get; }
        public DataComponent DataComponent { get; } = DataComponent.None;
        public ActionState ActionState { get; } = ActionState.None;
        public float Progress { get; } = 1;
    }

    public class ConnectionEventArgs : ConnectionDataEventArgs
    {
        public ConnectionEventArgs(Connection connection, CommunicationData data, DataComponent dataComponent, ActionState actionState, float progress) : base(data, dataComponent, actionState, progress)
        {
            Connection = connection;
        }

        public ConnectionEventArgs(Connection connection)
        {
            Connection = connection;
        }

        public Connection Connection { get; }
    }
}
