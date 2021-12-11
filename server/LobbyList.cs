using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class LobbyList
    {
        public List<LobbyRoom> lobbyList;

        public LobbyList() { 
            lobbyList = new List<LobbyRoom>();
            addNewLobby(Guid.NewGuid().ToString(), "mainRoom");
        }

        //add new room
        public void addNewLobby(string name, string guid)
        {
            LobbyRoom lobby = new LobbyRoom(guid, name);
            lobbyList.Add(lobby);
        }

        //remove room
        public void RemoveLobby(LobbyRoom lobby)
        {
            lobbyList.Remove(lobby);
        }

        //change room
        public void moveToRoom(ClientInfo client, string newRoom)
        {
            client.lroom.RemoveClient(client.guid);
            foreach(LobbyRoom lobbyRoom in lobbyList)
            {
                if(lobbyRoom.lobbyName == newRoom) lobbyRoom.AddNewClient(client);
            }
        }

        //get listRooms
        public List<string> getListRooms()
        {
            List <string> list = new List<string>();
            foreach(LobbyRoom room in lobbyList)
            {
                list.Add(room.lobbyName);
            }
            return list;
        }
    }
}
