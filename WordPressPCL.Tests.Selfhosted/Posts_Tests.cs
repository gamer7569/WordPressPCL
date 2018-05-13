﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using WordPressPCL.Models;
using WordPressPCL.Tests.Selfhosted.Utility;
using System.Linq;
using WordPressPCL;
using WordPressPCL.Utility;
using System.Diagnostics;

namespace WordPressPCL.Tests.Selfhosted
{
    [TestClass]
    public class Posts_Tests
    {
        private static WordPressClient _client;
        private static WordPressClient _clientAuth;

        [ClassInitialize]
        public static async Task Init(TestContext testContext)
        {
            _client = ClientHelper.GetWordPressClient();
            _clientAuth = await ClientHelper.GetAuthenticatedWordPressClient();
        }

        [TestMethod]
        public async Task Posts_Create()
       {
            var post = new Post()
            {
                Title = new Title("Title 1"),
                Content = new Content("Content PostCreate")
            };
            var createdPost = await _clientAuth.Posts.Create(post);


            Assert.AreEqual(post.Content.Raw, createdPost.Content.Raw);
            Assert.IsTrue(createdPost.Content.Rendered.Contains(post.Content.Rendered));
        }

        [TestMethod]
        public async Task Posts_Read()
        {
            var posts = await _clientAuth.Posts.Query(new PostsQueryBuilder());
            Assert.IsNotNull(posts);
            Assert.AreNotEqual(posts.Count(), 0);

            var postsEdit = await _clientAuth.Posts.Query(new PostsQueryBuilder()
            { 
                Context = Context.Edit,
                PerPage = 1,
                Page = 1
            }, true);
            Assert.AreEqual(1, postsEdit.Count());
            Assert.IsNotNull(postsEdit.FirstOrDefault());
            Assert.IsNotNull(postsEdit.FirstOrDefault().Content.Raw);
        }

        [TestMethod]
        public async Task Posts_Get()
        {
            var posts = await _clientAuth.Posts.Get();
            Assert.IsNotNull(posts);
            Assert.AreNotEqual(posts.Count(), 0);
        }

        [TestMethod]
        public async Task Posts_Update()
        {
            var testContent = "Test" + new Random().Next() ;
            var posts = await _clientAuth.Posts.GetAll();
            Assert.IsTrue(posts.Count() > 0);

            // edit first post and update it
            var post = await _clientAuth.Posts.GetByID(posts.First().Id);
            post.Content.Raw = testContent;
            var updatedPost = await _clientAuth.Posts.Update(post);
            Assert.AreEqual(updatedPost.Content.Raw, testContent);
            Assert.IsTrue(updatedPost.Content.Rendered.Contains(testContent));
        }

        [TestMethod]
        public async Task Posts_Delete()
        {
            var post = new Post()
            {
                Title = new Title("Title 1"),
                Content = new Content("Content PostCreate")
            };
            var createdPost = await _clientAuth.Posts.Create(post);
            Assert.IsNotNull(createdPost);

            var resonse = await _clientAuth.Posts.Delete(createdPost.Id);
            Assert.IsTrue(resonse);
            
            await Assert.ThrowsExceptionAsync<WPException>(async () =>
            {
                var postById = await _clientAuth.Posts.GetByID(createdPost.Id);
            });
        }

        [TestMethod]
        public async Task Posts_Query()
        {
            var queryBuilder = new PostsQueryBuilder()
            {
                Page = 1,
                PerPage = 15,
                OrderBy = PostsOrderBy.Title,
                Order = Order.ASC,
                Statuses = new Status[] { Status.Publish },
                Embed = true
            };
            var queryresult = await _clientAuth.Posts.Query(queryBuilder);
            Assert.AreEqual(queryBuilder.BuildQueryURL(), "?page=1&per_page=15&orderby=title&status=publish&order=asc&_embed=true");
            Assert.IsNotNull(queryresult);
            Assert.AreNotSame(queryresult.Count(), 0);
        }
    }
}