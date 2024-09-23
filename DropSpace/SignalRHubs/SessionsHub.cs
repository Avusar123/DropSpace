﻿using DropSpace.Contracts.Dtos;
using DropSpace.Services.Interfaces;
using DropSpace.Stores.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DropSpace.SignalRHubs
{

    [Authorize]
    public class SessionsHub(
        ISessionService sessionService,
        IFileService fileService,
        IInviteCodeStore inviteCodeStore,
        IConnectionIdStore connectionIdStore) : Hub
    {
        public override async Task OnConnectedAsync()
        {

            if (Context.UserIdentifier == null)
            {
                Context.Abort();

                return;
            }

            await connectionIdStore.SaveConnectionId(Context.UserIdentifier, Context.ConnectionId);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            inviteCodeStore.RemoveUserId(Context.UserIdentifier!);

            connectionIdStore.Remove(Context.UserIdentifier!);

            return Task.CompletedTask;
        }

        public async Task<List<SessionDto>> GetSessions()
        {
            return await sessionService.GetAllSessions(Context.UserIdentifier!);
        }

        public async Task<List<FileModelDto>> GetFiles(Guid sessionId)
        {
            return await fileService.GetAllFiles(sessionId);
        }

        public async Task<List<PendingUploadModelDto>> GetUploads(Guid sessionId)
        {
            return await fileService.GetAllUploads(sessionId);
        }

        public async Task<string> RefreshCode()
        {
            return await inviteCodeStore.RefreshCode(Context.UserIdentifier!);
        }

        public async Task<bool> SendInviteByCode(string code, string sessionId)
        {
            try
            {
                var userId = await inviteCodeStore.GetUserIdByCodeOrNull(code.ToUpper())
                    ?? throw new NullReferenceException("Пользователь не найден!");

                var session = await sessionService.GetAsync(Guid.Parse(sessionId));

                await Clients.User(userId).SendAsync("NewInvite", session.Name, session.Id);

                return true;
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorRecieved", ex.Message);

                return false;
            }
        }
    }
}
