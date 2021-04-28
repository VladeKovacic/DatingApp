using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.API.DTOs;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnectionAsync(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnectionAsync(string connectionId)
        {
            return await _context.Groups
              .Include(c => c.Connections)
              .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
              .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessageAsync(int id)
        {
            return await _context.Messages
                            .Include(s => s.Sender)
                            .Include(r => r.Recipient)
                            .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Group> GetMessageGroupAsync(string groupName)
        {
            return await _context.Groups.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams)
        {
            var query = _context.Messages
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
            };

            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
                                .Where(m => 
                                    m.Recipient.UserName == currentUsername
                                    && m.Sender.UserName == recipientUsername
                                    && m.RecipientDeleted == false
                                    || m.Recipient.UserName == recipientUsername
                                    && m.Sender.UserName == currentUsername
                                    && m.SenderDeleted == false
                                )
                                .OrderBy(m => m.MessageSent)
                                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                                .ToListAsync();
            var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername).ToList();

            if(unreadMessages.Any())
            {
                foreach(var message in unreadMessages) 
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}