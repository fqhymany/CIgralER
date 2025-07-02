(function () {
    // Configuration
    var config = {
        baseUrl: '/Support/',
        position: 'bottom-right',
        primaryColor: '#007bff',
        title: 'پشتیبانی آنلاین'
    };

    // Create widget HTML
    var widgetHtml = `
        <div id="support-widget" class="support-widget ${config.position}">
            <button id="support-toggle" class="support-toggle">
                <i class="icon-chat"></i>
                <span class="badge" style="display:none;">0</span>
            </button>
            <div id="support-panel" class="support-panel" style="display:none;">
                <div class="support-header">
                    <h5>${config.title}</h5>
                    <button class="close-btn" onclick="SupportWidget.close()">×</button>
                </div>
                <div id="support-content" class="support-content">
                    <!-- Content will be loaded here -->
                </div>
            </div>
        </div>
    `;

    // Widget CSS
    var widgetCss = `
        <style>
            .support-widget {
                position: fixed;
                z-index: 9999;
            }
            .support-widget.bottom-right {
                bottom: 20px;
                right: 20px;
            }
            .support-toggle {
                width: 60px;
                height: 60px;
                border-radius: 50%;
                background: ${config.primaryColor};
                color: white;
                border: none;
                box-shadow: 0 4px 12px rgba(0,0,0,.15);
                cursor: pointer;
                position: relative;
            }
            .support-toggle:hover {
                transform: scale(1.1);
            }
            .support-toggle .badge {
                position: absolute;
                top: -5px;
                right: -5px;
                background: #dc3545;
                color: white;
                border-radius: 10px;
                padding: 2px 6px;
                font-size: 11px;
            }
            .support-panel {
                position: absolute;
                bottom: 80px;
                right: 0;
                width: 350px;
                height: 500px;
                background: white;
                border-radius: 10px;
                box-shadow: 0 5px 40px rgba(0,0,0,.16);
                display: flex;
                flex-direction: column;
            }
            .support-header {
                padding: 15px;
                background: ${config.primaryColor};
                color: white;
                border-radius: 10px 10px 0 0;
                display: flex;
                justify-content: space-between;
                align-items: center;
            }
            .support-header h5 {
                margin: 0;
                font-size: 16px;
            }
            .close-btn {
                background: none;
                border: none;
                color: white;
                font-size: 24px;
                cursor: pointer;
                padding: 0;
                width: 30px;
                height: 30px;
            }
            .support-content {
                flex: 1;
                overflow-y: auto;
                position: relative;
            }
            .login-form, .chat-container {
                padding: 20px;
                height: 100%;
                display: flex;
                flex-direction: column;
            }
            .form-group {
                margin-bottom: 15px;
            }
            .form-group label {
                display: block;
                margin-bottom: 5px;
                font-size: 14px;
            }
            .form-group input, .form-group textarea {
                width: 100%;
                padding: 8px 12px;
                border: 1px solid #ddd;
                border-radius: 4px;
                font-size: 14px;
            }
            .btn {
                padding: 10px 20px;
                border: none;
                border-radius: 4px;
                cursor: pointer;
                font-size: 14px;
            }
            .btn-primary {
                background: ${config.primaryColor};
                color: white;
            }
            .btn-primary:hover {
                opacity: 0.9;
            }
            .messages-container {
                flex: 1;
                overflow-y: auto;
                padding: 10px;
                background: #f8f9fa;
            }
            .message {
                margin-bottom: 10px;
                display: flex;
            }
            .message.visitor {
                justify-content: flex-end;
            }
            .message.support {
                justify-content: flex-start;
            }
            .message-bubble {
                max-width: 70%;
                padding: 8px 12px;
                border-radius: 8px;
                position: relative;
            }
            .message.visitor .message-bubble {
                background: ${config.primaryColor};
                color: white;
            }
            .message.support .message-bubble {
                background: #e9ecef;
                color: #333;
            }
            .message-time {
                font-size: 11px;
                opacity: 0.7;
                margin-top: 4px;
            }
            .chat-input-container {
                padding: 10px;
                border-top: 1px solid #ddd;
                display: flex;
                align-items: center;
            }
            .chat-input {
                flex: 1;
                padding: 8px 12px;
                border: 1px solid #ddd;
                border-radius: 20px;
                outline: none;
            }
            .send-btn {
                margin-left: 10px;
                width: 36px;
                height: 36px;
                border-radius: 50%;
                background: ${config.primaryColor};
                color: white;
                border: none;
                cursor: pointer;
                display: flex;
                align-items: center;
                justify-content: center;
            }
            .typing-indicator {
                padding: 10px;
                font-style: italic;
                color: #666;
                font-size: 13px;
            }
            .attachment-btn {
                margin-right: 10px;
                background: none;
                border: none;
                color: #666;
                cursor: pointer;
                font-size: 20px;
            }
            @media (max-width: 480px) {
                .support-panel {
                    width: 100vw;
                    height: 100vh;
                    bottom: 0;
                    right: 0;
                    border-radius: 0;
                }
                .support-widget.bottom-right {
                    bottom: 10px;
                    right: 10px;
                }
            }
        </style>
    `;

    // Support Widget Object
    window.SupportWidget = {
        isOpen: false,
        ticketId: null,
        connection: null,
        mobile: null,

        init: function () {
            // Add CSS
            document.head.insertAdjacentHTML('beforeend', widgetCss);

            // Add HTML
            document.body.insertAdjacentHTML('beforeend', widgetHtml);

            // Add event listeners
            document.getElementById('support-toggle').addEventListener('click', this.toggle.bind(this));

            // Check for existing session
            this.checkSession();
        },

        toggle: function () {
            this.isOpen = !this.isOpen;
            var panel = document.getElementById('support-panel');
            panel.style.display = this.isOpen ? 'flex' : 'none';

            if (this.isOpen && !this.ticketId) {
                this.showLoginForm();
            }
        },

        close: function () {
            this.isOpen = false;
            document.getElementById('support-panel').style.display = 'none';
        },

        showLoginForm: function () {
            var content = document.getElementById('support-content');
            content.innerHTML = `
                <div class="login-form">
                    <h6>شروع گفتگو</h6>
                    <form onsubmit="SupportWidget.startChat(event)">
                        <div class="form-group">
                            <label>شماره موبایل *</label>
                            <input type="tel" id="mobile" required pattern="09[0-9]{9}" 
                                   placeholder="09123456789" maxlength="11">
                        </div>
                        <div class="form-group">
                            <label>نام</label>
                            <input type="text" id="firstName" placeholder="نام">
                        </div>
                        <div class="form-group">
                            <label>نام خانوادگی</label>
                            <input type="text" id="lastName" placeholder="نام خانوادگی">
                        </div>
                        <div class="form-group">
                            <label>موضوع</label>
                            <input type="text" id="subject" placeholder="موضوع گفتگو">
                        </div>
                        <div class="form-group">
                            <label>پیام اولیه</label>
                            <textarea id="initialMessage" rows="3" 
                                      placeholder="پیام خود را بنویسید..."></textarea>
                        </div>
                        <button type="submit" class="btn btn-primary" style="width: 100%;">
                            شروع گفتگو
                        </button>
                    </form>
                </div>
            `;
        },

        startChat: function (e) {
            e.preventDefault();

            var mobile = document.getElementById('mobile').value;
            var firstName = document.getElementById('firstName').value;
            var lastName = document.getElementById('lastName').value;
            var subject = document.getElementById('subject').value;
            var initialMessage = document.getElementById('initialMessage').value;

            if (!mobile) {
                alert('لطفا شماره موبایل را وارد کنید');
                return;
            }

            // Save mobile for session
            this.mobile = mobile;
            localStorage.setItem('support_mobile', mobile);

            // Send request to create ticket
            fetch(config.baseUrl + 'CreateTicket.ashx', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    mobile: mobile,
                    firstName: firstName,
                    lastName: lastName,
                    subject: subject,
                    initialMessage: initialMessage
                })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        this.ticketId = data.ticketId;
                        this.showChat();
                        this.initSignalR();
                    } else {
                        alert(data.message || 'خطا در ایجاد تیکت');
                    }
                })
                .catch(error => {
                    alert('خطا در برقراری ارتباط');
                    console.error(error);
                });
        },

        showChat: function () {
            var content = document.getElementById('support-content');
            content.innerHTML = `
                <div class="chat-container">
                    <div class="messages-container" id="messages-container">
                        <!-- Messages will appear here -->
                    </div>
                    <div class="typing-indicator" id="typing-indicator" style="display:none;">
                        در حال تایپ...
                    </div>
                    <div class="chat-input-container">
                        <button class="attachment-btn" onclick="SupportWidget.selectFile()">
                            📎
                        </button>
                        <input type="file" id="file-input" style="display:none;" 
                               onchange="SupportWidget.uploadFile(event)">
                        <input type="text" class="chat-input" id="message-input" 
                               placeholder="پیام خود را بنویسید..."
                               onkeypress="SupportWidget.handleKeyPress(event)">
                        <button class="send-btn" onclick="SupportWidget.sendMessage()">
                            ➤
                        </button>
                    </div>
                </div>
            `;

            // Load existing messages
            this.loadMessages();
        },

        initSignalR: function () {
            // Load SignalR script if not loaded
            if (!window.jQuery || !window.jQuery.signalR) {
                var script = document.createElement('script');
                script.src = '/Scripts/jquery.signalR-2.4.3.min.js';
                script.onload = () => {
                    this.connectSignalR();
                };
                document.head.appendChild(script);
            } else {
                this.connectSignalR();
            }
        },

        connectSignalR: function () {
            this.connection = $.hubConnection();
            var hub = this.connection.createHubProxy('supportHub');

            // Event handlers
            hub.on('receiveMessage', (message) => {
                this.addMessage(message);
            });

            hub.on('typing', (isTyping) => {
                document.getElementById('typing-indicator').style.display =
                    isTyping ? 'block' : 'none';
            });

            hub.on('ticketClosed', (ticketId) => {
                if (ticketId === this.ticketId) {
                    this.addSystemMessage('گفتگو بسته شد');
                    document.getElementById('message-input').disabled = true;
                }
            });

            // Start connection
            this.connection.start().done(() => {
                hub.invoke('joinChat', this.ticketId);
            });
        },

        loadMessages: function () {
            fetch(config.baseUrl + 'GetMessages.ashx?ticketId=' + this.ticketId)
                .then(response => response.json())
                .then(data => {
                    if (data.success && data.messages) {
                        data.messages.forEach(msg => this.addMessage(msg));
                    }
                });
        },

        addMessage: function (message) {
            var container = document.getElementById('messages-container');
            var messageClass = message.senderType === 1 ? 'visitor' : 'support';
            var time = new Date(message.createDate).toLocaleTimeString('fa-IR', {
                hour: '2-digit',
                minute: '2-digit'
            });

            var messageHtml = `
                <div class="message ${messageClass}">
                    <div class="message-bubble">
                        <div>${message.message}</div>
                        <div class="message-time">${time}</div>
                    </div>
                </div>
            `;

            container.insertAdjacentHTML('beforeend', messageHtml);
            container.scrollTop = container.scrollHeight;
        },

        addSystemMessage: function (text) {
            var container = document.getElementById('messages-container');
            var messageHtml = `
                <div style="text-align: center; margin: 10px 0; color: #666; font-size: 13px;">
                    ${text}
                </div>
            `;
            container.insertAdjacentHTML('beforeend', messageHtml);
        },

        sendMessage: function () {
            var input = document.getElementById('message-input');
            var message = input.value.trim();

            if (!message) return;

            // Send via SignalR
            var hub = this.connection.createHubProxy('supportHub');
            hub.invoke('sendMessage', this.ticketId, message, false);

            // Clear input
            input.value = '';
        },

        handleKeyPress: function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
        },

        selectFile: function () {
            document.getElementById('file-input').click();
        },

        uploadFile: function (e) {
            var file = e.target.files[0];
            if (!file) return;

            var formData = new FormData();
            formData.append('file', file);
            formData.append('ticketId', this.ticketId);

            fetch(config.baseUrl + 'UploadFile.ashx', {
                method: 'POST',
                body: formData
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        // File uploaded successfully
                        var message = `فایل ارسال شد: ${file.name}`;
                        var hub = this.connection.createHubProxy('supportHub');
                        hub.invoke('sendMessage', this.ticketId, message, false);
                    } else {
                        alert(data.message || 'خطا در آپلود فایل');
                    }
                });
        },

        checkSession: function () {
            var savedMobile = localStorage.getItem('support_mobile');
            if (savedMobile) {
                // Check for active ticket
                fetch(config.baseUrl + 'CheckSession.ashx?mobile=' + savedMobile)
                    .then(response => response.json())
                    .then(data => {
                        if (data.hasActiveTicket) {
                            this.ticketId = data.ticketId;
                            this.mobile = savedMobile;

                            // Show unread count
                            if (data.unreadCount > 0) {
                                var badge = document.querySelector('.support-toggle .badge');
                                badge.textContent = data.unreadCount;
                                badge.style.display = 'block';
                            }
                        }
                    });
            }
        }
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            SupportWidget.init();
        });
    } else {
        SupportWidget.init();
    }
})();