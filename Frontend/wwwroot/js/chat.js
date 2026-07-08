// chat.js - verbose logging for debugging
(function () {
    console.log('chat.js: script load start');
    function safeLog(line, msg) {
        try {
            console.log('LINE', line, '-', msg);
        } catch (e) {
            // ignore
        }
    }

    document.addEventListener('DOMContentLoaded', async function () {
        safeLog(1, 'DOMContentLoaded fired');
        // Ensure SignalR client library is available. If not, try to load it dynamically as a fallback.
        async function ensureSignalR() {
            // Check if SignalR is already available
            if (window.signalR && window.signalR.HubConnectionBuilder) {
                safeLog(1.1, 'SignalR already present');
                return;
            }

            safeLog(1.2, 'SignalR not present, attempting to load CDN fallback');

            return new Promise(function (resolve, reject) {
                try {
                    var s = document.createElement('script');
                    s.src = 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.12/dist/browser/signalr.min.js';
                    s.async = true;
                    s.onload = function () {
                        if (window.signalR && window.signalR.HubConnectionBuilder) {
                            safeLog(1.3, '✅ dynamically loaded SignalR script successfully');
                            resolve();
                        } else {
                            safeLog(1.35, '⚠ SignalR script loaded but global object missing');
                            reject(new Error("SignalR did not attach to window"));
                        }
                    };
                    s.onerror = function (e) {
                        safeLog(1.4, '❌ failed to load SignalR script');
                        reject(e);
                    };
                    document.head.appendChild(s);
                } catch (e) {
                    reject(e);
                }
            });
        }
        await ensureSignalR();
        console.log(window.signalR); // phải hiện object
        var root = document.getElementById('chat-root');
        if (!root) { safeLog(2, 'chat-root not found, aborting init'); return; }

        // Read user object from localStorage as requested by the client
        var rawUser = null;
        try {
            rawUser = localStorage.getItem('user');
            safeLog(3, 'raw user from localStorage: ' + rawUser);
        } catch (e) { safeLog(4, 'error reading localStorage: ' + e.message); }

        if (!rawUser) {
            safeLog(5, 'no user in localStorage — chat will be hidden until login');
            // hide chat toggle if present
            var tb = document.getElementById('chat-toggle-btn'); if (tb) tb.style.display = 'none';
            return;
        }

        var parsedUser = null;
        try {
            parsedUser = JSON.parse(rawUser);
            safeLog(6, 'parsed user: ' + JSON.stringify(parsedUser));
        } catch (e) { safeLog(7, 'failed to parse user JSON: ' + e.message); }

        // normalize parsedUser id to number to avoid strict equality issues between string/number
        if (parsedUser && parsedUser.id) {
            parsedUser.id = Number(parsedUser.id);
        }
        var username = parsedUser && parsedUser.username ? parsedUser.username : null;
        var role = parsedUser && parsedUser.role ? parsedUser.role : null;
        var isAdmin = (role && role.toString().toLowerCase() === 'admin');
        safeLog(8, 'resolved username=' + username + ' role=' + role + ' isAdmin=' + isAdmin);

        if (!username) {
            safeLog(9, 'parsed user has no username — aborting chat init');
            var tb = document.getElementById('chat-toggle-btn'); if (tb) tb.style.display = 'none';
            return;
        }

        // If another page requested to open chat with a specific user (fallback), handle it
        try {
            var savedTarget = localStorage.getItem('chat_target_user');
            if (savedTarget) {
                try {
                    var t = JSON.parse(savedTarget);
                    if (t && t.id) {
                        // remove after reading
                        localStorage.removeItem('chat_target_user');
                        // open chat after connection is ready (openChatWith will wait for connectionStarted)
                        // but if openChatWith is not defined yet, queue by calling after a short delay
                        setTimeout(function(){ if(window.openChatWith) window.openChatWith(t.id, t.name); else console.log('openChatWith not ready'); }, 300);
                    }
                } catch (e) { safeLog(3.5, 'failed to parse saved chat target: ' + e); }
            }
        } catch (e) { safeLog(3.6, 'error reading saved chat target: ' + e); }

        var toggleBtn = document.getElementById('chat-toggle-btn');
        var chatWindow = document.getElementById('chat-window');
        var chatBody = document.getElementById('chat-body');
        var chatInput = document.getElementById('chat-input');
        var chatSend = document.getElementById('chat-send');
        var userList = document.getElementById('chat-user-list');
        var chatHeader = document.getElementById('chat-header');
        var badge = document.getElementById('chat-badge');

        if (!toggleBtn || !chatWindow || !chatBody || !chatInput || !chatSend) {
            safeLog(5, 'some chat elements missing, aborting');
            return;
        }

        safeLog(6, 'elements found, wiring events');

        // Ensure sidebar is visible for all users (we show partners for customers/sellers as well)
        try{
            var sidebarEl = document.getElementById('chat-sidebar');
            if(sidebarEl){ sidebarEl.classList.remove('hidden'); }
        }catch(e){ safeLog(6.1, 'error ensuring sidebar visible: ' + e); }

        // Basic state
        var conversations = {}; // { key: [ {from, text, time} ] }
        var displayNames = {}; // map key -> display name for list rendering
        // default to no active conversation so refreshParticipantList can pick a sensible one
        var activeUser = null;
        const ADMIN_ID = 11; // server admin user id (assumption: admin has id 1)
        var connection = null;
        var connectionStarted = false;

        var userId = parsedUser && parsedUser.id ? parsedUser.id : null;
        var adminId = 1; // assume admin has id = 1; change if different
        safeLog(10, 'userId=' + userId + ' adminId=' + adminId);

        // Expose a global helper so other UI (product detail) can open a chat with a specific user/seller
        window.openChatWith = function(otherUserId, otherUserName) {
            try {
                otherUserId = Number(otherUserId);
                if (isNaN(otherUserId)) { safeLog(5.1, 'openChatWith called with invalid id: ' + otherUserId); return; }
                var key = 'user_' + otherUserId;
                conversations[key] = conversations[key] || [];
                if (otherUserName) displayNames[key] = otherUserName;
                // show chat window
                if (chatWindow.classList.contains('hidden')) chatWindow.classList.remove('hidden');
                // set active and load conversation from server (if connected)
                activeUser = key;
                // if connected, try to load conversation history
                if (connectionStarted && connection) {
                    connection.invoke('GetConversation', parsedUser.id, otherUserId).then(function(result){
                        safeLog(70, 'openChatWith loaded conversation count=' + (result?result.length:0));
                        conversations[key] = (result || []).map(function(m){
                            var sid = (m.SenderId!==undefined && m.SenderId!==null)?m.SenderId:(m.senderId!==undefined?m.senderId:null);
                            sid = sid!==null?Number(sid):null;
                            var sname = m.SenderUsername || m.senderUsername || null;
                            var content = (m.Content!==undefined && m.Content!==null)?m.Content:(m.content!==undefined?m.content:(m.message!==undefined?m.message:''));
                            var ts = m.Timestamp || m.timestamp || m.time || new Date().toISOString();
                            return { from: (sid===parsedUser.id? parsedUser.username : (sname || ('user_'+sid))), text: content, time: ts };
                        });
                        // ensure display name for this conversation is set to the most recent sender that's not us
                        try {
                            var convArr = conversations[key] || [];
                            if (convArr.length) {
                                var last = convArr[convArr.length - 1];
                                if (last && last.from && last.from !== parsedUser.username) {
                                    displayNames[key] = last.from;
                                }
                            }
                        } catch (e) { safeLog(70.5, 'set displayNames after GetConversation failed: ' + e); }
                        persist();
                        renderUserList();
                        renderMessagesForActive();
                    }).catch(function(err){
                        safeLog(71, 'openChatWith GetConversation failed: ' + err);
                        renderUserList();
                        renderMessagesForActive();
                    });
                } else {
                    renderUserList();
                    renderMessagesForActive();
                }
            } catch(e){ safeLog(5.2, 'openChatWith error: ' + e); }
        };

        // load persisted users from sessionStorage (if any)
        try {
            var stored = sessionStorage.getItem('chat_conversations');
            safeLog(7, 'loaded sessionStorage item');
            if (stored) {
                conversations = JSON.parse(stored) || {};
                safeLog(8, 'parsed conversations: ' + Object.keys(conversations).length + ' keys');
            }

            // Sanitize any persisted conversation keys from previous runs
            Object.keys(conversations).forEach(function(k){
                if (k === 'admin') return;
                if (!k || typeof k !== 'string') { delete conversations[k]; return; }
                if (!k.startsWith('user_')) { delete conversations[k]; return; }
                var idPart = k.split('_')[1];
                if (!idPart || isNaN(Number(idPart))) { safeLog(8.1, 'removing invalid conversation key: ' + k); delete conversations[k]; }
            });
        } catch (e) { safeLog(9, 'failed to load/parse sessionStorage: ' + e.message); }

        // API base (backend runs on fixed port 5236, HTTP)
        const API_BASE = 'http://localhost:5236';

        // Setup SignalR connection
        try {
            try {
                await ensureSignalR();
            } catch (e) { safeLog(1.5, 'ensureSignalR failed: ' + e); }
            connection = new signalR.HubConnectionBuilder()
                .withUrl(API_BASE + '/hubs/chat')
                .withAutomaticReconnect()
                .build();

            connection.on('ReceiveMessage', function (dto) {
                safeLog(40, 'SignalR ReceiveMessage: ' + JSON.stringify(dto));
                // dto may use different casing; tolerate senderId/receiverId variants
                var senderId = (dto.SenderId !== undefined && dto.SenderId !== null) ? dto.SenderId : (dto.senderId !== undefined ? dto.senderId : null);
                var receiverId = (dto.ReceiverId !== undefined && dto.ReceiverId !== null) ? dto.ReceiverId : (dto.receiverId !== undefined ? dto.receiverId : null);
                if (senderId === null || receiverId === null) {
                    safeLog(40.1, 'ReceiveMessage missing sender/receiver ids, ignoring DTO: ' + JSON.stringify(dto));
                    return;
                }
                // normalize numeric ids
                senderId = Number(senderId);
                receiverId = Number(receiverId);
                var other = (senderId === Number(parsedUser.id)) ? receiverId : senderId;
                if (other === undefined || other === null || isNaN(other)) {
                    safeLog(40.2, 'Computed other is invalid, ignoring DTO');
                    return;
                }
                var otherKey = (other === ADMIN_ID) ? 'admin' : ('user_' + other);
                conversations[otherKey] = conversations[otherKey] || [];
                var fromName = dto.SenderUsername || dto.senderUsername || (senderId === Number(parsedUser.id) ? parsedUser.username : 'admin');
                // store display name for admin list
                // store display name for the conversation for all users when possible
                if (otherKey !== 'admin') {
                    if (senderId === Number(parsedUser.id)) {
                        // we are the sender; if we don't know the other user's name yet, refresh participant list to fetch usernames
                        if (!displayNames[otherKey]) {
                            try { refreshParticipantList(parsedUser.id); } catch (e) { safeLog(40.7, 'refreshParticipantList failed: ' + e); }
                        }
                        // keep existing or fallback placeholder
                        displayNames[otherKey] = displayNames[otherKey] || ('user_' + other);
                    } else {
                        // incoming message: dto.SenderUsername is the other participant's name
                        displayNames[otherKey] = dto.SenderUsername || dto.senderUsername || displayNames[otherKey] || ('user_' + other);
                    }
                } else {
                    // conversation with admin: set name to 'Admin' or keep existing
                    displayNames[otherKey] = displayNames[otherKey] || 'Admin';
                }
                // determine content in a tolerant way
                var content = (dto.Content !== undefined && dto.Content !== null) ? dto.Content : (dto.content !== undefined ? dto.content : (dto.message !== undefined ? dto.message : ''));
                // validate timestamp - fall back to now if invalid
                var ts = dto.Timestamp || dto.timestamp || dto.time || null;
                var parsedTs = (ts && !isNaN(Date.parse(ts))) ? ts : new Date().toISOString();
                // prevent duplicates: if the most recent message in this conversation
                // matches the incoming one (same text and same sender), treat as duplicate
                var conv = conversations[otherKey];
                var duplicateHandled = false;
                if (conv.length > 0) {
                    var lastMsg = conv[conv.length - 1];
                    try {
                        if (lastMsg && lastMsg.text === content && lastMsg.from === fromName) {
                            // update the timestamp to the server-provided one (or normalized)
                            lastMsg.time = parsedTs;
                            duplicateHandled = true;
                            safeLog(40.3, 'Duplicate message detected for ' + otherKey + ' — updated timestamp');
                        }
                    } catch (ex) { safeLog(40.4, 'duplicate check error: ' + ex); }
                }
                if (!duplicateHandled) {
                    conv.push({ from: fromName, text: content, time: parsedTs });
                }
                persist();
                // Refresh user list for all user types so everyone sees updated previews
                try { renderUserList(); } catch (e) { safeLog(40.5, 'renderUserList failed: ' + e); }
                // If no conversation is active yet, open this one
                if (!activeUser) {
                    try { setActiveUser((otherKey === 'admin') ? 'admin' : otherKey); } catch (e) { safeLog(40.6, 'setActiveUser failed: ' + e); }
                }
                // Append to active panel if this is the current active conversation
                if (activeUser === ((otherKey === 'admin') ? 'admin' : otherKey)) {
                    if (duplicateHandled) {
                        // re-render the active conversation so updated timestamp is shown
                        renderMessagesForActive();
                    } else {
                        appendMessageToBody(fromName || 'Unknown', content, parsedTs);
                    }
                }
                updateBadge();
            });

            connection.on('UserConnected', function (userId) {
                safeLog(41, 'SignalR UserConnected: ' + userId);
            });

            connection.start().then(function () {
                connectionStarted = true;
                safeLog(42, 'SignalR connected, connectionId=' + (connection.connectionId || '[unknown]'));
                // register current user id with hub
                connection.invoke('Register', parsedUser.id).then(function () {
                    safeLog(43, 'registered on hub as user ' + parsedUser.id);
                }).catch(function (err) { safeLog(44, 'register failed: ' + err.toString()); });

                // load conversation with admin
                connection.invoke('GetConversation', parsedUser.id, ADMIN_ID).then(function (result) {
                    safeLog(45, 'loaded conversation from server, count=' + (result ? result.length : 0));
                    if (result) {
                        var key = 'admin';
                            conversations[key] = result.map(function (m) {
                                var sid = (m.SenderId !== undefined && m.SenderId !== null) ? m.SenderId : (m.senderId !== undefined ? m.senderId : null);
                                sid = sid !== null ? Number(sid) : null;
                                var sname = m.SenderUsername || m.senderUsername || null;
                                var content = (m.Content !== undefined && m.Content !== null) ? m.Content : (m.content !== undefined ? m.content : (m.message !== undefined ? m.message : ''));
                                var ts = m.Timestamp || m.timestamp || m.time || new Date().toISOString();
                                return { from: (sid === Number(parsedUser.id) ? parsedUser.username : (sname || 'admin')), text: content, time: ts };
                            });
                        persist();
                        renderMessagesForActive();
                        updateBadge();
                    }
                }).catch(function (err) { safeLog(46, 'GetConversation failed: ' + err.toString()); });
            }).catch(function (err) { safeLog(47, 'SignalR connection failed: ' + err.toString()); });
        } catch (e) { safeLog(48, 'Error creating SignalR connection: ' + e.message); }

        function persist() {
            try {
                sessionStorage.setItem('chat_conversations', JSON.stringify(conversations));
                safeLog(10, 'persisted conversations to sessionStorage');
            } catch (e) { safeLog(11, 'persist error: ' + e.message); }
        }

        // Note: SignalR connection is created above (single shared connection). Duplicate builders removed to avoid overwriting the connection and losing connectionStarted flag.

        function renderUserList() {
            safeLog(12, 'renderUserList start');
            if (!userList) return;
            userList.innerHTML = '';
            var keys = Object.keys(conversations).sort(function (a, b) {
                // sort by most recent message timestamp (descending)
                try {
                    var aMsgs = conversations[a] || [];
                    var bMsgs = conversations[b] || [];
                    var aTs = 0;
                    var bTs = 0;
                    if (aMsgs.length) {
                        var at = aMsgs[aMsgs.length - 1] && aMsgs[aMsgs.length - 1].time;
                        aTs = at ? Date.parse(at) : 0;
                    }
                    if (bMsgs.length) {
                        var bt = bMsgs[bMsgs.length - 1] && bMsgs[bMsgs.length - 1].time;
                        bTs = bt ? Date.parse(bt) : 0;
                    }
                    return (bTs - aTs);
                } catch (e) {
                    return a.localeCompare(b);
                }
            });
            safeLog(13, 'users (sorted by latest): ' + keys.join(','));
            keys.forEach(function (user) {
                var li = document.createElement('li');
                li.className = 'chat-user-item';
                li.setAttribute('data-username', user);
                // choose a friendly display name: prefer displayNames map (from server list),
                // otherwise find the first message author that is NOT the admin (so we show the customer's name),
                // otherwise fallback to the key
                var displayName = displayNames[user] || null;
                if (!displayName) {
                    var convArr = conversations[user] || [];
                    // pick the most recent non-admin sender if possible (iterate from end)
                    for (var i = convArr.length - 1; i >= 0; i--) {
                        var candidate = convArr[i] && convArr[i].from;
                        if (candidate && candidate.toString().toLowerCase() !== 'admin' && candidate.toString().toLowerCase() !== 'you') {
                            displayName = candidate; break;
                        }
                    }
                }
                if (!displayName) displayName = user;
                // show the latest message (last element) as preview
                var lastText = '';
                try {
                    var convLatest = conversations[user] || [];
                    if (convLatest.length) {
                        var last = convLatest[convLatest.length - 1];
                        lastText = last && last.text ? last.text : '';
                    }
                } catch (e) { lastText = ''; }
                li.innerHTML = '<div class="chat-user-avatar">' + (displayName[0] || '?') + '</div><div class="chat-user-meta"><div class="chat-user-name">' + displayName + '</div><div class="chat-user-last">' + (lastText.length > 28 ? lastText.substring(0, 28) + '...' : lastText) + '</div></div>';
                // mark active
                if (user === activeUser) { li.classList.add('active'); }
                li.addEventListener('click', function () {
                    safeLog(14, 'clicked user ' + user);
                    // remove active class on all
                    Array.from(userList.children).forEach(function (n) { n.classList.remove('active'); });
                    li.classList.add('active');
                    setActiveUser(user);
                });
                userList.appendChild(li);
            });
            safeLog(15, 'renderUserList done');
        }

        function setActiveUser(user) {
            safeLog(16, 'setActiveUser to ' + user);
            activeUser = user;
            var titleName = 'Admin';
            if (isAdmin) {
                titleName = displayNames[user] || (conversations[user] && conversations[user][0] && conversations[user][0].from) || user;
            }
            chatHeader.textContent = isAdmin ? ('Chat with ' + titleName) : 'Chat with Admin';
            // If admin opened a user conversation, load full history from server
            if (isAdmin && connectionStarted && connection) {
                // expect user keys like 'user_5' or 'admin'
                if (user && user.indexOf('user_') === 0) {
                    var otherId = parseInt(user.split('_')[1]);
                    if (isNaN(otherId)) {
                        safeLog(60.1, 'setActiveUser: invalid otherId parsed from key ' + user);
                        renderMessagesForActive();
                        return;
                    }
                    connection.invoke('GetConversation', adminId, otherId).then(function (result) {
                        safeLog(60, 'loaded conversation for admin with user ' + otherId + ', count=' + (result ? result.length : 0));
                        conversations[user] = (result || []).map(function (m) {
                            var sid = (m.SenderId !== undefined && m.SenderId !== null) ? m.SenderId : (m.senderId !== undefined ? m.senderId : null);
                            sid = sid !== null ? Number(sid) : null;
                            var sname = m.SenderUsername || m.senderUsername || null;
                            var content = (m.Content !== undefined && m.Content !== null) ? m.Content : (m.content !== undefined ? m.content : (m.message !== undefined ? m.message : ''));
                            var ts = m.Timestamp || m.timestamp || m.time || new Date().toISOString();
                            return { from: (sid === adminId ? 'admin' : (sname || ('user_' + sid))), text: content, time: ts };
                        });
                        // set display name for this user conversation based on the most recent non-admin sender
                        try {
                            var convs = conversations[user] || [];
                            if (convs.length) {
                                var lastMsg = convs[convs.length - 1];
                                if (lastMsg && lastMsg.from && lastMsg.from !== 'admin') {
                                    displayNames[user] = lastMsg.from;
                                }
                            }
                        } catch (e) { safeLog(61.5, 'set displayNames after admin GetConversation failed: ' + e); }
                        persist();
                        renderMessagesForActive();
                    }).catch(function (err) {
                        safeLog(61, 'GetConversation (SignalR) failed for admin: ' + err);
                        // fallback to REST
                        fetch(API_BASE + '/api/chat/conversation?userAId=' + adminId + '&userBId=' + otherId).then(function (r) { if (!r.ok) throw new Error(r.statusText); return r.json(); }).then(function (result) {
                            conversations[user] = (result || []).map(function (m) {
                                var sid = (m.SenderId !== undefined && m.SenderId !== null) ? m.SenderId : (m.senderId !== undefined ? m.senderId : null);
                                sid = sid !== null ? Number(sid) : null;
                                var sname = m.SenderUsername || m.senderUsername || null;
                                var content = (m.Content !== undefined && m.Content !== null) ? m.Content : (m.content !== undefined ? m.content : (m.message !== undefined ? m.message : ''));
                                var ts = m.Timestamp || m.timestamp || m.time || new Date().toISOString();
                                return { from: (sid === adminId ? 'admin' : (sname || ('user_' + sid))), text: content, time: ts };
                            });
                            persist();
                            renderMessagesForActive();
                        }).catch(function (err2) { safeLog(62, 'GetConversation REST fallback failed: ' + err2); renderMessagesForActive(); });
                    });
                    return; // we'll render after async load
                }
            }
            renderMessagesForActive();
        }

        function renderMessagesForActive() {
            safeLog(17, 'renderMessagesForActive for ' + activeUser);
            chatBody.innerHTML = '';
            if (!activeUser) return;
            var msgs = conversations[activeUser] || [];
            msgs.forEach(function (m) {
                appendMessageToBody(m.from, m.text, m.time);
            });
            // scroll to bottom
            chatBody.scrollTop = chatBody.scrollHeight;
            safeLog(18, 'rendered ' + msgs.length + ' messages');
        }

        function appendMessageToBody(from, text, time) {
            safeLog(19, 'appendMessageToBody from=' + from + ' text=' + text);
            var row = document.createElement('div');
            row.className = 'msg-row ' + ((from === username) ? 'me' : '');
            var bubble = document.createElement('div');
            bubble.className = 'msg-bubble ' + ((from === username) ? 'me' : '');
            bubble.textContent = text;
            row.appendChild(bubble);
            var meta = document.createElement('div');
            meta.className = 'msg-meta';
            // Guard against invalid time values
            var dateObj = new Date(time);
            var timeLabel = isNaN(dateObj.getTime()) ? 'Unknown time' : dateObj.toLocaleTimeString();
            meta.textContent = (from === username ? 'You' : from) + ' • ' + timeLabel;
            row.appendChild(meta);
            chatBody.appendChild(row);
            safeLog(20, 'appended message element');
        }

        function sendMessageTo(user, text) {
            safeLog(21, 'sendMessageTo user=' + user + ' text=' + text);
            safeLog(21.1, 'connectionStarted=' + connectionStarted + ' parsedUserId=' + (parsedUser && parsedUser.id));
            if (!user) { safeLog(22, 'no target user'); return; }
            // Use SignalR to send the message to server
            // determine numeric target id for 'admin' or 'user_<id>' keys (use same logic for fallback)
            var targetId = null;
            if (user === 'admin') targetId = ADMIN_ID;
            else if (typeof user === 'string' && user.indexOf('user_') === 0) {
                var parts = user.split('_');
                targetId = parseInt(parts[1]);
                if (isNaN(targetId)) targetId = null;
            }

            // Prevent admin from accidentally sending to themselves (admin -> admin)
            if (isAdmin && targetId === ADMIN_ID) {
                safeLog(21.5, 'Admin attempted to send a message to admin conversation; select a user first.');
                return;
            }

            var msg = { from: username, text: text, time: new Date().toISOString() };
            conversations[user] = conversations[user] || [];
            conversations[user].push(msg); // optimistic add
            persist();
            if (activeUser === user) appendMessageToBody(msg.from, msg.text, msg.time);
            if (connectionStarted && parsedUser && parsedUser.id) {
                if (targetId) {
                    connection.invoke('SendMessage', parsedUser.id, targetId, text).catch(function (err) { safeLog(40, 'SendMessage failed: ' + err); });
                } else {
                    safeLog(41, 'unknown target id for user key ' + user);
                }
            } else {
                safeLog(42, 'no connection or user id, message not sent to server — falling back to REST POST');
                // Fallback: POST to REST endpoint to ensure message is persisted
                try {
                    fetch(API_BASE + '/api/chat/messages', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ senderId: parsedUser && parsedUser.id, receiverId: targetId, content: text })
                    }).then(function (resp) {
                        if (!resp.ok) return resp.text().then(function (t) { throw new Error(t || resp.statusText); });
                        return resp.json();
                    }).then(function (saved) {
                        safeLog(43, 'Fallback POST saved message: ' + JSON.stringify(saved));
                        // update local conversation with saved data (ensure timestamp from server)
                        conversations['admin'] = conversations['admin'] || [];
                        conversations['admin'].push({ from: username, text: saved.content, time: saved.timestamp });
                        persist();
                        if (activeUser === 'admin') appendMessageToBody(username, saved.content, saved.timestamp);
                        updateBadge();
                    }).catch(function (err) { safeLog(44, 'Fallback POST failed: ' + err); });
                } catch (e) { safeLog(45, 'Fallback POST exception: ' + e.message); }
            }
            updateBadge();
        }

        chatSend.addEventListener('click', function () {
            safeLog(25, 'chatSend clicked');
            var text = chatInput.value && chatInput.value.trim();
            if (!text) { safeLog(26, 'empty input'); return; }
            sendMessageTo(activeUser, text);
            chatInput.value = '';
        });

        chatInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                chatSend.click();
            }
        });

        toggleBtn.addEventListener('click', function () {
            safeLog(27, 'toggleBtn clicked');
            if (chatWindow.classList.contains('hidden')) {
                chatWindow.classList.remove('hidden');
                safeLog(28, 'chatWindow shown');
            } else {
                chatWindow.classList.add('hidden');
                safeLog(29, 'chatWindow hidden');
            }
        });

        function updateBadge() {
            safeLog(30, 'updateBadge start');
            if (!badge) return;
            var totalUnread = 0;
            Object.keys(conversations).forEach(function (u) {
                var msgs = conversations[u] || [];
                // naive: count messages not from admin if user is admin? Keep simple demo
                totalUnread += msgs.length;
            });
            badge.style.display = totalUnread > 0 ? 'none' : 'none';
            badge.textContent = '' + totalUnread;
            safeLog(31, 'badge set to ' + totalUnread);
        }

        // For all users (admin, seller, customer): fetch chat partners and seed the Active Conversations list
        async function refreshParticipantList(userId) {
            try {
                safeLog(32, 'refreshParticipantList for user ' + userId);
                var resp = await fetch(API_BASE + '/api/chat/users?adminId=' + userId);
                if (!resp.ok) { safeLog(33, 'failed to fetch chat users: ' + resp.status); return; }
                var list = await resp.json();
                safeLog(34, 'fetched chat users: ' + JSON.stringify(list));
                list.forEach(function (u) {
                    var uid = Number(u.userId);
                    if (isNaN(uid)) { safeLog(34.1, 'skipping chat user with invalid id: ' + JSON.stringify(u)); return; }
                    // skip self if present
                    if (uid === Number(userId)) { safeLog(34.2, 'skipping self in participant list: ' + uid); return; }
                    var key = 'user_' + uid;
                    conversations[key] = conversations[key] || [];
                    var displayName = u.username || ('user_' + uid);
                    // remember display name for rendering
                    displayNames[key] = displayName;
                    // seed with last message if present
                    if (u.lastMessage) {
                        var ts = u.lastTimestamp || new Date().toISOString();
                        var parsedTs = (!isNaN(Date.parse(ts))) ? ts : new Date().toISOString();
                        conversations[key].push({ from: displayName, text: u.lastMessage, time: parsedTs });
                    }
                });
                renderUserList();
                var keys = Object.keys(conversations).filter(function(k){ return k.indexOf('user_')===0; });
                if (keys.length && !activeUser) { setActiveUser(keys[0]); }
            } catch (e) { safeLog(35, 'refreshParticipantList error: ' + e.message); }
        }

        // call for the current logged-in user (works for admin, seller, or customer)
        refreshParticipantList(parsedUser.id);

        safeLog(34, 'chat init complete');
    });

})();
