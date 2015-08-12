﻿using System;
using TextmagicRest.Model;
using Moq;
using NUnit.Framework;
using RestSharp;
using System.Text;
using RestSharp.Deserializers;

namespace TextmagicRest.Tests
{
    [TestFixture]
    public class MessagesTests
    {
        private Mock<Client> mockClient;

        private const int messageId = 49575710;
        private const string messageReceiver = "999123456";
        private const string messageSender = "447624800500";
        private DateTime messageTime = new DateTime(2015, 05, 25, 06, 40, 45, DateTimeKind.Utc);
        private const DeliveryStatus messageStatus = DeliveryStatus.Queued;
        private const string messageText = "Test C# API message";
        private const string messageCharset = "ISO-8859-1";
        private string messageCountryId = "EE";
        private double messagePrice = 0.037;
        private int messagePartsCount = 1;

        private const int replyId = 5946228;
        private const string replyReceiver = messageSender;
        private const string replySender = messageReceiver;
        private DateTime replyTime = new DateTime(2015, 05, 25, 06, 45, 45, DateTimeKind.Utc);
        private const string replyText = "Test C# API reply";

        private const int scheduleId = 4466;
        private DateTime scheduleTime = new DateTime(2015, 05, 08, 13, 18, 38);

        private const string sessionReferenceId = "reference-id-test";
        private const int sessionId = 34436259;

        [SetUp]
        public void Setup()
        {
            mockClient = new Mock<Client>(Common.Username, Common.Token);
            mockClient.CallBase = true;
        }

        [Test]
        public void ShouldGetSingleMessage()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<Message>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new Message());
            var client = mockClient.Object;

            client.GetMessage(messageId);

