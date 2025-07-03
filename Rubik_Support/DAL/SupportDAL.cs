using Rubik_Support.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Rubik_SDK;
using LogLevel = Rubik_Support.Models.LogLevel;


namespace Rubik_Support.DAL
{
    public class SupportDAL
    {
        private readonly string _connectionString;

        public SupportDAL()
        {
            _connectionString = ConnectionEncryption.Decrypt(GlobalDef.ConnectionString);
        }

        #region Visitor Methods

        public SupportVisitor GetVisitorByMobile(string mobile)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT * FROM Support_Visitors 
                    WHERE Mobile = @Mobile AND IsBlocked = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@Mobile", mobile);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapVisitor(reader);
                        }
                    }
                }
            }
            return null;
        }

        public int CreateVisitor(SupportVisitor visitor)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    INSERT INTO Support_Visitors (Mobile, FirstName, LastName, Email, CreateDate)
                    VALUES (@Mobile, @FirstName, @LastName, @Email, GETDATE());
                    SELECT SCOPE_IDENTITY();", conn))
                {
                    cmd.Parameters.AddWithValue("@Mobile", visitor.Mobile);
                    cmd.Parameters.AddWithValue("@FirstName", visitor.FirstName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastName", visitor.LastName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", visitor.Email ?? (object)DBNull.Value);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateVisitorLastVisit(int visitorId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    UPDATE Support_Visitors 
                    SET LastVisitDate = GETDATE() 
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", visitorId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Ticket Methods

        public string GenerateTicketNumber()
        {
            return $"T{DateTime.Now:yyMMdd}{new Random().Next(1000, 9999)}";
        }

        public int CreateTicket(SupportTicket ticket)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    INSERT INTO Support_Tickets 
                    (TicketNumber, VisitorId, UserId, Subject, Status, CreateDate, IsActive)
                    VALUES 
                    (@TicketNumber, @VisitorId, @UserId, @Subject, @Status, GETDATE(), 1);
                    SELECT SCOPE_IDENTITY();", conn))
                {
                    cmd.Parameters.AddWithValue("@TicketNumber", ticket.TicketNumber);
                    cmd.Parameters.AddWithValue("@VisitorId", ticket.VisitorId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId", ticket.UserId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subject", ticket.Subject ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", (byte)ticket.Status);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public SupportTicket GetTicketById(int ticketId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT t.*, 
                           v.Mobile, v.FirstName, v.LastName,
                           u1.FirstName + ' ' + u1.LastName AS UserFullName,
                           u2.FirstName + ' ' + u2.LastName AS SupportFullName
                    FROM Support_Tickets t
                    LEFT JOIN Support_Visitors v ON t.VisitorId = v.Id
                    LEFT JOIN KCI_Users u1 ON t.UserId = u1.id
                    LEFT JOIN KCI_Users u2 ON t.SupportUserId = u2.id
                    WHERE t.Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", ticketId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapTicket(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<SupportTicket> GetActiveTickets(int? supportUserId = null)
        {
            var tickets = new List<SupportTicket>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var query = @"
                    SELECT t.*, 
                           v.Mobile, v.FirstName, v.LastName,
                           u1.FirstName + ' ' + u1.LastName AS UserFullName,
                           u2.FirstName + ' ' + u2.LastName AS SupportFullName
                    FROM Support_Tickets t
                    LEFT JOIN Support_Visitors v ON t.VisitorId = v.Id
                    LEFT JOIN KCI_Users u1 ON t.UserId = u1.id
                    LEFT JOIN KCI_Users u2 ON t.SupportUserId = u2.id
                    WHERE t.IsActive = 1 AND t.Status != 3";

                if (supportUserId.HasValue)
                {
                    query += " AND (t.SupportUserId = @SupportUserId OR t.SupportUserId IS NULL)";
                }

                query += " ORDER BY t.CreateDate DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (supportUserId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@SupportUserId", supportUserId.Value);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tickets.Add(MapTicket(reader));
                        }
                    }
                }
            }
            return tickets;
        }

        public void UpdateTicketStatus(int ticketId, TicketStatus status, int? supportUserId = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var query = @"
                    UPDATE Support_Tickets 
                    SET Status = @Status, 
                        LastUpdateDate = GETDATE()";

                if (status == TicketStatus.Closed)
                {
                    query += ", CloseDate = GETDATE()";
                }

                if (supportUserId.HasValue)
                {
                    query += ", SupportUserId = @SupportUserId";
                }

                query += " WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", ticketId);
                    cmd.Parameters.AddWithValue("@Status", (byte)status);
                    if (supportUserId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@SupportUserId", supportUserId.Value);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateTicketConnectionId(int ticketId, string connectionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    UPDATE Support_Tickets 
                    SET ConnectionId = @ConnectionId 
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", ticketId);
                    cmd.Parameters.AddWithValue("@ConnectionId", connectionId ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Message Methods

        public int CreateMessage(SupportMessage message)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    INSERT INTO Support_Messages 
                    (TicketId, Message, SenderId, SenderType, CreateDate, IsRead)
                    VALUES 
                    (@TicketId, @Message, @SenderId, @SenderType, GETDATE(), 0);
                    SELECT SCOPE_IDENTITY();", conn))
                {
                    cmd.Parameters.AddWithValue("@TicketId", message.TicketId);
                    cmd.Parameters.AddWithValue("@Message", message.Message);
                    cmd.Parameters.AddWithValue("@SenderId", message.SenderId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SenderType", (byte)message.SenderType);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public List<SupportMessage> GetTicketMessages(int ticketId)
        {
            var messages = new List<SupportMessage>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT m.*, 
                           CASE 
                               WHEN m.SenderType = 1 THEN v.FirstName + ' ' + v.LastName
                               WHEN m.SenderType = 2 THEN u.FirstName + ' ' + u.LastName
                               ELSE 'سیستم'
                           END AS SenderName
                    FROM Support_Messages m
                    LEFT JOIN Support_Tickets t ON m.TicketId = t.Id
                    LEFT JOIN Support_Visitors v ON t.VisitorId = v.Id AND m.SenderType = 1
                    LEFT JOIN KCI_Users u ON m.SenderId = u.id AND m.SenderType = 2
                    WHERE m.TicketId = @TicketId AND m.IsDeleted = 0
                    ORDER BY m.CreateDate ASC", conn))
                {
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(MapMessage(reader));
                        }
                    }
                }
            }

            // Load attachments for each message
            foreach (var message in messages)
            {
                message.Attachments = GetMessageAttachments(message.Id);
            }

            return messages;
        }

        public void MarkMessagesAsRead(int ticketId, int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    UPDATE Support_Messages 
                    SET IsRead = 1, ReadDate = GETDATE() 
                    WHERE TicketId = @TicketId 
                    AND SenderId != @UserId 
                    AND IsRead = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateMessage(int messageId, string newMessage, int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    UPDATE Support_Messages 
                    SET Message = @Message, 
                        EditedDate = GETDATE(), 
                        EditedBy = @EditedBy 
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", messageId);
                    cmd.Parameters.AddWithValue("@Message", newMessage);
                    cmd.Parameters.AddWithValue("@EditedBy", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteMessage(int messageId, int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    UPDATE Support_Messages 
                    SET IsDeleted = 1, 
                        DeletedDate = GETDATE(), 
                        DeletedBy = @DeletedBy 
                    WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", messageId);
                    cmd.Parameters.AddWithValue("@DeletedBy", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Attachment Methods

        public int CreateAttachment(SupportAttachment attachment)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    INSERT INTO Support_Attachments 
                    (MessageId, FileName, FileExtension, FileSize, FilePath, UploadDate)
                    VALUES 
                    (@MessageId, @FileName, @FileExtension, @FileSize, @FilePath, GETDATE());
                    SELECT SCOPE_IDENTITY();", conn))
                {
                    cmd.Parameters.AddWithValue("@MessageId", attachment.MessageId);
                    cmd.Parameters.AddWithValue("@FileName", attachment.FileName);
                    cmd.Parameters.AddWithValue("@FileExtension", attachment.FileExtension);
                    cmd.Parameters.AddWithValue("@FileSize", attachment.FileSize);
                    cmd.Parameters.AddWithValue("@FilePath", attachment.FilePath);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public List<SupportAttachment> GetMessageAttachments(int messageId)
        {
            var attachments = new List<SupportAttachment>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT * FROM Support_Attachments 
                    WHERE MessageId = @MessageId", conn))
                {
                    cmd.Parameters.AddWithValue("@MessageId", messageId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            attachments.Add(MapAttachment(reader));
                        }
                    }
                }
            }
            return attachments;
        }

        #endregion

        #region Settings Methods

        public string GetSetting(string key)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT SettingValue FROM Support_Settings 
                    WHERE SettingKey = @Key", conn))
                {
                    cmd.Parameters.AddWithValue("@Key", key);
                    return cmd.ExecuteScalar()?.ToString();
                }
            }
        }

        public void UpdateSetting(string key, string value)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    UPDATE Support_Settings 
                    SET SettingValue = @Value 
                    WHERE SettingKey = @Key", conn))
                {
                    cmd.Parameters.AddWithValue("@Key", key);
                    cmd.Parameters.AddWithValue("@Value", value ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Dictionary<string, string> GetAllSettings()
        {
            var settings = new Dictionary<string, string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT SettingKey, SettingValue FROM Support_Settings", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            settings[reader["SettingKey"].ToString()] = reader["SettingValue"]?.ToString();
                        }
                    }
                }
            }
            return settings;
        }

        #endregion

        #region Logging Methods

        public void AddLog(LogLevel logLevel, string message, int? userId = null, int? ticketId = null, string ip = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    INSERT INTO Support_Logs 
                    (LogLevel, LogType, Message, UserId, TicketId, IP, CreateDate)
                    VALUES 
                    (@LogLevel, @LogType, @Message, @UserId, @TicketId, @IP, GETDATE())", conn))
                {
                    cmd.Parameters.AddWithValue("@LogLevel", (byte)logLevel);
                    cmd.Parameters.AddWithValue("@LogType", "Support");
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@UserId", userId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TicketId", ticketId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IP", ip ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Helper Methods

        private SupportVisitor MapVisitor(SqlDataReader reader)
        {
            return new SupportVisitor
            {
                Id = Convert.ToInt32(reader["Id"]),
                Mobile = reader["Mobile"].ToString(),
                FirstName = reader["FirstName"]?.ToString(),
                LastName = reader["LastName"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                CreateDate = Convert.ToDateTime(reader["CreateDate"]),
                LastVisitDate = reader["LastVisitDate"] as DateTime?,
                IsBlocked = Convert.ToBoolean(reader["IsBlocked"])
            };
        }

        private SupportTicket MapTicket(SqlDataReader reader)
        {
            var ticket = new SupportTicket
            {
                Id = Convert.ToInt32(reader["Id"]),
                TicketNumber = reader["TicketNumber"].ToString(),
                VisitorId = reader["VisitorId"] as int?,
                UserId = reader["UserId"] as int?,
                SupportUserId = reader["SupportUserId"] as int?,
                Subject = reader["Subject"]?.ToString(),
                Status = (TicketStatus)Convert.ToByte(reader["Status"]),
                CreateDate = Convert.ToDateTime(reader["CreateDate"]),
                LastUpdateDate = reader["LastUpdateDate"] as DateTime?,
                CloseDate = reader["CloseDate"] as DateTime?,
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                ConnectionId = reader["ConnectionId"]?.ToString()
            };

            // Map visitor info if exists
            if (ticket.VisitorId.HasValue && reader.FieldCount > 12)
            {
                ticket.Visitor = new SupportVisitor
                {
                    Id = ticket.VisitorId.Value,
                    Mobile = reader["Mobile"]?.ToString(),
                    FirstName = reader["FirstName"]?.ToString(),
                    LastName = reader["LastName"]?.ToString()
                };
            }

            // Map user names
            try
            {
                ticket.UserFullName = reader["UserFullName"]?.ToString();
                ticket.SupportFullName = reader["SupportFullName"]?.ToString();
            }
            catch { }

            return ticket;
        }

        private SupportMessage MapMessage(SqlDataReader reader)
        {
            return new SupportMessage
            {
                Id = Convert.ToInt32(reader["Id"]),
                TicketId = Convert.ToInt32(reader["TicketId"]),
                Message = reader["Message"].ToString(),
                SenderId = reader["SenderId"] as int?,
                SenderType = (SenderType)Convert.ToByte(reader["SenderType"]),
                CreateDate = Convert.ToDateTime(reader["CreateDate"]),
                IsRead = Convert.ToBoolean(reader["IsRead"]),
                ReadDate = reader["ReadDate"] as DateTime?,
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                DeletedDate = reader["DeletedDate"] as DateTime?,
                DeletedBy = reader["DeletedBy"] as int?,
                EditedDate = reader["EditedDate"] as DateTime?,
                EditedBy = reader["EditedBy"] as int?,
                SenderName = reader["SenderName"]?.ToString()
            };
        }

        private SupportAttachment MapAttachment(SqlDataReader reader)
        {
            return new SupportAttachment
            {
                Id = Convert.ToInt32(reader["Id"]),
                MessageId = Convert.ToInt32(reader["MessageId"]),
                FileName = reader["FileName"].ToString(),
                FileExtension = reader["FileExtension"].ToString(),
                FileSize = Convert.ToInt64(reader["FileSize"]),
                FilePath = reader["FilePath"].ToString(),
                UploadDate = Convert.ToDateTime(reader["UploadDate"])
            };
        }

        #endregion

        #region Support Agent Methods

        public SupportAgent GetAgentByUserId(int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT a.*, 
                   u.FirstName + ' ' + u.LastName AS UserFullName,
                   u.Mobile AS UserMobile,
                   u.Email AS UserEmail
            FROM Support_Agents a
            INNER JOIN KCI_Users u ON a.UserId = u.id
            WHERE a.UserId = @UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapAgent(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<SupportAgent> GetAllAgents(bool? isActive = null, bool? isOnline = null)
        {
            var agents = new List<SupportAgent>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var query = @"
            SELECT a.*, 
                   u.FirstName + ' ' + u.LastName AS UserFullName,
                   u.Mobile AS UserMobile,
                   u.Email AS UserEmail
            FROM Support_Agents a
            INNER JOIN KCI_Users u ON a.UserId = u.id
            WHERE 1=1";

                if (isActive.HasValue)
                    query += " AND a.IsActive = @IsActive";
                if (isOnline.HasValue)
                    query += " AND a.IsOnline = @IsOnline";

                query += " ORDER BY a.Priority, a.CurrentActiveTickets";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (isActive.HasValue)
                        cmd.Parameters.AddWithValue("@IsActive", isActive.Value);
                    if (isOnline.HasValue)
                        cmd.Parameters.AddWithValue("@IsOnline", isOnline.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            agents.Add(MapAgent(reader));
                        }
                    }
                }
            }
            return agents;
        }

        public int CreateAgent(SupportAgent agent)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            INSERT INTO Support_Agents 
            (UserId, IsActive, MaxConcurrentTickets, Specialties, 
             WorkingHours, Priority, Notes)
            VALUES 
            (@UserId, @IsActive, @MaxConcurrentTickets, @Specialties, 
             @WorkingHours, @Priority, @Notes);
            SELECT SCOPE_IDENTITY();", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", agent.UserId);
                    cmd.Parameters.AddWithValue("@IsActive", agent.IsActive);
                    cmd.Parameters.AddWithValue("@MaxConcurrentTickets", agent.MaxConcurrentTickets);
                    cmd.Parameters.AddWithValue("@Specialties", agent.Specialties ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@WorkingHours", agent.WorkingHours ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Priority", agent.Priority);
                    cmd.Parameters.AddWithValue("@Notes", agent.Notes ?? (object)DBNull.Value);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateAgent(SupportAgent agent)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            UPDATE Support_Agents 
            SET IsActive = @IsActive,
                MaxConcurrentTickets = @MaxConcurrentTickets,
                Specialties = @Specialties,
                WorkingHours = @WorkingHours,
                Priority = @Priority,
                Notes = @Notes,
                ModifiedDate = GETDATE()
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", agent.Id);
                    cmd.Parameters.AddWithValue("@IsActive", agent.IsActive);
                    cmd.Parameters.AddWithValue("@MaxConcurrentTickets", agent.MaxConcurrentTickets);
                    cmd.Parameters.AddWithValue("@Specialties", agent.Specialties ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@WorkingHours", agent.WorkingHours ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Priority", agent.Priority);
                    cmd.Parameters.AddWithValue("@Notes", agent.Notes ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateAgentOnlineStatus(int userId, bool isOnline)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            UPDATE Support_Agents 
            SET IsOnline = @IsOnline,
                LastOnlineDate = CASE WHEN @IsOnline = 1 THEN GETDATE() ELSE LastOnlineDate END
            WHERE UserId = @UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@IsOnline", isOnline);
                    cmd.ExecuteNonQuery();
                }
            }

            // Log action
            LogAgentAction(userId, isOnline ? AgentActionType.Login : AgentActionType.Logout);
        }

        public void IncrementAgentTicketCount(int agentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            UPDATE Support_Agents 
            SET CurrentActiveTickets = CurrentActiveTickets + 1
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", agentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DecrementAgentTicketCount(int agentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            UPDATE Support_Agents 
            SET CurrentActiveTickets = CASE 
                WHEN CurrentActiveTickets > 0 THEN CurrentActiveTickets - 1 
                ELSE 0 END,
                TotalHandledTickets = TotalHandledTickets + 1
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", agentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void LogAgentAction(int userId, AgentActionType actionType, string ip = null)
        {
            var agent = GetAgentByUserId(userId);
            if (agent == null) return;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            INSERT INTO Support_AgentLogs (AgentId, ActionType, IP)
            VALUES (@AgentId, @ActionType, @IP)", conn))
                {
                    cmd.Parameters.AddWithValue("@AgentId", agent.Id);
                    cmd.Parameters.AddWithValue("@ActionType", actionType.ToString());
                    cmd.Parameters.AddWithValue("@IP", ip ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public SupportAgent GetBestAvailableAgent(string specialty = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var query = @"
            SELECT TOP 1 a.*, 
                   u.FirstName + ' ' + u.LastName AS UserFullName,
                   u.Mobile AS UserMobile,
                   u.Email AS UserEmail
            FROM Support_Agents a
            INNER JOIN KCI_Users u ON a.UserId = u.id
            WHERE a.IsActive = 1 
                  AND a.IsOnline = 1 
                  AND a.CurrentActiveTickets < a.MaxConcurrentTickets";

                if (!string.IsNullOrEmpty(specialty))
                {
                    query += " AND (a.Specialties LIKE @Specialty OR a.Specialties IS NULL)";
                }

                query += " ORDER BY a.Priority, a.CurrentActiveTickets, a.TotalHandledTickets DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(specialty))
                    {
                        cmd.Parameters.AddWithValue("@Specialty", $"%{specialty}%");
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapAgent(reader);
                        }
                    }
                }
            }
            return null;
        }

        private SupportAgent MapAgent(SqlDataReader reader)
        {
            return new SupportAgent
            {
                Id = Convert.ToInt32(reader["Id"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                IsOnline = Convert.ToBoolean(reader["IsOnline"]),
                LastOnlineDate = reader["LastOnlineDate"] as DateTime?,
                MaxConcurrentTickets = Convert.ToInt32(reader["MaxConcurrentTickets"]),
                CurrentActiveTickets = Convert.ToInt32(reader["CurrentActiveTickets"]),
                Specialties = reader["Specialties"]?.ToString(),
                WorkingHours = reader["WorkingHours"]?.ToString(),
                Priority = Convert.ToInt32(reader["Priority"]),
                TotalHandledTickets = Convert.ToInt32(reader["TotalHandledTickets"]),
                AverageResponseTime = reader["AverageResponseTime"] as int?,
                AverageRating = reader["AverageRating"] as decimal?,
                Notes = reader["Notes"]?.ToString(),
                CreateDate = Convert.ToDateTime(reader["CreateDate"]),
                ModifiedDate = reader["ModifiedDate"] as DateTime?,
                UserFullName = reader["UserFullName"]?.ToString(),
                UserMobile = reader["UserMobile"]?.ToString(),
                UserEmail = reader["UserEmail"]?.ToString()
            };
        }

        #endregion

        #region SMS Queue Methods

        public int AddToSMSQueue(SMSQueue sms)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            INSERT INTO Support_SMSQueue 
            (TicketId, RecipientMobile, Message, ScheduledDate)
            VALUES (@TicketId, @RecipientMobile, @Message, @ScheduledDate);
            SELECT SCOPE_IDENTITY();", conn))
                {
                    cmd.Parameters.AddWithValue("@TicketId", sms.TicketId);
                    cmd.Parameters.AddWithValue("@RecipientMobile", sms.RecipientMobile);
                    cmd.Parameters.AddWithValue("@Message", sms.Message);
                    cmd.Parameters.AddWithValue("@ScheduledDate", sms.ScheduledDate);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void CancelPendingSMS(int ticketId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            UPDATE Support_SMSQueue 
            SET IsCancelled = 1 
            WHERE TicketId = @TicketId AND IsSent = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<SMSQueue> GetPendingSMS()
        {
            var smsList = new List<SMSQueue>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT * FROM Support_SMSQueue 
            WHERE IsSent = 0 AND IsCancelled = 0 
            AND ScheduledDate <= GETDATE() 
            AND RetryCount < 3
            ORDER BY ScheduledDate", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            smsList.Add(MapSMSQueue(reader));
                        }
                    }
                }
            }
            return smsList;
        }

        public void MarkSMSAsSent(int smsId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            UPDATE Support_SMSQueue 
            SET IsSent = 1, SentDate = GETDATE() 
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", smsId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Agent Request Methods

        public int CreateAgentRequest(int ticketId, int agentId, int timeoutSeconds)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            INSERT INTO Support_AgentRequests 
            (TicketId, AgentId, TimeoutDate)
            VALUES (@TicketId, @AgentId, DATEADD(SECOND, @Timeout, GETDATE()));
            SELECT SCOPE_IDENTITY();", conn))
                {
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);
                    cmd.Parameters.AddWithValue("@AgentId", agentId);
                    cmd.Parameters.AddWithValue("@Timeout", timeoutSeconds);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public AgentRequest GetPendingAgentRequest(int ticketId, int agentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT * FROM Support_AgentRequests 
            WHERE TicketId = @TicketId AND AgentId = @AgentId 
            AND ResponseDate IS NULL AND TimeoutDate > GETDATE()", conn))
                {
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);
                    cmd.Parameters.AddWithValue("@AgentId", agentId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapAgentRequest(reader);
                        }
                    }
                }
            }
            return null;
        }

        public void UpdateAgentRequestResponse(int requestId, bool isAccepted)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            UPDATE Support_AgentRequests 
            SET ResponseDate = GETDATE(), IsAccepted = @IsAccepted 
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@IsAccepted", isAccepted);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<AgentRequest> GetExpiredRequests()
        {
            var requests = new List<AgentRequest>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT ar.*, a.UserId 
            FROM Support_AgentRequests ar
            INNER JOIN Support_Agents a ON ar.AgentId = a.Id
            WHERE ar.ResponseDate IS NULL 
            AND ar.TimeoutDate <= GETDATE()", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapAgentRequest(reader));
                        }
                    }
                }
            }
            return requests;
        }

        #endregion

        #region User Limit Methods

        public UserLimit GetUserLimit(string identifier, string identifierType)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT * FROM Support_UserLimits 
            WHERE Identifier = @Identifier 
            AND IdentifierType = @IdentifierType", conn))
                {
                    cmd.Parameters.AddWithValue("@Identifier", identifier);
                    cmd.Parameters.AddWithValue("@IdentifierType", identifierType);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapUserLimit(reader);
                        }
                    }
                }
            }
            return null;
        }

        public void UpdateUserLimit(string identifier, string identifierType)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            MERGE Support_UserLimits AS target
            USING (SELECT @Identifier AS Identifier, @IdentifierType AS IdentifierType) AS source
            ON target.Identifier = source.Identifier AND target.IdentifierType = source.IdentifierType
            WHEN MATCHED THEN
                UPDATE SET 
                    TicketCount = CASE 
                        WHEN DATEDIFF(HOUR, LastTicketDate, GETDATE()) > 24 
                        THEN 1 
                        ELSE TicketCount + 1 
                    END,
                    LastTicketDate = GETDATE(),
                    IsBlocked = CASE 
                        WHEN TicketCount >= 5 AND DATEDIFF(HOUR, LastTicketDate, GETDATE()) <= 1 
                        THEN 1 
                        ELSE 0 
                    END,
                    BlockedUntil = CASE 
                        WHEN TicketCount >= 5 AND DATEDIFF(HOUR, LastTicketDate, GETDATE()) <= 1 
                        THEN DATEADD(HOUR, 24, GETDATE()) 
                        ELSE NULL 
                    END
            WHEN NOT MATCHED THEN
                INSERT (Identifier, IdentifierType, LastTicketDate, TicketCount)
                VALUES (@Identifier, @IdentifierType, GETDATE(), 1);", conn))
                {
                    cmd.Parameters.AddWithValue("@Identifier", identifier);
                    cmd.Parameters.AddWithValue("@IdentifierType", identifierType);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Ticket Assignment Methods

        public bool TryLockTicketForAssignment(int ticketId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Use UPDLOCK to prevent race conditions
                        using (var cmd = new SqlCommand(@"
                    UPDATE Support_Tickets WITH (UPDLOCK, ROWLOCK)
                    SET AssignmentAttempts = AssignmentAttempts + 1,
                        LastAssignmentDate = GETDATE()
                    WHERE Id = @Id 
                    AND SupportUserId IS NULL 
                    AND Status = 1
                    AND AssignmentAttempts < 5", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@Id", ticketId);
                            var affected = cmd.ExecuteNonQuery();

                            if (affected > 0)
                            {
                                trans.Commit();
                                return true;
                            }
                            else
                            {
                                trans.Rollback();
                                return false;
                            }
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public SupportAgent GetNextAvailableAgent(int excludeAgentId = 0, string specialty = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var query = @"
            SELECT TOP 1 a.*, 
                   u.FirstName + ' ' + u.LastName AS UserFullName,
                   u.Mobile AS UserMobile,
                   u.Email AS UserEmail
            FROM Support_Agents a
            INNER JOIN KCI_Users u ON a.UserId = u.id
            WHERE a.IsActive = 1 
                  AND a.IsOnline = 1 
                  AND a.CurrentActiveTickets < a.MaxConcurrentTickets
                  AND a.Id != @ExcludeId";

                if (!string.IsNullOrEmpty(specialty))
                {
                    query += " AND (a.Specialties LIKE @Specialty OR a.Specialties IS NULL)";
                }

                // Smart ordering: least busy agents first, then by priority
                query += @" ORDER BY 
                    (CAST(a.CurrentActiveTickets AS FLOAT) / a.MaxConcurrentTickets),
                    a.Priority, 
                    a.TotalHandledTickets DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ExcludeId", excludeAgentId);
                    if (!string.IsNullOrEmpty(specialty))
                    {
                        cmd.Parameters.AddWithValue("@Specialty", $"%{specialty}%");
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapAgent(reader);
                        }
                    }
                }
            }
            return null;
        }

        #endregion

        #region Helper Mapping Methods

        private SMSQueue MapSMSQueue(SqlDataReader reader)
        {
            return new SMSQueue
            {
                Id = Convert.ToInt32(reader["Id"]),
                TicketId = Convert.ToInt32(reader["TicketId"]),
                RecipientMobile = reader["RecipientMobile"].ToString(),
                Message = reader["Message"].ToString(),
                ScheduledDate = Convert.ToDateTime(reader["ScheduledDate"]),
                SentDate = reader["SentDate"] as DateTime?,
                IsSent = Convert.ToBoolean(reader["IsSent"]),
                IsCancelled = Convert.ToBoolean(reader["IsCancelled"]),
                RetryCount = Convert.ToInt32(reader["RetryCount"])
            };
        }

        private AgentRequest MapAgentRequest(SqlDataReader reader)
        {
            return new AgentRequest
            {
                Id = Convert.ToInt32(reader["Id"]),
                TicketId = Convert.ToInt32(reader["TicketId"]),
                AgentId = Convert.ToInt32(reader["AgentId"]),
                RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                ResponseDate = reader["ResponseDate"] as DateTime?,
                IsAccepted = reader["IsAccepted"] as bool?,
                TimeoutDate = Convert.ToDateTime(reader["TimeoutDate"])
            };
        }

        private UserLimit MapUserLimit(SqlDataReader reader)
        {
            return new UserLimit
            {
                Id = Convert.ToInt32(reader["Id"]),
                Identifier = reader["Identifier"].ToString(),
                IdentifierType = reader["IdentifierType"].ToString(),
                LastTicketDate = Convert.ToDateTime(reader["LastTicketDate"]),
                TicketCount = Convert.ToInt32(reader["TicketCount"]),
                IsBlocked = Convert.ToBoolean(reader["IsBlocked"]),
                BlockedUntil = reader["BlockedUntil"] as DateTime?
            };
        }

        #endregion


        // گرفتن درخواست AgentRequest بر اساس Id
        public AgentRequest GetAgentRequestById(int requestId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT * FROM Support_AgentRequests 
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapAgentRequest(reader);
                        }
                    }
                }
            }
            return null;
        }

        // افزایش شمارنده تلاش ارسال SMS
        public void IncrementSMSRetryCount(int smsId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            UPDATE Support_SMSQueue 
            SET RetryCount = RetryCount + 1 
            WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", smsId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // لغو سایر درخواست‌های Agent برای یک تیکت به جز Agent فعلی
        public void CancelOtherAgentRequests(int ticketId, int acceptedAgentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            UPDATE Support_AgentRequests
            SET ResponseDate = GETDATE(), IsAccepted = 0
            WHERE TicketId = @TicketId 
              AND AgentId <> @AcceptedAgentId
              AND ResponseDate IS NULL", conn))
                {
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);
                    cmd.Parameters.AddWithValue("@AcceptedAgentId", acceptedAgentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public SupportAgent GetAgentById(int agentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT * FROM SupportAgent WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", agentId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapAgent(reader);
                    }
                }
            }
            return null;
        }
    }
}