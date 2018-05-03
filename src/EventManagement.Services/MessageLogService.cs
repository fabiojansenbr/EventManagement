﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using losol.EventManagement.Domain;
using losol.EventManagement.Infrastructure;
using losol.EventManagement.Services.PowerOffice;
using Microsoft.EntityFrameworkCore;
using static losol.EventManagement.Domain.Order;

namespace losol.EventManagement.Services {
	public class MessageLogService : IMessageLogService {
		private readonly ApplicationDbContext _db;

		public MessageLogService (ApplicationDbContext db) {
			_db = db;
		}

		public async Task<bool> AddAsync(int eventinfoId, string recipients, string messageContent, string messageType, string provider = "", string result = "") {
			var entry = new MessageLog() {
				EventInfoId = eventinfoId, 
				Recipients = recipients, 
				MessageContent = messageContent,
				MessageType = messageType,
				Provider = provider,
				Result = result
			};
			_db.MessageLogs.Add(entry);
			return await _db.SaveChangesAsync() > 0;
		}

		public async Task<List<MessageLog>> Get(int eventInfoId) {
			return await _db.MessageLogs
				.Where( m => m.EventInfoId == eventInfoId)
				.OrderByDescending ( m => m.TimeStamp)
				.ToListAsync();
		}
	}
}