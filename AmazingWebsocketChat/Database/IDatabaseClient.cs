using System;
namespace AmazingWebsocketChat.Database
{
	public interface IDatabaseClient
	{
        public Task<List<ChatResponse>> getChats();
        public Task insertChat(ChatResponse? chatResponse);

    }
}