            mockClient.Verify(trc => trc.Execute<Message>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("messages/{id}", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(1, savedRequest.Parameters.Count);


            var content = "{ \"id\": 49575710, \"receiver\": \"999123456\", \"messageTime\": \"2015-05-25T06:40:45+0000\", \"status\": \"q\","
                + "\"text\": \"Test C# API message\", \"charset\": \"ISO-8859-1\", \"firstName\": null, \"lastName\": null, \"country\": \"EE\","
                + "\"sender\": \"447624800500\", \"price\": 0.037, \"partsCount\": 1 }";

            var testClient = Common.CreateClient<Message>(content, null, null);
            client = new Client(testClient);

            var message = client.GetMessage(messageId);

            Assert.IsTrue(message.Success);
            Assert.AreEqual(messageId, message.Id);
            Assert.AreEqual(messageStatus, message.Status);
            Assert.AreEqual(messageSender, message.Sender);
            Assert.AreEqual(messageReceiver, message.Receiver);
            Assert.AreEqual(messageTime.ToLocalTime(), message.MessageTime);
            Assert.AreEqual(messageCharset, message.Charset);
            Assert.AreEqual(messageCountryId, message.CountryId);
            Assert.AreEqual(messagePrice, message.Price);
            Assert.AreEqual(messagePartsCount, message.PartsCount);
            Assert.IsNull(message.FirstName);
            Assert.IsNull(message.LastName);
        }

        [Test]
        public void ShouldGetAllMessages()
        {
            var page = 2;
            var limit = 3;

            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<MessagesResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new MessagesResult());
            var client = mockClient.Object;

            client.GetMessages(page, limit);

            mockClient.Verify(trc => trc.Execute<MessagesResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("messages", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(2, savedRequest.Parameters.Count);
            Assert.AreEqual(page.ToString(), savedRequest.Parameters.Find(x => x.Name == "page").Value);
            Assert.AreEqual(limit.ToString(), savedRequest.Parameters.Find(x => x.Name == "limit").Value);

            var content = "{ \"page\": 2,  \"limit\": 3, \"pageCount\": 3, \"resources\": ["
                + "{ \"id\": 49575710, \"receiver\": \"999123456\", \"messageTime\": \"2015-05-25T06:40:45+0000\", \"status\": \"q\","
                + "\"text\": \"Test C# API message\", \"charset\": \"ISO-8859-1\", \"firstName\": null, \"lastName\": null, \"country\": \"EE\","
                + "\"sender\": \"447624800500\", \"price\": 0.037, \"partsCount\": 1 }"

                + "{ \"id\": 49575711, \"receiver\": \"999123456\", \"messageTime\": \"2015-05-25T06:40:46+0000\", \"status\": \"a\","
                + "\"text\": \"Test C# API message\", \"charset\": \"ISO-8859-1\", \"firstName\": \"Albert\", \"lastName\": null, \"country\": \"EE\","
                + "\"sender\": \"447624800500\", \"price\": 0.037, \"partsCount\": 1 }"

                + "{ \"id\": 49575712, \"receiver\": \"999123456\", \"messageTime\": \"2015-05-25T06:40:49+0000\", \"status\": \"d\","
                + "\"text\": \"Test C# API message\", \"charset\": \"ISO-8859-1\", \"firstName\": null, \"lastName\": null, \"country\": \"EE\","
                + "\"sender\": \"447624800500\", \"price\": 0.037, \"partsCount\": 1 }"

                + "] }";

            var testClient = Common.CreateClient<MessagesResult>(content, null, null);
            client = new Client(testClient);

            var messages = client.GetMessages(2, 3);

            Assert.IsTrue(messages.Success);
            Assert.NotNull(messages.Messages);
            Assert.AreEqual(3, messages.Messages.Count);
            Assert.AreEqual(page, messages.Page);
            Assert.AreEqual(limit, messages.Limit);
            Assert.AreEqual(3, messages.PageCount);
            Assert.IsNotNull(messages.Messages[1].FirstName);
            Assert.AreEqual(DeliveryStatus.Acked, messages.Messages[1].Status);
            Assert.AreEqual(DeliveryStatus.Delivered, messages.Messages[2].Status);            
        }

        [Test]
        public void ShouldDeleteMessage()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<DeleteResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new DeleteResult());
            var client = mockClient.Object;

            var message = new Message()
            {
                Id = messageId
            };

            var result = client.DeleteMessage(message);

            mockClient.Verify(trc => trc.Execute<DeleteResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("messages/{id}", savedRequest.Resource);
            Assert.AreEqual(Method.DELETE, savedRequest.Method);
            Assert.AreEqual(1, savedRequest.Parameters.Count);
            Assert.AreEqual(messageId.ToString(), savedRequest.Parameters.Find(x => x.Name == "id").Value);
        }

        [Test]
        public void ShouldGetSingleReply()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<Reply>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new Reply());
            var client = mockClient.Object;

            client.GetReply(replyId);

