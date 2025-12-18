using DiscordClone.Data;
using DiscordClone.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordClone.Services
{
    public class FriendsService
    {
        private readonly ApplicationDbContext _context;

        public FriendsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public string[] SplitName(string text)
        {
            string[] parts = text.Split('#');
            return new string[] { parts[0], parts[1] };
        }

        public UserProfile GetUserProfile(string userId)
        {
            return _context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
        }

        public int GetUserIntId(string userId)
        {   if(_context.UserProfiles.FirstOrDefault(x => x.UserId == userId) == null)
                return -1;
            return _context.UserProfiles.FirstOrDefault(x => x.UserId == userId).Id;
        }

        public List<UserProfile> GetUserFriends(string userId)
        {
            var id = GetUserIntId(userId);
            if (id == null) return new List<UserProfile>();

            var friends = _context.Friendships
                .Where(f => (f.UserId == id || f.FriendId == id) &&
                           (f.Status == Status.Accepted || f.Status == Status.Chating))
                .Select(f => f.UserId == id ? f.FriendId : f.UserId) 
                .Join(_context.UserProfiles,
                      friendId => friendId,
                      u => u.Id,
                      (friendId, profile) => profile)
                .ToList();

            return friends;
        }

        public List<UserProfile> GetPendingFriendRequests(string userId)
        {
            var UserId = GetUserIntId(userId);
            if (UserId == null) return null;

            return _context.Friendships
                .Where(f => f.FriendId == UserId && f.Status == Status.Pending)
                .Join(_context.UserProfiles,
                      f => f.UserId,
                      u => u.Id,
                      (f, u) => u)
                .ToList();
        }

        public void AddFriend(string userId, string usernameInput)
        {
            var userProfile = _context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
            if (userProfile == null || string.IsNullOrEmpty(usernameInput)) return;

            try
            {
                var nameTag = SplitName(usernameInput);
                var friend = _context.UserProfiles
                    .FirstOrDefault(f => f.Username == nameTag[0] && f.Tag == int.Parse(nameTag[1]));

                _context.Friendships.Add(new Friendship
                {
                    UserId = userProfile.Id,
                    FriendId = friend.Id,
                    Status = Status.Pending
                });

                _context.SaveChanges();


            }
            catch
            {
                return;
            }

        }

        public void AcceptFriendship(string userId, int friendId)
        {
            var userProfile = _context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
            if (userProfile == null) return;

            var friendship = _context.Friendships.FirstOrDefault(f =>
                f.FriendId == userProfile.Id  && f.UserId == friendId);

            if (friendship != null)
            {
                friendship.Status = Status.Accepted;
                _context.SaveChanges();

                //_context.Friendships.Add(new Friendship
                //{
                //    UserId = userProfile.Id,
                //    FriendId = friendId,
                //    Status = Status.Accepted
                //});


            }
        }

        public void CreateFriendshipChannel(int UserId , int FriendId)
        {
            if (ifChating(UserId, FriendId)) return;
            _context.Friendships.Add(
                new Friendship
                {
                    UserId = UserId,
                    FriendId = FriendId,
                    Status = Status.Chating
                });
            _context.SaveChanges();
        }

        public Boolean ifChating(int UserId, int FriendId)
        {
             var Chating = _context.Friendships
                .FirstOrDefault(f => (f.UserId == UserId && f.FriendId == FriendId) && (f.Status == Status.Chating || f.Status == Status.Accepted));
            if (Chating == null)
            {
                Chating = _context.Friendships
                .FirstOrDefault(f => (f.UserId == FriendId  && f.FriendId == UserId) && (f.Status == Status.Chating || f.Status == Status.Accepted));
            }
            if (Chating != null)
                return true;
            return false;
        }

        public int GetFriendshipId(int UserId, int FriendId)
        {
            var friendship = _context.Friendships
                .FirstOrDefault(f => (f.UserId == UserId && f.FriendId == FriendId) && (f.Status == Status.Chating || f.Status == Status.Accepted));
            if (friendship == null)
                friendship = _context.Friendships
                .FirstOrDefault(f => (f.UserId == FriendId && f.FriendId == UserId) && (f.Status == Status.Chating || f.Status == Status.Accepted));
            if (friendship != null)
            {
                return friendship.Id;
            }
            return -1;
        }
    }
}
