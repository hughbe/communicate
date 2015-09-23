using System;
using System.Collections;
using System.Collections.Generic;

namespace Communicate
{
    public class Connections<TConnection, TTxtRecords> : IEnumerable<TConnection> where TConnection : BaseConnection<TTxtRecords> where TTxtRecords: BaseTxtRecords
    {
        public List<TConnection> AllConnections { get; set; } = new List<TConnection>();

        public IEnumerator<TConnection> GetEnumerator() => AllConnections.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) AllConnections).GetEnumerator();

        public void DisconnectAll(bool disconnectImmediately)
        {
            PerformActionOnAll(connection => connection.Disconnect(disconnectImmediately));
        }

        public void SendToAll(Data data)
        {
            PerformActionOnAll(connection => connection.Send(data));
        }

        public void PerformActionOnAll(Action<TConnection> action)
        {
            foreach (var connection in AllConnections.ToArray())
            {
                action?.Invoke(connection);
            }
        }

        public bool Contains(TConnection connection) => AllConnections.Contains(connection);

        public void Add(TConnection connection)
        {
            if (!Contains(connection))
            {
                AllConnections.Add(connection);
            }
        }

        public void Remove(TConnection connection)
        {
            if (Contains(connection))
            {
                AllConnections.Remove(connection);
            }
        }
    }
}