            mockClient.Verify(trc => trc.Execute<Reply>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("replies/{id}", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(1, savedRequest.Parameters.Count);

            var content = "{ \"id\": 5946228, \"receiver\": \"447624800500\", \"messageTime\": \"2015-05-25T06:45:45+0000\","
                + "\"text\": \"Test C# API reply\", \"sender\": \"999123456\" }";

            var testClient = Common.CreateClient<Reply>(content, null, null);
            client = new Client(testClient);

            var reply = client.GetReply(messageId);

            Assert.IsTrue(reply.Success);
            Assert.AreEqual(replyId, reply.Id);
            Assert.AreEqual(replySender, reply.Sender);
            Assert.AreEqual(replyReceiver, reply.Receiver);
            Assert.AreEqual(replyTime.ToLocalTime(), reply.MessageTime);
            Assert.AreEqual(replyText, reply.Text);          
        }

        [Test]
        public void ShouldGetAllReplies()
        {
            var page = 2;
            var limit = 3;

            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<RepliesResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new RepliesResult());
            var client = mockClient.Object;

            client.GetReplies(page, limit);

            mockClient.Verify(trc => trc.Execute<RepliesResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("replies", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(2, savedRequest.Parameters.Count);
            Assert.AreEqual(page.ToString(), savedRequest.Parameters.Find(x => x.Name == "page").Value);
            Assert.AreEqual(limit.ToString(), savedRequest.Parameters.Find(x => x.Name == "limit").Value);

            var content = "{ \"page\": 2,  \"limit\": 3, \"pageCount\": 3, \"resources\": ["
                + "{ \"id\": 5946228, \"receiver\": \"447624800500\", \"messageTime\": \"2015-05-25T06:45:45+0000\","
                + "\"text\": \"Test C# API reply\", \"sender\": \"999123456\" }"

                + "{ \"id\": 5946229, \"receiver\": \"447624800500\", \"messageTime\": \"2015-05-25T06:45:46+0000\","
                + "\"text\": \"Test C# API reply 2\", \"sender\": \"999123456\" }"
                
                + "] }";

            var testClient = Common.CreateClient<RepliesResult>(content, null, null);
            client = new Client(testClient);

            var replies = client.GetReplies(2, 3);

            Assert.IsTrue(replies.Success);
            Assert.NotNull(replies.Replies);
            Assert.AreEqual(2, replies.Replies.Count);
            Assert.AreEqual(page, replies.Page);
            Assert.AreEqual(limit, replies.Limit);
            Assert.AreEqual(3, replies.PageCount);
            Assert.AreEqual(replyText, replies.Replies[0].Text);
            Assert.AreEqual(replyText + " 2", replies.Replies[1].Text);
        }

        [Test]
        public void ShouldDeleteReply()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<DeleteResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new DeleteResult());
            var client = mockClient.Object;

            var reply = new Reply()
            {
                Id = replyId
            };

            var result = client.DeleteReply(reply);

            mockClient.Verify(trc => trc.Execute<DeleteResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("replies/{id}", savedRequest.Resource);
            Assert.AreEqual(Method.DELETE, savedRequest.Method);
            Assert.AreEqual(1, savedRequest.Parameters.Count);
            Assert.AreEqual(replyId.ToString(), savedRequest.Parameters.Find(x => x.Name == "id").Value);
        }

        [Test]
        public void ShouldGetSingleSchedule()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<Schedule>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new Schedule());
            var client = mockClient.Object;

            client.GetSchedule(scheduleId);

            mockClient.Verify(trc => trc.Execute<Schedule>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("schedules/{id}", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(1, savedRequest.Parameters.Count);

            var content = "{\"id\":4466,\"nextSend\":\"2015-05-08T13:18:38+0000\",\"rrule\":null"
                + "\"session\":{\"id\":34436259,\"startTime\":\"2015-05-08T13:18:38+0000\",\"text\":\"SCHEDULED API TEST\",\"source\":\"A\","
                + "\"referenceId\":\"reference-id-test\",\"price\":0.074,\"numbersCount\":1}}";

            var testClient = Common.CreateClient<Schedule>(content, null, null);
            client = new Client(testClient);

            var schedule = client.GetSchedule(messageId);

            Assert.IsTrue(schedule.Success);
            Assert.AreEqual(scheduleId, schedule.Id);
            Assert.AreEqual(scheduleTime.ToLocalTime(), schedule.NextSend);
            Assert.IsNull(schedule.Rrule);
            Assert.IsNotNull(schedule.Session);
            Assert.AreEqual(sessionReferenceId, schedule.Session.ReferenceId);
        }
        
        [Test]
        public void ShouldGetAllSchedules()
        {
            var page = 2;
            var limit = 3;

            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<SchedulesResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new SchedulesResult());
            var client = mockClient.Object;

            client.GetSchedules(page, limit);

            mockClient.Verify(trc => trc.Execute<SchedulesResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("schedules", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(2, savedRequest.Parameters.Count);
            Assert.AreEqual(page.ToString(), savedRequest.Parameters.Find(x => x.Name == "page").Value);
            Assert.AreEqual(limit.ToString(), savedRequest.Parameters.Find(x => x.Name == "limit").Value);

            var content = "{ \"page\": 2,  \"limit\": 3, \"pageCount\": 3, \"resources\": ["
                + "{\"id\":4466,\"nextSend\":\"2015-05-08T13:18:38+0000\",\"rrule\":null"
                + "\"session\":{\"id\":34436259,\"startTime\":\"2015-05-08T13:18:38+0000\",\"text\":\"SCHEDULED API TEST\",\"source\":\"A\","
                + "\"referenceId\":\"reference-id-test\",\"price\":0.074,\"numbersCount\":1}}"

                + "{\"id\":4466,\"nextSend\":null,\"rrule\":null"
                + "\"session\":{\"id\":34436262,\"startTime\":\"2015-05-08T13:18:38+0000\",\"text\":\"SCHEDULED API TEST\",\"source\":\"A\","
                + "\"referenceId\":\"reference-id-test\",\"price\":0.074,\"numbersCount\":1}}"

                + "] }";

            var testClient = Common.CreateClient<SchedulesResult>(content, null, null);
            client = new Client(testClient);

            var schedules = client.GetSchedules(2, 3);

            Assert.IsTrue(schedules.Success);
            Assert.NotNull(schedules.Schedules);
            Assert.AreEqual(2, schedules.Schedules.Count);
            Assert.AreEqual(page, schedules.Page);
            Assert.AreEqual(limit, schedules.Limit);
            Assert.AreEqual(3, schedules.PageCount);
            Assert.AreEqual(sessionId, schedules.Schedules[0].Session.Id);
        }

        [Test]
        public void ShouldDeleteSchedule()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<DeleteResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new DeleteResult());
            var client = mockClient.Object;

            var schedule = new Schedule() { Id = scheduleId };

            var result = client.DeleteSchedule(schedule);

            mockClient.Verify(trc => trc.Execute<DeleteResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("schedules/{id}", savedRequest.Resource);
            Assert.AreEqual(Method.DELETE, savedRequest.Method);
            Assert.AreEqual(1, savedRequest.Parameters.Count);
            Assert.AreEqual(scheduleId.ToString(), savedRequest.Parameters.Find(x => x.Name == "id").Value);
        }

        [Test]
        public void ShouldGetSingleSession()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<Session>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new Session());
            var client = mockClient.Object;

