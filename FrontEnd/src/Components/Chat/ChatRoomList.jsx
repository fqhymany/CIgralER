import React, {useState, useEffect, useRef, useMemo} from 'react';
import {ListGroup, Button, Form, Spinner, Badge} from 'react-bootstrap';
import {BiSearch} from 'react-icons/bi';
import {BsPeopleFill, BsFillPersonFill} from 'react-icons/bs';
import {useChat} from '../../hooks/useChat';
import {chatApi} from '../../services/chatApi';
import {useQueryClient} from '@tanstack/react-query';

const ChatRoomList = ({rooms, currentRoom, onRoomSelect, onNewRoom, isLoading}) => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [allUsers, setAllUsers] = useState([]);
  const [isLoadingUsers, setIsLoadingUsers] = useState(false);
  const [isSearchFocused, setIsSearchFocused] = useState(false);
  const {createChatRoom, rooms: contextRooms} = useChat();
  const searchInputRef = useRef(null);

  // فیلتر کردن چت‌ها بر اساس عبارت جستجو
  const filteredRooms = rooms.filter((room) => room.name.toLowerCase().includes(searchTerm.toLowerCase()) || (room.description && room.description.toLowerCase().includes(searchTerm.toLowerCase())));

  // فیلتر کردن کاربران بر اساس عبارت جستجو
  const filteredUsers = allUsers.filter((user) => user.fullName.toLowerCase().includes(searchTerm.toLowerCase()) || user.userName.toLowerCase().includes(searchTerm.toLowerCase()));

  // حذف کاربرانی که قبلاً چت با آنها وجود دارد
  const usersWithoutExistingChats = filteredUsers.filter((user) => {
    return !rooms.some((room) => !room.isGroup && (room.name === user.fullName || (room.members && room.members.some((member) => member.id === user.id))));
  });

  const groupRoomsByType = (rooms) => {
    const personal = [];
    const groups = [];
    const support = [];

    rooms.forEach((room) => {
      if (room.chatRoomType === 2) {
        // Support type
        support.push(room);
      } else if (room.isGroup) {
        groups.push(room);
      } else {
        personal.push(room);
      }
    });

    return {personal, groups, support};
  };

  const groupedRooms = groupRoomsByType(filteredRooms);

  // دریافت لیست کاربران هنگام فوکوس روی کادر جستجو
  // دریافت لیست کاربران هنگام فوکوس روی کادر جستجو
  useEffect(() => {
    const fetchAllUsers = async () => {
      if (isSearchFocused) {
        setIsLoadingUsers(true);
        try {
          // به جای getAllUsers از searchUsers با یک رشته خالی استفاده می‌کنیم
          const users = await chatApi.searchUsers('');
          setAllUsers(users || []);
        } catch (error) {
          console.error('Error fetching all users:', error);
        } finally {
          setIsLoadingUsers(false);
        }
      } else {
        // پاک کردن لیست کاربران هنگام خروج از حالت جستجو
        setAllUsers([]);
      }
    };

    fetchAllUsers();
  }, [isSearchFocused]);

  // جستجوی کاربران با تایپ در کادر جستجو
  useEffect(() => {
    if (!isSearchFocused) return;

    const delayDebounceFn = setTimeout(async () => {
      if (searchTerm.trim().length >= 2) {
        setIsLoadingUsers(true);
        try {
          const users = await chatApi.searchUsers(searchTerm);
          // ترکیب نتایج جستجو با کاربران موجود و حذف موارد تکراری
          const uniqueUsers = [...allUsers];
          users.forEach((user) => {
            if (!uniqueUsers.some((u) => u.id === user.id)) {
              uniqueUsers.push(user);
            }
          });
          setAllUsers(uniqueUsers);
        } catch (error) {
          console.error('Error searching users:', error);
        } finally {
          setIsLoadingUsers(false);
        }
      }
    }, 500);

    return () => clearTimeout(delayDebounceFn);
  }, [searchTerm, isSearchFocused, allUsers]);

  const handleUserSelectForChat = async (selectedUser) => {
    // بررسی وجود چت خصوصی با کاربر انتخاب شده
    const existingRoom = contextRooms.find((room) => !room.isGroup && ((room.members && room.members.some((member) => member.id === selectedUser.id) && room.members.length === 2) || room.name === selectedUser.fullName));

    if (existingRoom) {
      onRoomSelect(existingRoom);
    } else {
      try {
        // ایجاد چت خصوصی جدید
        const newRoomDto = await createChatRoom({
          name: selectedUser.fullName,
          isGroup: false,
          memberIds: [selectedUser.id],
        });
        if (newRoomDto) {
          // اضافه کردن اتاق جدید به لیست اتاق‌ها در کانتکست
          // این خط را اضافه کنید:
          queryClient.invalidateQueries(['chatRooms']);

          // سپس انتخاب اتاق جدید
          onRoomSelect(newRoomDto);
        }
      } catch (error) {
        console.error('Error creating chat room:', error);
      }
    }

    // پاک کردن کادر جستجو و خارج شدن از حالت جستجو
    setSearchTerm('');
    setIsSearchFocused(false);
    if (searchInputRef.current) {
      searchInputRef.current.blur();
    }
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (days === 0) {
      return date.toLocaleTimeString('fa-IR', {
        hour: '2-digit',
        minute: '2-digit',
      });
    } else if (days === 1) {
      return 'دیروز';
    } else if (days < 7) {
      return `${days} روز پیش`;
    } else {
      return date.toLocaleDateString('fa-IR');
    }
  };

  return (
    <div className="d-flex flex-column h-100">
      <div className="p-3 border-bottom">
        <div className="d-flex gap-2 mb-2">
          {/* کادر جستجوی ادغام شده */}
          <div className="position-relative flex-grow-1">
            <Form.Control
              ref={searchInputRef}
              type="text"
              placeholder="جستجوی چت‌ها و کاربران..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onFocus={() => setIsSearchFocused(true)}
              onBlur={() => setTimeout(() => setIsSearchFocused(false), 200)} // تاخیر برای امکان کلیک روی نتایج
              size="sm"
              className="search-input pe-4"
            />
            <BiSearch className="position-absolute" style={{top: '7px', left: '10px', opacity: 0.5}} />
            {isLoadingUsers && <Spinner animation="border" size="sm" className="position-absolute" style={{top: '7px', right: '10px', width: '15px', height: '15px'}} />}
          </div>

          {/* دکمه ایجاد گروه جدید */}
          <Button variant="primary" size="sm" onClick={onNewRoom} className="flex-shrink-0" title="ایجاد گروه جدید">
            <BsPeopleFill size={18} /> <span className="d-none d-md-inline">گروه</span>
          </Button>
        </div>
      </div>

      {/* لیست چت‌ها و کاربران */}
      <div className="flex-grow-1 overflow-auto">
        {isLoading && !isLoadingUsers ? (
          <div className="d-flex justify-content-center p-4">
            <Spinner animation="border" size="sm" />
          </div>
        ) : filteredRooms.length === 0 && (!isSearchFocused || usersWithoutExistingChats.length === 0) ? (
          <div className="text-center p-4 text-muted">{searchTerm ? 'چت یا کاربری پیدا نشد' : 'هنوز چتی ندارید'}</div>
        ) : (
          <ListGroup variant="flush">
            {/* نمایش چت‌های موجود */}
            {filteredRooms.length > 0 && (
              <>
                <ListGroup.Item className="border-0 border-bottom rounded-0 bg-light">
                  <small className="text-muted">چت‌های موجود</small>
                </ListGroup.Item>
                {filteredRooms.map((room) => (
                  <ListGroup.Item key={room.id} action active={currentRoom?.id === room.id} onClick={() => onRoomSelect(room)} className="border-0 border-bottom rounded-0 chat-room-list-item">
                    <div className="d-flex flex-row-reverse align-items-center">
                      {/* آواتار چت */}
                      <div className="me-3 flex-shrink-0 position-relative">
                        {room.avatar ? (
                          <img src={room.avatar} alt={room.name} className="rounded-circle" style={{width: '45px', height: '45px'}} />
                        ) : (
                          <div className={`rounded-circle ${room.isGroup ? 'bg-success' : 'bg-primary'} text-white d-flex align-items-center justify-content-center`} style={{width: '45px', height: '45px'}}>
                            {room.isGroup ? <BsPeopleFill size={20} /> : <BsFillPersonFill size={20} />}
                          </div>
                        )}
                        {room.unreadCount > 0 && (
                          <Badge
                            bg="primary"
                            pill
                            className="position-absolute top-0 start-100 translate-middle p-1 border border-light rounded-circle"
                            style={{fontSize: '0.6rem', lineHeight: '1', minWidth: '18px', minHeight: '18px', display: 'flex', alignItems: 'center', justifyContent: 'center'}}
                            title={`${room.unreadCount} پیام خوانده نشده`}
                          >
                            {room.unreadCount > 9 ? '9+' : room.unreadCount}
                          </Badge>
                        )}
                      </div>

                      {/* اطلاعات چت */}
                      <div className="flex-grow-1 min-width-0">
                        <div className="d-flex justify-content-between align-items-start">
                          <h6 className={`mb-0 text-truncate fw-semibold ${room.unreadCount > 0 && currentRoom?.id !== room.id ? 'text-primary' : ''}`}>{room.name}</h6>
                          {room.lastMessageTime && <small className="text-muted flex-shrink-0 ms-2">{formatDate(room.lastMessageTime)}</small>}
                        </div>

                        <div className="d-flex justify-content-between align-items-center">
                          <small className={`text-muted text-truncate ${room.unreadCount > 0 && currentRoom?.id !== room.id ? 'fw-bold' : ''}`} style={{fontSize: '0.8rem'}}>
                            {room.lastMessageSenderFullName && room.lastMessageContent ? `${room.lastMessageSenderFullName}: ${room.lastMessageContent}` : room.description || (room.isGroup ? 'گروه' : 'چت شخصی')}
                          </small>
                        </div>
                      </div>
                    </div>
                  </ListGroup.Item>
                ))}
              </>
            )}

            {/* Support Chats Section */}
            {groupedRooms.support.length > 0 && (
              <>
                <ListGroup.Item className="border-0 border-bottom rounded-0 bg-light">
                  <small className="text-muted d-flex align-items-center">
                    <i className="bi bi-headset me-2"></i>
                    پشتیبانی
                  </small>
                </ListGroup.Item>
                {groupedRooms.support.map((room) => (
                  <ListGroup.Item key={room.id} action active={currentRoom?.id === room.id} onClick={() => handleRoomSelect(room)} className="border-0 border-bottom rounded-0 chat-room-list-item position-relative">
                    <div className="d-flex flex-row-reverse align-items-center">
                      <div className="me-3 flex-shrink-0 position-relative">
                        <div className="rounded-circle bg-warning text-white d-flex align-items-center justify-content-center" style={{width: '45px', height: '45px'}}>
                          <i className="bi bi-headset" style={{fontSize: '20px'}}></i>
                        </div>
                        {room.unreadCount > 0 && (
                          <Badge
                            bg="primary"
                            pill
                            className="position-absolute top-0 start-100 translate-middle p-1 border border-light rounded-circle"
                            style={{fontSize: '0.6rem', lineHeight: '1', minWidth: '18px', minHeight: '18px'}}
                          >
                            {room.unreadCount > 9 ? '9+' : room.unreadCount}
                          </Badge>
                        )}
                      </div>
                      <div className="flex-grow-1 min-width-0">
                        <div className="d-flex justify-content-between align-items-start">
                          <h6 className={`mb-0 text-truncate fw-semibold ${room.unreadCount > 0 ? 'text-primary' : ''}`}>{room.name}</h6>
                          {room.lastMessageTime && <small className="text-muted flex-shrink-0 ms-2">{formatDate(room.lastMessageTime)}</small>}
                        </div>
                        <div className="d-flex justify-content-between align-items-center">
                          <small className={`text-muted text-truncate ${room.unreadCount > 0 ? 'fw-bold' : ''}`}>{room.lastMessageContent || 'پشتیبانی آنلاین'}</small>
                        </div>
                      </div>
                    </div>
                  </ListGroup.Item>
                ))}
              </>
            )}

            {/* نمایش کاربران بدون چت موجود - فقط در حالت جستجو */}
            {isSearchFocused && usersWithoutExistingChats.length > 0 && (
              <>
                <ListGroup.Item className="border-0 border-bottom rounded-0 bg-light">
                  <small className="text-muted">کاربران</small>
                </ListGroup.Item>
                {usersWithoutExistingChats.map((user) => (
                  <ListGroup.Item key={`user-${user.id}`} action onClick={() => handleUserSelectForChat(user)} className="border-0 border-bottom rounded-0 chat-room-list-item">
                    <div className="d-flex flex-row-reverse align-items-center">
                      <div className="me-3 flex-shrink-0">
                        {user.avatar ? (
                          <img src={user.avatar} alt={user.userName} className="rounded-circle" style={{width: '45px', height: '45px'}} />
                        ) : (
                          <div className="rounded-circle bg-secondary text-white d-flex align-items-center justify-content-center" style={{width: '45px', height: '45px'}}>
                            <BsFillPersonFill size={20} />
                          </div>
                        )}
                      </div>
                      <div className="flex-grow-1 min-width-0">
                        <h6 className="mb-0 text-truncate fw-semibold">{user.fullName}</h6>
                        <small className="text-muted text-truncate" style={{fontSize: '0.8rem'}}>
                          {user.userName || 'کاربر'}
                        </small>
                      </div>
                    </div>
                  </ListGroup.Item>
                ))}
              </>
            )}
          </ListGroup>
        )}
      </div>
    </div>
  );
};

export default ChatRoomList;
