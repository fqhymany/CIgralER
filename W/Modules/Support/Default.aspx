<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Modules_Support_Default" %>

<style>
    /* Support Panel Styles */
.tickets-list {
    max-height: 600px;
    overflow-y: auto;
}

.ticket-item {
    padding: 15px;
    border-bottom: 1px solid #e9ecef;
    cursor: pointer;
    transition: background-color 0.2s;
    position: relative;
}

    .ticket-item:hover {
        background-color: #f8f9fa;
    }

    .ticket-item.active {
        background-color: #e3f2fd;
        border-left: 3px solid #2196f3;
    }

.status-indicator {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background-color: #6c757d;
    display: inline-block;
    margin-top: 5px;
}

    .status-indicator.online {
        background-color: #28a745;
    }

.no-chat-selected {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    color: #6c757d;
}

.message {
    margin-bottom: 15px;
}

.message-visitor {
    text-align: right;
}

.message-support {
    text-align: left;
}

.message-content {
    display: inline-block;
    max-width: 70%;
    padding: 10px 15px;
    border-radius: 10px;
    text-align: right;
}

.message-visitor .message-content {
    background-color: #e3f2fd;
}

.message-support .message-content {
    background-color: #f5f5f5;
}

.message-header {
    display: flex;
    justify-content: space-between;
    margin-bottom: 5px;
    font-size: 0.85rem;
}

.message-body {
    white-space: pre-wrap;
    word-wrap: break-word;
}

.message-attachments {
    margin-top: 10px;
}

.attachment-link {
    display: inline-block;
    margin-right: 10px;
    padding: 5px 10px;
    background-color: #ffffff;
    border: 1px solid #dee2e6;
    border-radius: 5px;
    text-decoration: none;
    color: #495057;
    font-size: 0.85rem;
}

    .attachment-link:hover {
        background-color: #f8f9fa;
    }

.system-message {
    text-align: center;
    margin: 20px 0;
}

.typing-indicator {
    padding: 10px 15px;
    text-align: center;
}

/* RTL Support */
body[dir="rtl"] .ticket-item.active {
    border-left: none;
    border-right: 3px solid #2196f3;
}

body[dir="rtl"] .message-visitor {
    text-align: left;
}

body[dir="rtl"] .message-support {
    text-align: right;
}

body[dir="rtl"] .message-content {
    text-align: right;
}

