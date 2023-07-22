using System;
using Microsoft.Data.SqlClient;

namespace AmazingWebsocketChat.Database
{
    public class SQLDatabaseClient : IDatabaseClient
    {
        private SQLDatabaseClient()
        {
            this.connectionString = Secrets.SQL_CONN_STRING;
        }

        private string connectionString;
        private static SQLDatabaseClient sQLDatabaseClientInstance = null;
        private static readonly object lock_object = new object();

        public static SQLDatabaseClient getInstance()
        {
            if(sQLDatabaseClientInstance == null)
            {
                lock(lock_object)
                {
                    if(sQLDatabaseClientInstance == null)
                    {
                        sQLDatabaseClientInstance = new SQLDatabaseClient();
                    }
                }
            }

            return sQLDatabaseClientInstance;
        }
        

        public async Task<List<ChatResponse>> getChats()
        {
            List<ChatResponse> chats = new List<ChatResponse>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand(
                    "SELECT TOP 100 Sender, ChatMessage, Convert(int, SentTime) from Chats order by SentTime desc",
                    connection
                    );

                using SqlDataReader reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        chats.Add(new ChatResponse()
                        {
                            Sender = reader.GetString(0),
                            ChatMessage = reader.GetString(1),
                            SentTime = reader.GetInt32(2)
                        });

                        //byte[] buffer = new byte[8];
                        //reader.GetBytes(2, 0, buffer, 0, buffer.Length);
                    }
                }
            }


            return chats;
        }

        public async Task insertChat(ChatResponse? chatResponse)
        {
            if (chatResponse == null) return;

            List<ChatResponse> chats = new List<ChatResponse>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    $"INSERT INTO Chats (Sender, ChatMessage) VALUES ('{chatResponse.Sender}', '{chatResponse.ChatMessage}')",
                    connection
                );

                using SqlDataReader reader = await command.ExecuteReaderAsync();
            }
            return;
        }



    }
}