            client.GetSession(sessionId);

            mockClient.Verify(trc => trc.Execute<Session>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("sessions/{id}", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(1, savedRequest.Parameters.Count);

            var content = "{\"id\":34436259,\"startTime\":\"2015-05-08T13:18:38+0000\",\"text\":\"SCHEDULED API TEST\",\"source\":\"A\","
                + "\"referenceId\":\"reference-id-test\",\"price\":0.074,\"numbersCount\":1}";

            var testClient = Common.CreateClient<Session>(content, null, null);
            client = new Client(testClient);

            var session = client.GetSession(sessionId);

            Assert.IsTrue(session.Success);
            Assert.AreEqual(sessionId, session.Id);
            Assert.AreEqual(scheduleTime.ToLocalTime(), session.StartTime);
            Assert.AreEqual(sessionReferenceId, session.ReferenceId);
        }

        [Test]
        public void ShouldDeleteSession()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<DeleteResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new DeleteResult());
            var client = mockClient.Object;

            var session = new Session() { Id = sessionId };

            var result = client.DeleteSession(session);

            mockClient.Verify(trc => trc.Execute<DeleteResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("sessions/{id}", savedRequest.Resource);
            Assert.AreEqual(Method.DELETE, savedRequest.Method);
            Assert.AreEqual(1, savedRequest.Parameters.Count);
            Assert.AreEqual(sessionId.ToString(), savedRequest.Parameters.Find(x => x.Name == "id").Value);
        }

        [Test]
        public void ShouldGetsSessionMessages()
        {
            var page = 2;
            var limit = 3;

            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<MessagesResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new MessagesResult());
            var client = mockClient.Object;

            client.GetSessionMessages(sessionId, page, limit);

            mockClient.Verify(trc => trc.Execute<MessagesResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("sessions/{id}/messages", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(3, savedRequest.Parameters.Count);
            Assert.AreEqual(sessionId.ToString(), savedRequest.Parameters.Find(x => x.Name == "id").Value);
            Assert.AreEqual(page.ToString(), savedRequest.Parameters.Find(x => x.Name == "page").Value);
            Assert.AreEqual(limit.ToString(), savedRequest.Parameters.Find(x => x.Name == "limit").Value);
        }

        [Test]
        public void ShouldGetAllSessions()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<SessionsResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new SessionsResult());
            var client = mockClient.Object;
            int page = 2;
            int limit = 3;