body[dir="rtl"] .attachment-link {
    margin-right: 0;
    margin-left: 10px;
}
</style>

    <div class="container-fluid">
        <div class="row">
            <!-- لیست تیکت‌ها -->
            <div class="col-md-4 col-lg-3">
                <div class="card">
                    <div class="card-header">
                        <h5 class="mb-0">تیکت‌های فعال</h5>
                    </div>
                    <div class="card-body p-0">
                        <div id="ticketsList" class="tickets-list">
                            <!-- تیکت‌ها از طریق SignalR بارگذاری می‌شوند -->
                        </div>
                    </div>
                </div>
            </div>

            <!-- پنجره چت -->
            <div class="col-md-8 col-lg-9">
                <div class="card h-100">
                    <div id="noChatSelected" class="no-chat-selected">
                        <i class="fa fa-comments fa-3x text-muted"></i>
                        <p class="text-muted mt-3">یک تیکت را انتخاب کنید</p>
                    </div>

                    <div id="chatContainer" style="display: none;" class="h-100 d-flex flex-column">
                        <div class="card-header">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <h5 class="mb-0" id="ticketTitle">
                                        <span id="ticketNumber"></span>
                                        <small class="text-muted" id="visitorName"></small>
                                    </h5>
                                    <small class="text-muted">
                                        <span id="ticketStatus"></span>| 
                                       
                                        <span id="ticketDate"></span>
                                    </small>
                                </div>
                                <div>
                                    <button type="button" class="btn btn-sm btn-success"
                                        onclick="assignToMe()">
                                        <i class="fa fa-user-plus"></i>اختصاص به من
                                   
                                    </button>
                                    <button type="button" class="btn btn-sm btn-danger"
                                        onclick="closeTicket()">
                                        <i class="fa fa-times"></i>بستن تیکت
                                   
                                    </button>
                                </div>
                            </div>
                        </div>

                        <div class="card-body flex-grow-1 overflow-auto" id="messagesContainer">
                            <!-- پیام‌ها -->
                        </div>

                        <div class="typing-indicator" id="typingIndicator" style="display: none;">
                            <small class="text-muted">در حال تایپ...</small>
                        </div>

                        <div class="card-footer">
                            <div class="input-group">
                                <div class="input-group-prepend">
                                    <button class="btn btn-outline-secondary" type="button"
                                        onclick="selectFile()">
                                        <i class="fa fa-paperclip"></i>
                                    </button>
                                    <input type="file" id="fileInput" style="display: none;"
                                        onchange="uploadFile(event)" />
                                </div>
                                <textarea class="form-control" id="messageInput" rows="2"
                                    placeholder="پیام خود را بنویسید..."
                                    onkeypress="handleKeyPress(event)"></textarea>
                                <div class="input-group-append">
                                    <button class="btn btn-primary" type="button"
                                        onclick="sendMessage()">
                                        <i class="fa fa-paper-plane"></i>ارسال
                                   
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script src="/Scripts/jquery-3.6.0.min.js"></script>
    <script src="/Scripts/jquery.signalR-2.4.3.min.js"></script>
    <script src="/signalr/hubs"></script>
    <script>
        var currentTicketId = null;
        var currentUserId = <%= CurrentUserId %>;
        var connection = $.hubConnection();
        var hub = connection.createHubProxy('supportHub');

        // SignalR event handlers
        hub.on('loadActiveTickets', function (tickets) {
            loadTicketsList(tickets);
        });

        hub.on('receiveMessage', function (message) {
            if (message.ticketId === currentTicketId) {
                addMessageToChat(message);
            }
            updateTicketInList(message.ticketId);
        });

        hub.on('newMessage', function (ticketId, message) {
            updateTicketInList(ticketId);
            showNotification('پیام جدید', message);
        });

        hub.on('ticketOnline', function (ticketId) {
            $('#ticket-' + ticketId).find('.status-indicator').addClass('online');
        });

        hub.on('typing', function (isTyping) {
            $('#typingIndicator').toggle(isTyping);
        });

        hub.on('ticketAssigned', function (ticketId, supportUserId) {
            if (currentTicketId === ticketId) {
                loadTicketInfo(ticketId);
            }
            updateTicketInList(ticketId);
        });

        hub.on('ticketClosed', function (ticketId) {
            if (currentTicketId === ticketId) {
                $('#messageInput').prop('disabled', true);
                addSystemMessage('تیکت بسته شد');
            }
            removeTicketFromList(ticketId);
        });

        // Start SignalR connection
        connection.start().done(function () {
            hub.invoke('joinSupport', currentUserId);
        });

        function loadTicketsList(tickets) {
            var html = '';
            tickets.forEach(function (ticket) {
                html += createTicketListItem(ticket);
            });
            $('#ticketsList').html(html);
        }

        function createTicketListItem(ticket) {
            var statusClass = ticket.status === 1 ? 'badge-warning' : 'badge-info';
            var statusText = ticket.status === 1 ? 'باز' : 'در حال بررسی';

            return `
                <div class="ticket-item" id="ticket-${ticket.id}" onclick="selectTicket(${ticket.id})">
                    <div class="d-flex justify-content-between align-items-start">
                        <div class="flex-grow-1">
                            <h6 class="mb-1">${ticket.ticketNumber}</h6>
                            <p class="mb-1 small text-muted">
                                ${ticket.visitor ? ticket.visitor.fullName : 'کاربر'} - 
                                ${ticket.visitor ? ticket.visitor.mobile : ''}
                            </p>
                            <p class="mb-0 small">${ticket.subject || 'بدون موضوع'}</p>
                        </div>
                        <div class="text-right">
                            <span class="badge ${statusClass}">${statusText}</span>
                            <div class="status-indicator"></div>
                        </div>
                    </div>
                    <small class="text-muted">${formatDate(ticket.createDate)}</small>
                </div>
            `;
        }

        function selectTicket(ticketId) {
            currentTicketId = ticketId;
            $('.ticket-item').removeClass('active');
            $('#ticket-' + ticketId).addClass('active');

            $('#noChatSelected').hide();
            $('#chatContainer').show();

            loadTicketInfo(ticketId);
            loadMessages(ticketId);

            // Mark messages as read
            hub.invoke('markAsRead', ticketId, currentUserId);
        }

        function loadTicketInfo(ticketId) {
            $.get('/Support/GetTicket.ashx', { ticketId: ticketId }, function (data) {
                if (data.success) {
                    var ticket = data.ticket;
                    $('#ticketNumber').text(ticket.ticketNumber);
                    $('#visitorName').text(ticket.visitor ?
                        `(${ticket.visitor.fullName})` : '');
                    $('#ticketStatus').text(getStatusText(ticket.status));
                    $('#ticketDate').text(formatDate(ticket.createDate));

                    if (ticket.supportUserId === currentUserId || !ticket.supportUserId) {
                        $('#messageInput').prop('disabled', false);
                    } else {
                        $('#messageInput').prop('disabled', true);
                        addSystemMessage('این تیکت به پشتیبان دیگری اختصاص یافته است');
                    }
                }
            });
        }

        function loadMessages(ticketId) {
            $.get('/Support/GetMessages.ashx', { ticketId: ticketId }, function (data) {
                if (data.success) {
                    $('#messagesContainer').empty();
                    data.messages.forEach(function (msg) {
                        addMessageToChat(msg);
                    });
                    scrollToBottom();
                }
            });
        }

        function addMessageToChat(message) {
            var isSupport = message.senderType === 2;
            var messageClass = isSupport ? 'message-support' : 'message-visitor';
            var time = formatTime(message.createDate);

            var html = `
                <div class="message ${messageClass}">
                    <div class="message-content">
                        <div class="message-header">
                            <strong>${message.senderName}</strong>
                            <small class="text-muted">${time}</small>
                        </div>
                        <div class="message-body">${escapeHtml(message.message)}</div>
                        ${message.attachments && message.attachments.length > 0 ?
                    '<div class="message-attachments">' +
                    message.attachments.map(a => createAttachmentHtml(a)).join('') +
                    '</div>' : ''}
                    </div>
                </div>
            `;

            $('#messagesContainer').append(html);
            scrollToBottom();
        }

        function createAttachmentHtml(attachment) {
            var icon = getFileIcon(attachment.fileExtension);
            return `
                <a href="/Uploads/${attachment.filePath}" target="_blank" 
                   class="attachment-link">
                    <i class="fa ${icon}"></i> ${attachment.fileName}
                </a>
            `;
        }

        function addSystemMessage(text) {
            var html = `
                <div class="system-message">
                    <small class="text-muted">${text}</small>
                </div>
            `;
            $('#messagesContainer').append(html);
            scrollToBottom();
        }

        function sendMessage() {
            var message = $('#messageInput').val().trim();
            if (!message) return;

            hub.invoke('sendMessage', currentTicketId, message, true);
            $('#messageInput').val('');
        }

        function handleKeyPress(e) {
            if (e.keyCode === 13 && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        }

        function assignToMe() {
            if (confirm('آیا می‌خواهید این تیکت را به خود اختصاص دهید؟')) {
                hub.invoke('assignTicket', currentTicketId, currentUserId);
            }
        }

        function closeTicket() {
            if (confirm('آیا از بستن این تیکت اطمینان دارید؟')) {
                hub.invoke('closeTicket', currentTicketId, currentUserId);
            }
        }

        function selectFile() {
            $('#fileInput').click();
        }

        function uploadFile(e) {
            var file = e.target.files[0];
            if (!file) return;

            var formData = new FormData();
            formData.append('file', file);
            formData.append('ticketId', currentTicketId);
            formData.append('isSupport', true);

            $.ajax({
                url: '/Support/UploadFile.ashx',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (data) {
                    if (data.success) {
                        var message = 'فایل پیوست: ' + file.name;
                        hub.invoke('sendMessage', currentTicketId, message, true);
                    } else {
                        alert(data.message || 'خطا در آپلود فایل');
                    }
                }
            });
        }

        function updateTicketInList(ticketId) {
            // Reload ticket info in list
            $.get('/Support/GetTicket.ashx', { ticketId: ticketId }, function (data) {
                if (data.success) {
                    var $item = $('#ticket-' + ticketId);
                    if ($item.length) {
                        $item.replaceWith(createTicketListItem(data.ticket));
                    } else {
                        $('#ticketsList').prepend(createTicketListItem(data.ticket));
                    }
                }
            });
        }

        function removeTicketFromList(ticketId) {
            $('#ticket-' + ticketId).fadeOut(function () {
                $(this).remove();
            });
        }

        function showNotification(title, message) {
            if (Notification.permission === 'granted') {
                new Notification(title, {
                    body: message,
                    icon: '/Content/images/support-icon.png'
                });
            }
        }

        function scrollToBottom() {
            var container = $('#messagesContainer');
            container.scrollTop(container[0].scrollHeight);
        }

        function formatDate(date) {
            return new Date(date).toLocaleDateString('fa-IR');
        }

        function formatTime(date) {
            return new Date(date).toLocaleTimeString('fa-IR', {
                hour: '2-digit',
                minute: '2-digit'
            });
        }

        function getStatusText(status) {
            switch (status) {
                case 1: return 'باز';
                case 2: return 'در حال بررسی';
                case 3: return 'بسته شده';
                default: return '';
            }
        }

        function getFileIcon(extension) {
            extension = extension.toLowerCase();
            if (['.jpg', '.jpeg', '.png', '.gif'].includes(extension)) return 'fa-image';
            if (['.pdf'].includes(extension)) return 'fa-file-pdf';
            if (['.doc', '.docx'].includes(extension)) return 'fa-file-word';
            if (['.xls', '.xlsx'].includes(extension)) return 'fa-file-excel';
            if (['.zip', '.rar'].includes(extension)) return 'fa-file-archive';
            return 'fa-file';
        }

        function escapeHtml(text) {
            var map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, function (m) { return map[m]; });
        }

        // Request notification permission
        if (Notification.permission === 'default') {
            Notification.requestPermission();
        }
    </script>