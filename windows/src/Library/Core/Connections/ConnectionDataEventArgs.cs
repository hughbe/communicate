using System;

namespace Communicate
{
    public class ConnectionDataEventArgs : EventArgs
    {
        public ConnectionDataEventArgs()
        {
        }

        public ConnectionDataEventArgs(CommunicationData data, DataComponent component, ActionState state, float progress)
        {
            Data = data;
            Component = component;
            State = state;
            Progress = progress;
        }

        public CommunicationData Data { get; }
        public DataComponent Component { get; } = DataComponent.None;
        public ActionState State { get; } = ActionState.None;
        public float Progress { get; } = 1;
    }

    public class ConnectionEventArgs : ConnectionDataEventArgs
    {
        public ConnectionEventArgs(Connection connection, CommunicationData data, DataComponent component, ActionState state, float progress) : base(data, component, state, progress)
        {
            ActiveConnection = connection;
        }

        public ConnectionEventArgs(Connection connection)
        {
            ActiveConnection = connection;
        }

        public Connection ActiveConnection { get; }
    }
}