            client.GetSessions(page, limit);

            mockClient.Verify(trc => trc.Execute<SessionsResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("sessions", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(2, savedRequest.Parameters.Count);

            var content = "{ \"page\": 2,  \"limit\": 3, \"pageCount\": 3, \"resources\": ["
                + "{\"id\":34436259,\"startTime\":\"2015-05-08T13:18:38+0000\",\"text\":\"SCHEDULED API TEST\",\"source\":\"A\","
                + "\"referenceId\":\"reference-id-test\",\"price\":0.074,\"numbersCount\":1}"

                + "{\"id\":34436258,\"startTime\":\"2015-05-08T13:18:38+0000\",\"text\":\"SCHEDULED API TEST\",\"source\":\"A\","
                + "\"referenceId\":\"reference-id-test\",\"price\":0.074,\"numbersCount\":1}"

                + "{\"id\":34436257,\"startTime\":\"2015-05-08T13:18:38+0000\",\"text\":\"SCHEDULED API TEST\",\"source\":\"A\","
                + "\"referenceId\":\"reference-id-test\",\"price\":0.074,\"numbersCount\":1}"

                + "] }";

            var testClient = Common.CreateClient<SessionsResult>(content, null, null);
            client = new Client(testClient);

            var sessions = client.GetSessions(sessionId);

            Assert.IsTrue(sessions.Success);
            Assert.NotNull(sessions.Sessions);
            Assert.AreEqual(3, sessions.Sessions.Count);
            Assert.AreEqual(page, sessions.Page);
            Assert.AreEqual(limit, sessions.Limit);
            Assert.AreEqual(3, sessions.PageCount);
            Assert.AreEqual(sessionId, sessions.Sessions[0].Id);
        }

        [Test]
        public void ShouldGetAllChats()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<ChatsResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new ChatsResult());
            var client = mockClient.Object;
            int page = 2;
            int limit = 3;

            client.GetChats(page, limit);

            mockClient.Verify(trc => trc.Execute<ChatsResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("chats", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(2, savedRequest.Parameters.Count);

            var content = "{\"page\":2,\"limit\":3,\"pageCount\":3,\"resources\":["
                + "{\"id\":44577,\"phone\":\"999123456\",\"contact\":null,\"unread\":\"0\",\"updatedAt\":\"2015-04-08T11:58:49+0000\"},"
                + "{\"id\":44433,\"phone\":\"999123457\",\"contact\":null,\"unread\":\"5\",\"updatedAt\":\"2014-08-13T05:36:40+0000\"},"
                + "{\"id\":39564,\"phone\":\"999123458\",\"contact\":null,\"unread\":\"0\",\"updatedAt\":\"2014-08-13T05:36:28+0000\"}"
                + "]}";

            var testClient = Common.CreateClient<ChatsResult>(content, null, null);
            client = new Client(testClient);
            var chats = client.GetChats(page, limit);

            Assert.IsTrue(chats.Success);
            Assert.NotNull(chats.Chats);
            Assert.AreEqual(3, chats.Chats.Count);
            Assert.AreEqual(page, chats.Page);
            Assert.AreEqual(limit, chats.Limit);
            Assert.AreEqual(3, chats.PageCount);
            Assert.AreEqual(5, chats.Chats[1].Unread);
        }

        [Test]
        public void ShouldGetSingleChat()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<ChatMessagesResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new ChatMessagesResult());
            var client = mockClient.Object;
            string phone = "999123456";
            int page = 2;
            int limit = 3;

            client.GetChat(phone, 2, 3);

