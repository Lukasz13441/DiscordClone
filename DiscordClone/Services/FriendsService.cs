using DiscordClone.Data;
using DiscordClone.Models;

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
        {
            return _context.UserProfiles.FirstOrDefault(x => x.UserId == userId).Id;
        }

        public List<UserProfile> GetUserFriends(string userId)
        {
            var UserId = GetUserIntId(userId);
            if (UserId == null) return null;

            return _context.Friendships
                .Where(f => f.UserId == UserId && f.Status == Status.Accepted)
                .Join(_context.UserProfiles,
                      f => f.FriendId,
                      u => u.Id,
                      (f, u) => u)
                .ToList();
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
                f.FriendId == userProfile.Id && f.UserId == friendId);

            if (friendship != null)
            {
                friendship.Status = Status.Accepted;

                _context.Friendships.Add(new Friendship
                {
                    UserId = userProfile.Id,
                    FriendId = friendId,
                    Status = Status.Accepted
                });

                _context.SaveChanges();
            }
        }
    }
}
