using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Communicate.Server
{
    /// <summary>
    /// A class that holds a list of clients connected to the server
    /// </summary>
    public class ConnectedClientsCollection : IEnumerable<ConnectedClient>, IDisposable
    {
        /// <summary>
        /// The internal collection of connected clients
        /// </summary>
        private Collection<ConnectedClient> _connectedClients;

        /// <summary>
        /// Implements the IEnumberable interface
        /// </summary>
        /// <returns>The enumerator for connected clients</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        /// <summary>
        /// Implements the IEnumberable interface
        /// </summary>
        /// <returns>The enumerator for connected clients</returns>
        public ClientEnumerator GetEnumerator()
        {
            return new ClientEnumerator(_connectedClients);
        }

        /// <summary>
        /// The empty constructor for the connected clients object
        /// </summary>
        public ConnectedClientsCollection()
        {
            _connectedClients = new Collection<ConnectedClient>();
        }

        /// <summary>
        /// Implements indexed subscripting
        /// </summary>
        /// <param name="i">The index of the object to return</param>
        /// <returns>The object at the given index</returns>
        public ConnectedClient this[int index]
        {
            get { return _connectedClients[index]; }
            set { _connectedClients[index] = value; }
        }

        public void AddClient(ConnectedClient client)
        {
            if (_connectedClients != null)
            {
                _connectedClients.Add(client);
            }
        }

        #region Ending

        /// <summary>
        /// Disconnects all connected clients from the server
        /// </summary>
        public void DisconnectAll()
        {
            if (_connectedClients != null)
            {
                foreach (ConnectedClient client in _connectedClients)
                {
                    client.Disconnect();
                    client.Dispose();
                }
                _connectedClients.Clear();
            }
        }

        /// <summary>
        /// Disconnects a particular client from the server
        /// </summary>
        /// <param name="client"></param>
        public void DisconnectClient(ConnectedClient client)
        {
            if (_connectedClients != null && client != null)
            {
                client.Disconnect();
                _connectedClients.Remove(client);
            }
        }

        /// <summary>
        /// Dipsposes all connected clients
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dipsposes all objects connected clients
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connectedClients != null)
                {
                    foreach (ConnectedClient client in _connectedClients)
                    {
                        client.Disconnect();
                        client.Dispose();
                    }
                    _connectedClients = null;
                }
            }
        }

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the connected clients in a readable format</returns>
        public override string ToString()
        {
            return "Clients: " + _connectedClients.ToString();
        }

        #endregion

        IEnumerator<ConnectedClient> IEnumerable<ConnectedClient>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// The enumerator for clients connected to the server
    /// </summary>
    public class ClientEnumerator : IEnumerator
    {
        private Collection<ConnectedClient> _connectedClients;

        // Enumerators are positioned before the first element 
        // until the first MoveNext() call. 
        int position = -1;

        /// <summary>
        /// The default constructor for the connected client enumerator
        /// </summary>
        /// <param name="list">The backend list of clients connected t the server to enumerate</param>
        public ClientEnumerator(Collection<ConnectedClient> list)
        {
            _connectedClients = list;
        }

        /// <summary>
        /// Helper function that increments the index for enumeration
        /// </summary>
        /// <returns>Whether the enumerator should continue enumerating</returns>
        public bool MoveNext()
        {
            position++;
            return (position < _connectedClients.Count);
        }

        /// <summary>
        /// Helper function that resets the enumeration
        /// </summary>
        public void Reset()
        {
            position = -1;
        }

        /// <summary>
        /// Gets the current connected client being enumerated
        /// </summary>
        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        /// <summary>
        /// Gets the current connected client being enumerated
        /// </summary>
        public ConnectedClient Current
        {
            get
            {
                try
                {
                    return _connectedClients[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
