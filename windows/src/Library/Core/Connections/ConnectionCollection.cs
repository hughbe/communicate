using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Communicate
{
    public class ConnectionCollection : Collection<Connection>
    {
        public void DisconnectAll(bool disconnectImmediately)
        {
            PerformActionOnAll(connection => connection.Disconnect(disconnectImmediately));
        }

        public void SendToAll(CommunicationData data)
        {
            PerformActionOnAll(connection => connection.Send(data));
        }

        public void PerformActionOnAll(Action<Connection> action)
        {
            foreach (var connection in this.ToList())
            {
                action?.Invoke(connection);
            }
        }

        public new void Add(Connection connection)
        {
            if (!Contains(connection))
            {
                base.Add(connection);
            }
        }

        public new void Remove(Connection connection)
        {
            if (Contains(connection))
            {
                base.Remove(connection);
            }
        }
    }
}