            mockClient.Verify(trc => trc.Execute<ChatMessagesResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("chats/{phone}", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(3, savedRequest.Parameters.Count);

            var content = "{\"page\":2,\"limit\":3,\"pageCount\":3,\"resources\":["
                + "{\"id\":49360873,\"sender\":\"9990001234\",\"messageTime\":\"2014-08-13T05:05:51+0000\",\"text\":\"Hello. TextMagic test.\n\nPlease reply.\",\"receiver\":\"999123456\",\"status\":\"f\",\"firstName\":null,\"lastName\":null,\"direction\":\"o\"},"
                + "{\"id\":49430972,\"sender\":\"999123456\",\"messageTime\":\"2014-09-19T05:34:22+0000\",\"text\":\"testing\",\"receiver\":\"9990001234\",\"status\":\"d\",\"firstName\":null,\"lastName\":null,\"direction\":\"i\"}"
                + "]}";

            var testClient = Common.CreateClient<ChatMessagesResult>(content, null, null);
            client = new Client(testClient);
            var chat = client.GetChat(phone, 2, 3);

            Assert.IsTrue(chat.Success);
            Assert.AreEqual(2, chat.Messages.Count);
            Assert.AreEqual("testing", chat.Messages[1].Text);
            Assert.AreEqual(phone, chat.Messages[1].Sender);
            Assert.AreEqual(ChatMessageDirection.Outgoing, chat.Messages[0].Direction);
            Assert.AreEqual(ChatMessageDirection.Incoming, chat.Messages[1].Direction);
            Assert.AreEqual(page, chat.Page);
            Assert.AreEqual(limit, chat.Limit);
            Assert.AreEqual(3, chat.PageCount);
        }

        [Test]
        public void ShouldGetAllBulks()
        {
            IRestRequest savedRequest = null;
            mockClient.Setup(trc => trc.Execute<BulkSessionsResult>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>((request) => savedRequest = request)
                .Returns(new BulkSessionsResult());
            var client = mockClient.Object;
            int page = 2;
            int limit = 3;

            client.GetBulkSessions(page, limit);

            mockClient.Verify(trc => trc.Execute<BulkSessionsResult>(It.IsAny<IRestRequest>()), Times.Once);
            Assert.IsNotNull(savedRequest);
            Assert.AreEqual("bulks", savedRequest.Resource);
            Assert.AreEqual(Method.GET, savedRequest.Method);
            Assert.AreEqual(2, savedRequest.Parameters.Count);

            var content = "{\"page\":2,\"limit\":3,\"pageCount\":2,\"resources\":["
                + "{\"id\":271,\"status\":\"c\",\"itemsProcessed\":9937,\"itemsTotal\":9937,\"createdAt\":\"2014-12-14T04:34:46+0000\",\"session\":{\"id\":34419457,\"startTime\":\"2014-12-14T04:34:53+0000\",\"text\":\"test\",\"source\":\"O\",\"referenceId\":\"O_tester_098f6bcd4621d373cade4e832627b4f6_1414151612548d136b600eb4.33276307\",\"price\":393.712,\"numbersCount\":9937},\"text\":\"test\"},"
                + "{\"id\":270,\"status\":\"f\",\"itemsProcessed\":9937,\"itemsTotal\":9937,\"createdAt\":\"2014-12-12T07:34:39+0000\",\"session\":{\"id\":34419456,\"startTime\":\"2014-12-12T07:34:46+0000\",\"text\":\"wewerwerwerwerwerwerwr\",\"source\":\"O\",\"referenceId\":\"O_tester_c0ec90d8914c15a564032c2d3bec588d_1843256795548a9a9479e5f7.33123700\",\"price\":393.712,\"numbersCount\":9937},\"text\":\"test me\"},"
                + "]}";

            var testClient = Common.CreateClient<BulkSessionsResult>(content, null, null);
            client = new Client(testClient);
            var bulks = client.GetBulkSessions(page, limit);

            Assert.IsTrue(bulks.Success);
            Assert.NotNull(bulks.BulkSessions);
            Assert.AreEqual(2, bulks.BulkSessions.Count);
            Assert.AreEqual(page, bulks.Page);
            Assert.AreEqual(limit, bulks.Limit);
            Assert.AreEqual(2, bulks.PageCount);
            Assert.NotNull(bulks.BulkSessions[0].Session);
            Assert.AreEqual(BulkSessionStatus.Completed, bulks.BulkSessions[0].Status);
            Assert.AreEqual(BulkSessionStatus.Failed, bulks.BulkSessions[1].Status);
        }
    }
}
