namespace Communicate
{
    public class ConnectionDataEventArgs
    {
        public ConnectionDataEventArgs()
        {
        }

        public ConnectionDataEventArgs(Data data, DataComponent dataComponent, ActionState actionState, float progress)
        {
            Data = data;
            DataComponent = dataComponent;
            ActionState = actionState;
            Progress = progress;
        }

        public Data Data { get; }
        public DataComponent DataComponent { get; } = DataComponent.None;
        public ActionState ActionState { get; } = ActionState.None;
        public float Progress { get; } = 1;
    }

    public class ConnectionEventArgs<TConnection, TTxtRecords> : ConnectionDataEventArgs where TConnection : BaseConnection<TTxtRecords>, new() where TTxtRecords : BaseTxtRecords
    {
        public ConnectionEventArgs(TConnection connection, Data data, DataComponent dataComponent, ActionState actionState, float progress) : base(data, dataComponent, actionState, progress)
        {
            Connection = connection;
        }

        public ConnectionEventArgs(TConnection connection)
        {
            Connection = connection;
        }

        public TConnection Connection { get; }
    }
}
