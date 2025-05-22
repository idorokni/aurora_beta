using Aurora.Server.Communication.DataStruct;
using Aurora.Server.Communication.Services;
using Aurora.Server.Database.Data;
using Aurora.Server.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.Managers
{
        internal class DatabaseManager
        {
            private static DatabaseManager _instance;
            public static DatabaseManager Instance
            {
                get
                {
                    _instance ??= new DatabaseManager();
                    return _instance;
                }
            }

            public async Task<bool> UserExists(string username)
            {
                try
                {
                    using (var _db = new AuroraDB())
                    {
                        return await _db.Users.AnyAsync(u => u.Username == username);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public async Task<(ClientUserData, string, int)> AddUser(string username, string password, string email)
            {
                try
                {
                    using (var _db = new AuroraDB())
                    {
                        var user = new User
                        {
                            Username = username,
                            Password = password,
                            Email = email,
                            ProfileImagePath = string.Empty,
                            Bio = "This user has not set a bio yet.",
                            Followers = 0,
                            Following = 0,
                            JoinDate = DateTime.Now.ToString("MM/dd/yyyy"),
                            Birthday = "This user has not set a birthday yet."
                        };
                        _db.Users.Add(user);

                        await _db.SaveChangesAsync();

                        return (new ClientUserData
                        {
                            Username = user.Username,
                            Bio = user.Bio,
                            Email = user.Email,
                            Followers = user.Followers,
                            Following = user.Following,
                            JoinDate = user.JoinDate,
                            Birthday = user.Birthday,
                            ProfilePicture = Encoding.UTF8.GetBytes(user.ProfileImagePath)
                        }, user.ProfileImagePath, user.UserID);
                    }
                }
                catch (Exception ex)
                {
                    return (null, string.Empty, 0);
                }
            }

            public async Task RemoveUser(string username)
            {
                using (var _db = new AuroraDB())
                {
                    _db.Users.Remove(await _db.Users.FirstOrDefaultAsync(u => u.Username == username));
                }
            }

            public async Task<bool> checkIfPasswordsMatch(string username, string password)
            {
                using (var _db = new AuroraDB())
                {
                    return (await _db.Users.FirstOrDefaultAsync(u => u.Username == username)).Password == password;
                }
            }

            public async Task AddPost(int userID, string description, string postPath)
            {
                using (var _db = new AuroraDB())
                {
                    _db.Posts.Add(new Post
                    {
                        UserID = userID,
                        Description = description,
                        PostPath = postPath,
                        AmountLikes = 0,
                        AmountDislikes = 0,
                        AmountSuperLikes = 0
                    });

                    try
                    {
                        await _db.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            public async Task<UserData> GetUser(int userID)
            {
                using (var _db = new AuroraDB())
                {
                    var user = await _db.Users.FindAsync(userID);
                    return new UserData
                    {
                        Username = user.Username,
                        Bio = user.Bio,
                        Followers = user.Followers,
                        Following = user.Following,
                        Email = user.Email,
                        Birthday = user.Birthday,
                        JoinDate = user.JoinDate,
                        ProfilePicture = user.ProfileImagePath == string.Empty
                            ? string.Empty
                            : await ImageStorageService.GetImageAsync(user.ProfileImagePath)
                    };
                }
            }

            public async Task<(UserData, string, int)> GetUser(string username)
            {
                using (var _db = new AuroraDB())
                {
                    var user = _db.Users.FirstOrDefault(u => u.Username == username);
                    if (user == null)
                    {
                        throw new Exception("User not found.");
                    }

                    var profileImage = string.IsNullOrEmpty(user.ProfileImagePath)
                        ? string.Empty
                        : await ImageStorageService.GetImageAsync(user.ProfileImagePath);

                    return (
                        new UserData
                        {
                            Username = user.Username,
                            Bio = user.Bio,
                            Followers = user.Followers,
                            Following = user.Following,
                            Email = user.Email,
                            Birthday = user.Birthday,
                            JoinDate = user.JoinDate,
                            ProfilePicture = profileImage
                        },
                        user.ProfileImagePath,
                        user.UserID
                    );
                }
            }

            public async Task UpdateUser(string oldUsername, string email, string bio, string birthday, string imagePath)
            {
                try
                {
                    using (var _db = new AuroraDB())
                    {
                        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == oldUsername);
                        user.Email = email;
                        user.Bio = bio;
                        user.Birthday = birthday;
                        user.ProfileImagePath = imagePath;

                        await _db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    return;
                }
            }

            public List<SearchData> SearchUsers(string query)
            {
                using (var _db = new AuroraDB())
                {
                    return _db.Users.Where(u => u.Username.Contains(query)).Select(u => new SearchData
                    {
                        UserID = u.UserID,
                        Username = u.Username,
                        Followers = u.Followers,
                        Following = u.Following
                    }).ToList();
                }
            }

            public async Task<int> GetAmountOfPosts(int userID)
            {
                try
                {
                    using (var _db = new AuroraDB())
                    {
                        return _db.Posts.Count(p => p.User.UserID == userID);
                    }
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }

            public async Task<List<CommentData>> GetAllComments(int postID)
            {
                using (var _db = new AuroraDB())
                {
                    return await _db.Comments
                        .Where(c => c.PostID == postID)
                        .Include(c => c.User)
                        .Select(c => new CommentData
                        {
                            Username = c.User.Username,
                            CommentContent = c.CommentContent,
                            Email = c.User.Email,
                            ProfilePicture = c.User.ProfileImagePath == string.Empty
                                ? string.Empty
                                : ImageStorageService.GetImageAsync(c.User.ProfileImagePath).Result
                        }) // Anonymous Type
                        .ToListAsync(); // Convert in memory
                }
            }

            public async Task<(bool, bool, bool)> GetUserLikeData(int userID, int postID)
            {
                using (var _db = new AuroraDB())
                {
                    try
                    {
                        var reactions = await _db.Reactions
                            .Where(r => r.UserID == userID && r.PostID == postID)
                            .Select(r => r.Type)
                            .ToListAsync();

                        return (reactions.Contains(ReactionType.Like), reactions.Contains(ReactionType.Dislike), reactions.Contains(ReactionType.SuperLike));
                    }
                    catch (Exception ex)
                    {
                        return (false, false, false);
                    }
                }
            }

            public async Task<Tuple<int, string>> GetPost(int userID, int postIndex)
            {
                using (var _db = new AuroraDB())
                {
                    var posts = _db.Posts.Where(p => p.UserID == userID).ToList();
                    return new(posts[postIndex].PostId, await ImageStorageService.GetImageAsync(posts[postIndex].PostPath));
                }
            }

            public async Task<PostData> GetPostData(int userID, int postID)
            {
                using (var _db = new AuroraDB())
                {
                    var post = await _db.Posts.FindAsync(postID);
                    var likePostData = await GetUserLikeData(userID, postID);

                    return new PostData
                    {
                        AmountOfDislikes = post.AmountDislikes,
                        AmountOfLikes = post.AmountLikes,
                        AmountOfSuperLikes = post.AmountSuperLikes,
                        Description = post.Description,
                        Comments = await GetAllComments(post.PostId),
                        AlreadyDisliked = likePostData.Item2,
                        AlreadyLiked = likePostData.Item1,
                        AlreadySuperLiked = likePostData.Item3
                    };

                }
            }

        public async Task AddComment(int userID, int postID, string comment)
        {
            using (var _db = new AuroraDB())
            {
                try
                {
                    var newComment = new Comment
                    {
                        UserID = userID,
                        PostID = postID,
                        CommentContent = comment
                    };
                    _db.Comments.Add(newComment);
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log the exception (ex) here
                    throw; // Re-throw or handle as needed
                }
            }
        }

        public async Task AddLike(int userID, int postID, int likeType)
        {
            using (var _db = new AuroraDB())
            {
                var newComment = new Reaction
                {
                    UserID = userID,
                    PostID = postID,
                    Type = (ReactionType)likeType
                };
                _db.Reactions.Add(newComment);
                var post = await _db.Posts.FindAsync(postID);
                switch (likeType)
                {
                    case 1:
                        post.AmountLikes++;
                        break;
                    case 2:
                        post.AmountDislikes++;
                        break;
                    case 3:
                        post.AmountSuperLikes++;
                        break;
                }
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveLike(int userID, int postID, int likeType)
        {
            using (var _db = new AuroraDB())
            {
                var reaction = await _db.Reactions
                    .FirstOrDefaultAsync(r => r.UserID == userID && r.PostID == postID && r.Type == (ReactionType)likeType);
                if (reaction != null)
                {
                    _db.Reactions.Remove(reaction);
                    var post = await _db.Posts.FindAsync(postID);
                    switch (likeType)
                    {
                        case 1:
                            post.AmountLikes--;
                            break;
                        case 2:
                            post.AmountDislikes--;
                            break;
                        case 3:
                            post.AmountSuperLikes--;
                            break;
                    }
                    await _db.SaveChangesAsync();
                }
            }
        }

        public async Task<bool> FollowUser(int userID, int followUserID)
        {
            try
            {
                using (var _db = new AuroraDB())
                {
                    var follow = await _db.Follows
                        .FirstOrDefaultAsync(f => f.FollowerUserID == userID && f.FollowedUserID == followUserID);
                    if (follow != null && userID != followUserID)
                    {
                        return false;
                    }
                    _db.Follows.Add(new Follow
                    {
                        FollowedUserID = followUserID,
                        FollowerUserID = userID
                    });

                    var user = await _db.Users.FindAsync(userID);
                    var followUser = await _db.Users.FindAsync(followUserID);
                    user.Following++;
                    followUser.Followers++;
                    await _db.SaveChangesAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UnfollowUser(int userID, int followUserID)
        {
            using (var _db = new AuroraDB())
            {
                var follow = await _db.Follows
                    .FirstOrDefaultAsync(f => f.FollowerUserID == userID && f.FollowedUserID == followUserID);
                if (follow == null && userID != followUserID)
                {
                    return false;
                }
                _db.Follows.Remove(follow);

                var user = await _db.Users.FindAsync(userID);
                var followUser = await _db.Users.FindAsync(followUserID);
                user.Following--;
                followUser.Followers--;
                await _db.SaveChangesAsync();

                return true;
            }
        }

        public async Task<Tuple<int, int, string>> GetRecentPosts(int startIndex)
        {
            using (var _db = new AuroraDB())
            {
                // First, retrieve the posts from the database synchronously
                var post = await _db.Posts
                    .OrderByDescending(p => p.PostId)
                    .Skip(startIndex)
                    .Take(1)
                    .FirstOrDefaultAsync();

                // Process each post to asynchronously fetch the image

                var image = await ImageStorageService.GetImageAsync(post.PostPath);
                return Tuple.Create(post.UserID, post.PostId, image);
            }
        }

        public async Task<Tuple<int, int, string>> GetFollowingPosts(int userID, int startIndex)
        {
            using (var _db = new AuroraDB())
            {
                var followID = await _db.Follows
                    .Where(f => f.FollowerUserID == userID)
                    .Select(f => f.FollowedUserID)
                    .ToListAsync();

                var post = await _db.Posts
                    .OrderByDescending(p => p.PostId)
                    .Where(p => followID.Contains(p.UserID))
                    .Skip(startIndex)
                    .Take(1)
                    .FirstOrDefaultAsync();
                if(post == null)
                {
                    return null;
                }
                var image = await ImageStorageService.GetImageAsync(post.PostPath);
                return Tuple.Create(post.UserID, post.PostId, image);
            }
        }

        public async Task<List<Tuple<int, string, string>>> GetFollowingUsers(int userID)
        {
            using (var _db = new AuroraDB())
            {
                var followIDs = await _db.Follows
                    .Where(f => f.FollowerUserID == userID)
                    .Select(f => f.FollowedUserID)
                    .ToListAsync();
                var onlineUsers = await _db.Users
                    .Where(u => followIDs.Contains(u.UserID))
                    .ToListAsync();

                var onlineUserData = new List<Tuple<int, string, string>>();

                foreach (var u in onlineUsers)
                {
                    string img = string.Empty;
                    if (u.ProfileImagePath != null)
                    {
                        img = await ImageStorageService.GetImageAsync(u.ProfileImagePath);
                    }
                    onlineUserData.Add(Tuple.Create(u.UserID, u.Username, img));
                }
                return onlineUserData;
            }
        }

        public async Task<List<Tuple<int, string, string>>> GetFollowedUsers(int userID)
        {
            using (var _db = new AuroraDB())
            {
                var followIDs = await _db.Follows
                    .Where(f => f.FollowedUserID == userID)
                    .Select(f => f.FollowerUserID)
                    .ToListAsync();

                var onlineUsers = await _db.Users
                    .Where(u => followIDs.Contains(u.UserID))
                    .ToListAsync();

                var onlineUserData = new List<Tuple<int, string, string>>();

                foreach (var u in onlineUsers)
                {
                    string img = string.Empty;
                    if (u.ProfileImagePath != null)
                    {
                        img = await ImageStorageService.GetImageAsync(u.ProfileImagePath);
                    }
                    onlineUserData.Add(Tuple.Create(u.UserID, u.Username, img));
                }

                return onlineUserData;
            }
        }


    }
}
