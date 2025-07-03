(function () {
    // Configuration
    var config = {
        baseUrl: "/Modules/Support/",
        position: "bottom-right",
        primaryColor: "#007bff",
        title: "پشتیبانی آنلاین",
        checkSessionInterval: 60000, // Check every minute
    };

    var state = {
        isOpen: false,
        ticketId: null,
        connection: null,
        mobile: null,
        isAuthenticated: false,
        sessionCheckInterval: null,
    };

    // Create enhanced widget HTML
    var widgetHtml = `
        <div id="support-widget" class="support-widget ${config.position}">
            <button id="support-toggle" class="support-toggle">
                <i class="icon-chat"></i>
                <span class="badge" id="unread-badge" style="display:none;">0</span>
                <span class="status-indicator" id="widget-status"></span>
            </button>
            <div id="support-panel" class="support-panel" style="display:none;">
                <div class="support-header">
                    <h5>${config.title}</h5>
                    <span id="connection-status" class="connection-status"></span>
                    <button class="close-btn" onclick="SupportWidget.close()">×</button>
                </div>
                <div id="support-content" class="support-content">
                    <div class="loading-spinner">
                        <i class="fa fa-spinner fa-spin"></i>
                        <p>در حال بارگذاری...</p>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Enhanced widget CSS
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
            
            .status-indicator {
                position: absolute;
                bottom: 5px;
                right: 5px;
                width: 12px;
                height: 12px;
                border-radius: 50%;
                background: #6c757d;
                border: 2px solid white;
            }
            
            .status-indicator.online {
                background: #28a745;
            }
            
            .status-indicator.away {
                background: #ffc107;
            }
            
            .connection-status {
                font-size: 12px;
                color: rgba(255,255,255,0.8);
            }
            
            .loading-spinner {
                text-align: center;
                padding: 50px;
                color: #6c757d;
            }
            
            .loading-spinner i {
                font-size: 32px;
                margin-bottom: 10px;
            }
            
            .info-message {
                background: #d1ecf1;
                border: 1px solid #bee5eb;
                color: #0c5460;
                padding: 10px;
                border-radius: 4px;
                margin: 10px 0;
                font-size: 14px;
            }
            
            .error-message {
                background: #f8d7da;
                border: 1px solid #f5c6cb;
                color: #721c24;
                padding: 10px;
                border-radius: 4px;
                margin: 10px 0;
                font-size: 14px;
            }
            
            .quick-replies {
                display: flex;
                flex-wrap: wrap;
                gap: 5px;
                margin-top: 10px;
            }
            
            .quick-reply-btn {
                background: #f0f0f0;
                border: 1px solid #ddd;
                padding: 5px 10px;
                border-radius: 15px;
                font-size: 12px;
                cursor: pointer;
                transition: all 0.2s;
            }
            
            .quick-reply-btn:hover {
                background: ${config.primaryColor};
                color: white;
            }
            
            .attachment-preview {
                display: flex;
                align-items: center;
                padding: 5px;
                background: #f8f9fa;
                border-radius: 4px;
                margin-top: 5px;
                font-size: 12px;
            }
            
            .attachment-preview i {
                margin-right: 5px;
            }
            
            .remove-attachment {
                margin-left: auto;
                color: #dc3545;
                cursor: pointer;
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
            document.head.insertAdjacentHTML("beforeend", widgetCss);

            // Add HTML
            document.body.insertAdjacentHTML("beforeend", widgetHtml);

            // Add event listeners
            document
                .getElementById("support-toggle")
                .addEventListener("click", this.toggle.bind(this));

            // Check for authenticated user
            this.checkAuthentication();

            // Check for existing session
            this.checkSession();

            // Start session check interval
            this.startSessionCheck();

            // Check agent availability
            this.checkAgentAvailability();
        },

        checkAuthentication: function () {
            // Check if user is logged in via cookie or other method
            var authCookie = document.cookie.match(/user_authenticated=true/);
            state.isAuthenticated = !!authCookie;
        },

        toggle: function () {
            state.isOpen = !state.isOpen;
            var panel = document.getElementById("support-panel");
            panel.style.display = state.isOpen ? "flex" : "none";

            if (state.isOpen) {
                if (state.ticketId) {
                    this.showChat();
                    this.initSignalR();
                } else if (state.isAuthenticated) {
                    this.showAuthenticatedForm();
                } else {
                    this.showLoginForm();
                }

                // Clear unread badge
                this.updateUnreadBadge(0);
            }
        },

        close: function () {
            state.isOpen = false;
            document.getElementById("support-panel").style.display = "none";
        },

        showAuthenticatedForm: function () {
            var content = document.getElementById("support-content");
            content.innerHTML = `
                <div class="login-form">
                    <h6>شروع گفتگوی جدید</h6>
                    <div class="info-message">
                        <i class="fa fa-info-circle"></i>
                        شما به عنوان کاربر سیستم وارد شده‌اید
                    </div>
                    <form onsubmit="SupportWidget.startAuthenticatedChat(event)">
                        <div class="form-group">
                            <label>موضوع</label>
                            <select class="form-control" id="subject" onchange="SupportWidget.onSubjectChange()">
                                <option value="">انتخاب موضوع</option>
                                <option value="technical">مشکل فنی</option>
                                <option value="billing">امور مالی</option>
                                <option value="general">سوال عمومی</option>
                                <option value="other">سایر</option>
                            </select>
                        </div>
                        <div class="form-group" id="customSubject" style="display:none;">
                            <input type="text" class="form-control" 
                                   placeholder="موضوع را وارد کنید">
                        </div>
                        <div class="form-group">
                            <label>پیام</label>
                            <textarea id="initialMessage" rows="3" class="form-control"
                                      placeholder="پیام خود را بنویسید..."></textarea>
                        </div>
                        <div class="quick-replies">
                            <span class="quick-reply-btn" onclick="SupportWidget.insertQuickReply('سلام')">
                                سلام
                            </span>
                            <span class="quick-reply-btn" onclick="SupportWidget.insertQuickReply('نیاز به راهنمایی دارم')">
                                نیاز به راهنمایی دارم
                            </span>
                            <span class="quick-reply-btn" onclick="SupportWidget.insertQuickReply('مشکل فنی دارم')">
                                مشکل فنی دارم
                            </span>
                        </div>
                        <button type="submit" class="btn btn-primary" style="width: 100%; margin-top: 15px;">
                            <i class="fa fa-comments"></i> شروع گفتگو
                        </button>
                    </form>
                </div>
            `;
        },

        showLoginForm: function () {
            var content = document.getElementById("support-content");
            content.innerHTML = `
                <div class="login-form">
                    <h6>شروع گفتگو</h6>
                    <div class="info-message">
                        <i class="fa fa-info-circle"></i>
                        برای دسترسی به امکانات بیشتر، وارد حساب کاربری خود شوید
                    </div>
                    <form onsubmit="SupportWidget.startChat(event)">
                        <div class="form-group">
                            <label>شماره موبایل *</label>
                            <input type="tel" id="mobile" required pattern="09[0-9]{9}" 
                                   placeholder="09123456789" maxlength="11"
                                   onblur="SupportWidget.checkExistingUser()">
                            <small class="form-text text-muted">
                                برای پیگیری راحت‌تر، شماره موبایل خود را وارد کنید
                            </small>
                        </div>
                        <div id="additionalFields" style="display:none;">
                            <div class="form-group">
                                <label>نام</label>
                                <input type="text" id="firstName" placeholder="نام">
                            </div>
                            <div class="form-group">
                                <label>نام خانوادگی</label>
                                <input type="text" id="lastName" placeholder="نام خانوادگی">
                            </div>
                        </div>
                        <div class="form-group">
                            <label>موضوع</label>
                            <input type="text" id="subject" placeholder="موضوع گفتگو">
                        </div>
                        <div class="form-group">
                            <label>پیام</label>
                            <textarea id="initialMessage" rows="3" class="form-control"
                                      placeholder="پیام خود را بنویسید..." required></textarea>
                        </div>
                        <button type="submit" class="btn btn-primary" style="width: 100%;">
                            <i class="fa fa-paper-plane"></i> ارسال پیام
                        </button>
                    </form>
                </div>
            `;
        },

        checkExistingUser: function () {
            var mobile = document.getElementById("mobile").value;
            if (mobile.length === 11) {
                // Check if this is a returning visitor
                fetch(config.baseUrl + "CheckVisitor.ashx?mobile=" + mobile)
                    .then((response) => response.json())
                    .then((data) => {
                        if (!data.exists) {
                            document.getElementById("additionalFields").style.display =
                                "block";
                        }
                    });
            }
        },

        onSubjectChange: function () {
            var subject = document.getElementById("subject").value;
            document.getElementById("customSubject").style.display =
                subject === "other" ? "block" : "none";
        },

        insertQuickReply: function (text) {
            var textarea = document.getElementById("initialMessage");
            textarea.value = text;
            textarea.focus();
        },

        startAuthenticatedChat: function (e) {
            e.preventDefault();

            var subject = document.getElementById("subject").value;
            if (subject === "other") {
                subject = document.querySelector("#customSubject input").value;
            }
            var initialMessage = document.getElementById("initialMessage").value;

            if (!initialMessage) {
                this.showError("لطفا پیام خود را وارد کنید");
                return;
            }

            this.showLoading();

            // Send request without mobile for authenticated users
            fetch(config.baseUrl + "CreateTicket.ashx", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                credentials: "same-origin",
                body: JSON.stringify({
                    subject: subject,
                    initialMessage: initialMessage,
                }),
            })
                .then((response) => response.json())
                .then((data) => {
                    if (data.success) {
                        state.ticketId = data.ticketId;
                        this.showChat();
                        this.initSignalR();

                        if (data.assignmentStatus === "pending") {
                            this.showInfo("در حال یافتن بهترین پشتیبان برای شما...");
                        }
                    } else {
                        if (data.isRateLimitError) {
                            this.showError(
                                "شما به حد مجاز ارسال تیکت رسیده‌اید. لطفا کمی صبر کنید."
                            );
                        } else {
                            this.showError(data.message || "خطا در ایجاد تیکت");
                        }
                    }
                })
                .catch((error) => {
                    this.showError("خطا در برقراری ارتباط");
                    console.error(error);
                });
        },

        startChat: function (e) {
            e.preventDefault();

            var mobile = document.getElementById("mobile").value;
            var firstName = document.getElementById("firstName")?.value;
            var lastName = document.getElementById("lastName")?.value;
            var subject = document.getElementById("subject").value;
            var initialMessage = document.getElementById("initialMessage").value;

            if (!mobile || !initialMessage) {
                this.showError("لطفا فیلدهای الزامی را پر کنید");
                return;
            }

            // Save mobile for session
            state.mobile = mobile;
            localStorage.setItem("support_mobile", mobile);

            this.showLoading();

            // Send request
            fetch(config.baseUrl + "CreateTicket.ashx", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    mobile: mobile,
                    firstName: firstName,
                    lastName: lastName,
                    subject: subject,
                    initialMessage: initialMessage,
                }),
            })
                .then((response) => response.json())
                .then((data) => {
                    if (data.success) {
                        state.ticketId = data.ticketId;
                        this.showChat();
                        this.initSignalR();
                    } else {
                        this.showError(data.message || "خطا در ایجاد تیکت");
                    }
                })

                .catch((error) => {
                    this.showError("خطا در برقراری ارتباط");
                });
        },

        showChat: function () {
            var content = document.getElementById("support-content");
            content.innerHTML = `
                <div class="chat-container">
                    <div class="messages-container" id="messages-container">
                        <!-- Messages will appear here -->
                    </div>
                    <div class="typing-indicator" id="typing-indicator" style="display:none;">
                        در حال تایپ...
                    </div>
                    <div id="attachment-preview" style="display:none;"></div>
                    <div class="chat-input-container">
                        <button class="attachment-btn" onclick="SupportWidget.selectFile()">
                            📎
                        </button>
                        <input type="file" id="file-input" style="display:none;" 
                               onchange="SupportWidget.previewFile(event)" multiple>
                        <input type="text" class="chat-input" id="message-input" 
                               placeholder="پیام خود را بنویسید..."
                               onkeypress="SupportWidget.handleKeyPress(event)"
                               oninput="SupportWidget.handleTyping()">
                        <button class="send-btn" onclick="SupportWidget.sendMessage()">
                            ➤
                        </button>
                    </div>
                </div>
            `;

            // Load existing messages
            this.loadMessages();
        },

        showLoading: function () {
            var content = document.getElementById("support-content");
            content.innerHTML = `
                <div class="loading-spinner">
                    <i class="fa fa-spinner fa-spin"></i>
                    <p>در حال بارگذاری...</p>
                </div>
            `;
        },

        showError: function (message) {
            var container = document.getElementById("support-content");
            var errorHtml = `<div class="error-message">${message}</div>`;

            if (container.querySelector(".login-form")) {
                container.querySelector(".error-message")?.remove();
                container
                    .querySelector("form")
                    .insertAdjacentHTML("beforebegin", errorHtml);
            } else {
                container.innerHTML = errorHtml;
            }
        },

        showInfo: function (message) {
            var container = document.getElementById("messages-container");
            if (container) {
                var infoHtml = `<div class="info-message">${message}</div>`;
                container.insertAdjacentHTML("beforeend", infoHtml);
                container.scrollTop = container.scrollHeight;
            }
        },

        checkAgentAvailability: function () {
            fetch(config.baseUrl + "CheckAvailability.ashx")
                .then((response) => response.json())
                .then((data) => {
                    var statusIndicator = document.getElementById("widget-status");
                    if (data.hasOnlineAgents) {
                        statusIndicator.className = "status-indicator online";
                        document.getElementById("connection-status").textContent = "آنلاین";
                    } else {
                        statusIndicator.className = "status-indicator away";
                        document.getElementById("connection-status").textContent = "آفلاین";
                    }
                });
        },

        handleTyping: function () {
            if (this.typingTimer) clearTimeout(this.typingTimer);

            var hub = state.connection?.createHubProxy("supportHub");
            if (hub) {
                hub.invoke("typing", state.ticketId, true);

                this.typingTimer = setTimeout(function () {
                    hub.invoke("typing", state.ticketId, false);
                }, 1000);
            }
        },

        previewFile: function (e) {
            var files = e.target.files;
            if (!files.length) return;

            var preview = document.getElementById("attachment-preview");
            preview.innerHTML = "";
            preview.style.display = "block";

            for (var i = 0; i < files.length; i++) {
                var file = files[i];
                preview.innerHTML += `
                    <div class="attachment-preview">
                        <i class="fa fa-file"></i>
                        ${file.name}
                        <span class="remove-attachment" onclick="SupportWidget.removeFile()">
                            ×
                        </span>
                    </div>
                `;
            }
        },

        removeFile: function () {
            document.getElementById("file-input").value = "";
            document.getElementById("attachment-preview").style.display = "none";
        },

        updateUnreadBadge: function (count) {
            var badge = document.getElementById("unread-badge");
            if (count > 0) {
                badge.textContent = count;
                badge.style.display = "block";
            } else {
                badge.style.display = "none";
            }
        },

        initSignalR: function () {
            var self = this;
            function loadScript(src, onload) {
                var script = document.createElement("script");
                script.src = src;
                script.onload = onload;
                document.head.appendChild(script);
            }

            // Ensure jQuery is loaded first
            if (!window.jQuery) {
                loadScript("/Scripts/jquery-3.7.1.min.js", function () {
                    // After jQuery, load SignalR
                    if (!window.jQuery.signalR) {
                        loadScript("/Scripts/jquery.signalR.min.js", function () {
                            self.connectSignalR();
                        });
                    } else {
                        self.connectSignalR();
                    }
                });
            } else if (!window.jQuery.signalR) {
                loadScript("/Scripts/jquery.signalR.min.js", function () {
                    self.connectSignalR();
                });
            } else {
                self.connectSignalR();
            }
        },

        connectSignalR: function () {
            this.connection = $.hubConnection();
            var hub = this.connection.createHubProxy("supportHub");

            // Event handlers
            hub.on("receiveMessage", (message) => {
                this.addMessage(message);
            });

            hub.on("typing", (isTyping) => {
                document.getElementById("typing-indicator").style.display = isTyping
                    ? "block"
                    : "none";
            });

            hub.on("ticketClosed", (ticketId) => {
                if (ticketId === this.ticketId) {
                    this.addSystemMessage("گفتگو بسته شد");
                    document.getElementById("message-input").disabled = true;
                }
            });

            // Start connection
            this.connection.start().done(() => {
                hub.invoke("joinChat", this.ticketId);
            });
        },

        loadMessages: function () {
            fetch(config.baseUrl + "GetMessages.ashx?ticketId=" + this.ticketId)
                .then((response) => response.json())
                .then((data) => {
                    if (data.success && data.messages) {
                        data.messages.forEach((msg) => this.addMessage(msg));
                    }
                });
        },

        addMessage: function (message) {
            var container = document.getElementById("messages-container");
            var messageClass = message.senderType === 1 ? "visitor" : "support";
            var time = new Date(message.createDate).toLocaleTimeString("fa-IR", {
                hour: "2-digit",
                minute: "2-digit",
            });

            var messageHtml = `
                <div class="message ${messageClass}">
                    <div class="message-bubble">
                        <div>${message.message}</div>
                        <div class="message-time">${time}</div>
                    </div>
                </div>
            `;

            container.insertAdjacentHTML("beforeend", messageHtml);
            container.scrollTop = container.scrollHeight;
        },

        addSystemMessage: function (text) {
            var container = document.getElementById("messages-container");
            var messageHtml = `
                <div style="text-align: center; margin: 10px 0; color: #666; font-size: 13px;">
                    ${text}
                </div>
            `;
            container.insertAdjacentHTML("beforeend", messageHtml);
        },

        sendMessage: function () {
            var input = document.getElementById("message-input");
            var message = input.value.trim();

            if (!message) return;

            // Send via SignalR
            var hub = this.connection.createHubProxy("supportHub");
            hub.invoke("sendMessage", this.ticketId, message, false);

            // Clear input
            input.value = "";
        },

        handleKeyPress: function (e) {
            if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
        },

        selectFile: function () {
            document.getElementById("file-input").click();
        },

        uploadFile: function (e) {
            var file = e.target.files[0];
            if (!file) return;

            var formData = new FormData();
            formData.append("file", file);
            formData.append("ticketId", this.ticketId);

            fetch(config.baseUrl + "UploadFile.ashx", {
                method: "POST",
                body: formData,
            })
                .then((response) => response.json())
                .then((data) => {
                    if (data.success) {
                        // File uploaded successfully
                        var message = `فایل ارسال شد: ${file.name}`;
                        var hub = this.connection.createHubProxy("supportHub");
                        hub.invoke("sendMessage", this.ticketId, message, false);
                    } else {
                        alert(data.message || "خطا در آپلود فایل");
                    }
                });
        },

        checkSession: function () {
            var savedMobile = localStorage.getItem("support_mobile");
            if (savedMobile || state.isAuthenticated) {
                // Check for active ticket
                var url = config.baseUrl + "CheckSession.ashx";
                if (savedMobile) {
                    url += "?mobile=" + savedMobile;
                }

                fetch(url, { credentials: "same-origin" })
                    .then((response) => response.json())
                    .then((data) => {
                        if (data.hasActiveTicket) {
                            state.ticketId = data.ticketId;
                            state.mobile = savedMobile;

                            // Show unread count
                            if (data.unreadCount > 0) {
                                this.updateUnreadBadge(data.unreadCount);
                            }
                        }
                    });
            }
        },

        startSessionCheck: function () {
            state.sessionCheckInterval = setInterval(() => {
                this.checkSession();
                this.checkAgentAvailability();
            }, config.checkSessionInterval);
        },

        // Cleanup on page unload
        cleanup: function () {
            if (state.sessionCheckInterval) {
                clearInterval(state.sessionCheckInterval);
            }
            if (state.connection) {
                state.connection.stop();
            }
        },
    };

    // Initialize when DOM is ready
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", function () {
            SupportWidget.init();
        });
    } else {
        SupportWidget.init();
    }

    // Cleanup on page unload
    window.addEventListener("beforeunload", function () {
        SupportWidget.cleanup();
    });
})();