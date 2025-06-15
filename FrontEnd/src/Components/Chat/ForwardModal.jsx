import React, {useState, useEffect} from 'react';
import {Search, X, Send, Users, User} from 'lucide-react';

const ForwardModal = ({isOpen, onClose, onForward, chatRooms = [], currentRoomId, messageToForward}) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedRooms, setSelectedRooms] = useState([]);
  const [isSearchingUsers, setIsSearchingUsers] = useState(false);
  const [searchResults, setSearchResults] = useState([]);
  const [isLoading, setIsLoading] = useState(false);

  // Filter existing chat rooms
  const filteredRooms = chatRooms.filter((room) => room.id !== currentRoomId && room.name.toLowerCase().includes(searchQuery.toLowerCase()));

  // Search for new users
  useEffect(() => {
    const searchUsers = async () => {
      if (searchQuery.length < 2) {
        setSearchResults([]);
        return;
      }

      setIsSearchingUsers(true);
      try {
        const response = await fetch(`/api/users/search?q=${encodeURIComponent(searchQuery)}`);
        const users = await response.json();

        // Filter out users who already have chats
        const existingUserIds = chatRooms.filter((room) => !room.isGroup).map((room) => room.otherUserId);

        const newUsers = users.filter((user) => !existingUserIds.includes(user.id));
        setSearchResults(newUsers);
      } catch (error) {
        console.error('Error searching users:', error);
      } finally {
        setIsSearchingUsers(false);
      }
    };

    const debounceTimer = setTimeout(searchUsers, 300);
    return () => clearTimeout(debounceTimer);
  }, [searchQuery, chatRooms]);

  const handleRoomSelect = (roomId) => {
    setSelectedRooms((prev) => (prev.includes(roomId) ? prev.filter((id) => id !== roomId) : [...prev, roomId]));
  };

  const handleUserSelect = async (user) => {
    // Create new chat room with user
    try {
      const response = await fetch('/api/chat/rooms', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({
          name: `${user.firstName} ${user.lastName}`,
          isGroup: false,
          memberIds: [user.id],
        }),
      });

      const newRoom = await response.json();
      handleRoomSelect(newRoom.id);

      // Add to filtered rooms for display
      filteredRooms.push({
        ...newRoom,
        isNew: true,
      });
    } catch (error) {
      console.error('Error creating chat room:', error);
    }
  };

  const handleForward = async () => {
    if (selectedRooms.length === 0) return;

    setIsLoading(true);
    try {
      for (const roomId of selectedRooms) {
        await onForward(messageToForward.id, roomId);
      }
      onClose();
    } catch (error) {
      console.error('Error forwarding message:', error);
    } finally {
      setIsLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg w-full max-w-md mx-4 max-h-[80vh] flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b">
          <h3 className="text-lg font-semibold">Forward Message</h3>
          <button onClick={onClose} className="p-1 hover:bg-gray-100 rounded-full transition">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Search */}
        <div className="p-4 border-b">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search chats or users..."
              className="w-full pl-10 pr-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        {/* Chat List */}
        <div className="flex-1 overflow-y-auto">
          {/* Existing Chats */}
          {filteredRooms.length > 0 && (
            <div>
              <div className="px-4 py-2 text-sm text-gray-500 font-medium">Chats</div>
              {filteredRooms.map((room) => (
                <label key={room.id} className="flex items-center px-4 py-3 hover:bg-gray-50 cursor-pointer">
                  <input type="checkbox" checked={selectedRooms.includes(room.id)} onChange={() => handleRoomSelect(room.id)} className="mr-3" />
                  <div className="flex items-center flex-1">
                    <div className="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center mr-3">{room.isGroup ? <Users className="w-5 h-5 text-gray-500" /> : <User className="w-5 h-5 text-gray-500" />}</div>
                    <div>
                      <div className="font-medium">{room.name}</div>
                      {room.lastMessageContent && <div className="text-sm text-gray-500 truncate max-w-xs">{room.lastMessageContent}</div>}
                    </div>
                  </div>
                  {room.isNew && <span className="text-xs text-blue-500 ml-2">New</span>}
                </label>
              ))}
            </div>
          )}

          {/* Search Results - New Users */}
          {searchResults.length > 0 && (
            <div>
              <div className="px-4 py-2 text-sm text-gray-500 font-medium">New Conversations</div>
              {searchResults.map((user) => (
                <div key={user.id} onClick={() => handleUserSelect(user)} className="flex items-center px-4 py-3 hover:bg-gray-50 cursor-pointer">
                  <div className="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center mr-3">
                    {user.avatar ? <img src={user.avatar} alt={user.firstName} className="w-10 h-10 rounded-full" /> : <User className="w-5 h-5 text-gray-500" />}
                  </div>
                  <div className="flex-1">
                    <div className="font-medium">
                      {user.firstName} {user.lastName}
                    </div>
                    <div className="text-sm text-gray-500">{user.email}</div>
                  </div>
                  <div className="text-sm text-blue-500">Start chat</div>
                </div>
              ))}
            </div>
          )}

          {/* Loading */}
          {isSearchingUsers && (
            <div className="text-center py-4">
              <div className="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-blue-500"></div>
            </div>
          )}

          {/* No Results */}
          {searchQuery && !isSearchingUsers && filteredRooms.length === 0 && searchResults.length === 0 && <div className="text-center py-8 text-gray-500">No chats or users found</div>}
        </div>

        {/* Footer */}
        <div className="p-4 border-t">
          <button
            onClick={handleForward}
            disabled={selectedRooms.length === 0 || isLoading}
            className="w-full bg-blue-500 text-white py-2 rounded-lg hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed flex items-center justify-center gap-2"
          >
            {isLoading ? (
              <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
            ) : (
              <>
                <Send className="w-4 h-4" />
                Forward to {selectedRooms.length} chat{selectedRooms.length !== 1 ? 's' : ''}
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ForwardModal;
