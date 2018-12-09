using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Constructors
{

    namespace BadPractice
    {

        public class User
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class UserRepository
        {
            private readonly bool _cacheEnabled;
            private List<User> _userCache;

            public UserRepository(bool cacheEnabled)
            {
                _cacheEnabled = cacheEnabled;

                if (cacheEnabled)
                {
                    _userCache = GetAllUsersFromDb();
                }
            }

            public User GetUser(int userId)
            {
                return _cacheEnabled ? 
                    GetUserFromCache(userId) : 
                    GetUserFromDb(userId);
            }

            private User GetUserFromDb(int userId)
            {
                // get user from db
                return null;
            }

            private User GetUserFromCache(int userId)
            {
                return _userCache[userId];
            }

            private List<User> GetAllUsersFromDb()
            {
                // connect to db and get all users into cache
                return null;
            }
        }

        [TestFixture]
        public class UserRepositoryTests
        {
            [Test]
            public void WhenCacheDisabledItShouldntGetAllUsersTest()
            {
                const bool IS_CACHE_ENABLED = false;
                UserRepository repo = new UserRepository(IS_CACHE_ENABLED);

                // Assert ???
            }

            [Test]
            public void WhenCacheEnabledGetSingleUserFromCacheTest()
            {
                const bool IS_CACHE_ENABLED = true;
                const int USER_ID = 5;
                User expectedUser = new User { Id = USER_ID, UserName = "serkan" };

                // provide user in arrange so that we check we get the same user in return?

                UserRepository repo = new UserRepository(IS_CACHE_ENABLED);
                
                Assert.AreEqual(expectedUser, repo.GetUser(USER_ID));
            }
        }
    }

    namespace GoodPractice
    {
        public class User
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public interface IDatabaseEngine
        {
            User GetUser(int userId);
            IEnumerable<User> GetAllUsers();
        }

        public interface ICacheEngine<T, K>
        {
            K GetData(T id);
            void SetData(List<K> data);
        }

        public class UserRepository
        {
            private readonly IDatabaseEngine _databaseEngine;
            private readonly ICacheEngine<int, User> _userCacheEngine;
            private readonly bool _cacheEnabled;

            public UserRepository(IDatabaseEngine databaseEngine, ICacheEngine<int, User> userCacheEngine, bool cacheEnabled)
            {
                _databaseEngine = databaseEngine;
                _userCacheEngine = userCacheEngine;
                _cacheEnabled = cacheEnabled;
            }

            public User GetUser(int userId)
            {
                return _cacheEnabled ?
                    _userCacheEngine.GetData(userId) :
                    _databaseEngine.GetUser(userId);
            }
        }

        [TestFixture]
        public class UserRepositoryTests
        {
            private Mock<ICacheEngine<int, User>> _cacheEngineMock;
            private Mock<IDatabaseEngine> _databaseEngineMock;

            [SetUp]
            public void SetUp()
            {
                _cacheEngineMock = new Mock<ICacheEngine<int, User>>(MockBehavior.Strict);
                _databaseEngineMock = new Mock<IDatabaseEngine>(MockBehavior.Strict);
            }

            [TearDown]
            public void TearDown()
            {
                _cacheEngineMock.VerifyAll();
                _databaseEngineMock.VerifyAll();
            }

            [Test]
            public void WhenCacheDisabledItShouldntGetAllUsersTest()
            {
                bool IS_CACHE_ENABLED = false;
                UserRepository repo = new UserRepository(_databaseEngineMock.Object, _cacheEngineMock.Object, IS_CACHE_ENABLED);
                
                _databaseEngineMock.Verify(x => x.GetAllUsers(), Times.Never());
            }

            [Test]
            public void WhenCacheEnabledGetSingleUserFromCacheTest()
            {
                const bool IS_CACHE_ENABLED = true;
                User expectedUser = new User { Id = 5, UserName = "serkan" };
                _cacheEngineMock.Setup(x => x.GetData(expectedUser.Id)).Returns(expectedUser);

                UserRepository repo = new UserRepository(_databaseEngineMock.Object, _cacheEngineMock.Object, IS_CACHE_ENABLED);

                Assert.AreEqual(expectedUser, repo.GetUser(expectedUser.Id));
            }

            [Test]
            public void WhenCacheIsNotEnabledGetSingleUserFromDatabaseTest()
            {
                const bool IS_CACHE_ENABLED = false;
                User expectedUser = new User { Id = 5, UserName = "serkan" };
                _databaseEngineMock.Setup(x => x.GetUser(expectedUser.Id)).Returns(expectedUser);

                UserRepository repo = new UserRepository(_databaseEngineMock.Object, _cacheEngineMock.Object, IS_CACHE_ENABLED);

                Assert.AreEqual(expectedUser, repo.GetUser(expectedUser.Id));
            }
        }
    }
}
