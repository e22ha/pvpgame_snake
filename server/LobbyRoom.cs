using System.Collections.Generic;

namespace server
{
    public class LobbyRoom
    {
        List<ClientInfo> clients;
        public string lobbyName { get; private set; }
        string guid;

        public LobbyRoom(string guid, string name)
        {
            clients = new List<ClientInfo>();
            this.guid = guid;
            this.lobbyName = name;
        }

        //add new clinet
        public void AddNewClient(ClientInfo client)
        {
            client.lroom = this;
            clients.Add(client);
        }

        //remove clinet by guid
        public bool RemoveClient(string guid)
        {
            foreach (ClientInfo client in clients)
            {
                if (client.guid == guid)
                {
                    clients.Remove(client);
                    return true;
                }
            }
            return false;
        }

        //get listClients
        public List<string> getListClientsName()
        {
            List<string> list = new List<string>();
            foreach (ClientInfo client in clients)
            {
                list.Add(client.name);
            }

            return list;
        }

        public List<ClientInfo> getClients()
        {
            return clients;
        }
    }
}