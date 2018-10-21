/*************************************************************************************************
 * Copyright (c) 2018 MagikInfo
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software withou
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
/*************************************************************************************************/

using MagikInfo.TeamCowboy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TeamCowboyAPITest
{
    [TestClass]
    public class TeamCowboyAPITest
    {
        static private TeamCowboyService _service;
        [ClassInitialize]
        public async static Task ClassInitialize(TestContext context)
        {
            _service = new TeamCowboyService(
                context.Properties["publicAPIKey"].ToString(),
                context.Properties["privateAPIKey"].ToString()
                );

            // Login since the other tests require to be logged in
            await Login(
                context.Properties["username"].ToString(),
                context.Properties["password"].ToString()
                );
        }

        public static async Task Login(string username, string password)
        {
            var loginToken = await _service.LoginAsync(username, password);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(loginToken));
        }

        private void ValidateRequest(JObject request)
        {
            Assert.IsTrue((bool)request["success"]);
        }

        private async Task<int> GetTeamId()
        {
            // Find a team
            var teams = await _service.GetUserTeamsAsync();
            ValidateRequest(teams);
            var body = teams["body"];
            return (int)body[0]["teamId"];
        }

        private async Task<int> GetUserId()
        {
            var user = await _service.GetUserAsync();
            ValidateRequest(user);
            return ((int)user["body"]["userId"]);
        }

        private async Task<int> GetEventId()
        {
            var teamId = await GetTeamId();
            var events = await _service.GetTeamEventsAsync(teamId);
            ValidateRequest(events);
            return (int)events["body"][0]["eventId"];
        }

        [TestMethod]
        public async Task TestPendingOps()
        {
            int added = 0;
            int removed = 0;
            _service.PendingOperationChanged += (add) =>
            {
                if (add)
                {
                    Interlocked.Increment(ref added);
                }
                else
                {
                    Interlocked.Increment(ref removed);
                }
            };

            // Invoke a couple of requests...
            try
            {
                await _service.GetUserAsync();
                await _service.GetUserTeamEventsAsync();
                await _service.GetUserTeamsAsync();
            }
            catch { }

            Assert.IsTrue(added == removed);
            Assert.IsTrue(added != 0);
        }

        [TestMethod]
        public async Task TestGetTeam()
        {
            var teamId = await GetTeamId();
            var team = await _service.GetTeamAsync(teamId);
            ValidateRequest(team);

            Assert.AreEqual(teamId, team["body"]["teamId"]);
        }

        [TestMethod]
        public async Task TestGetTeams()
        {
            var teams = await _service.GetUserTeamsAsync();
            ValidateRequest(teams);
            var body = teams["body"];
            Assert.IsNotNull(body);
            foreach (var team in body)
            {
                Assert.IsTrue((int)team["teamId"] != 0);
            }
        }

        [TestMethod]
        public async Task TestGetTeamSeasons()
        {
            var teamId = await GetTeamId();
            var seasons = await _service.GetTeamSeasonsAsync(teamId);
            ValidateRequest(seasons);
            Assert.IsNotNull(seasons["body"][0]);
        }

        [TestMethod]
        public async Task TestGetTeamRoster()
        {
            var teamId = await GetTeamId();
            var userId = await GetUserId();
            var roster = await _service.GetTeamRosterAsync(teamId);
            ValidateRequest(roster);

            bool foundUser = false;
            // Find the user on the roster
            foreach (var member in roster["body"])
            {
                if ((int)member["userId"] == userId)
                {
                    foundUser = true;
                }
            }

            Assert.IsTrue(foundUser);
        }

        /// <summary>
        /// Validate that we can get an event
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestGetEvent()
        {
            var team = await GetTeamId();
            var events = await _service.GetTeamEventsAsync(team);
            ValidateRequest(events);

            var myEvent = await _service.GetEventAsync(team, (int)events["body"][0]["eventId"]);
            ValidateRequest(myEvent);
            Assert.AreEqual(events["body"][0]["eventId"], myEvent["body"]["eventId"]);
        }

        [TestMethod]
        public async Task TestGetTeamEvents()
        {
            var team = await GetTeamId();
            var events = await _service.GetTeamEventsAsync(team);
            ValidateRequest(events);
            Assert.IsTrue((int)(events["body"][0]["eventId"]) != 0);
        }

        [TestMethod]
        public async Task TestGetEventAttendanceList()
        {
            var teamId = await GetTeamId();
            var eventId = await GetEventId();
            var attendance = await _service.GetEventAttendanceListAsync(teamId, eventId);
            ValidateRequest(attendance);
        }

        /// <summary>
        /// Save an RSVP and then Get the RSVP
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestEventRSVP()
        {
            var teamId = await GetTeamId();
            var eventId = await GetEventId();
            var status = "yes";
            var comment = "Coming to win!!!";

            var saveRSVP = await _service.SaveEventRSVPAsync(teamId, eventId, status, comment);
            ValidateRequest(saveRSVP);
            Assert.IsTrue((bool)saveRSVP["body"]["rsvpSaved"]);

            /// Get the RSVP from the event and see that it matches
            var getRSVP = await (_service.GetEventAsync(teamId, eventId, true));
            ValidateRequest(getRSVP);
            Assert.AreEqual(status, getRSVP["body"]["rsvpInstances"][0]["rsvpDetails"]["status"]);
            Assert.AreEqual(comment, getRSVP["body"]["rsvpInstances"][0]["rsvpDetails"]["comments"]);
        }

        [TestMethod]
        public async Task TestGetUser()
        {
            var user = await GetUserId();
            Assert.IsTrue(user != 0);
        }

        [TestMethod]
        public async Task TestGetUserNextTeamEvent()
        {
            var teamId = await GetTeamId();
            var nextEvent = await _service.GetUserNextTeamEventAsync(teamId, false);
            ValidateRequest(nextEvent);
            Assert.AreEqual(teamId, nextEvent["body"]["team"]["teamId"]);
        }

        /// <summary>
        /// Tests creating, deleting, and getting messages
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestMessages()
        {
            string subject = $"New Message {DateTime.Now.ToString()}";
            string body = "This is the message body.";
            string comment = $"Posting a new comment at {DateTime.Now.ToString()}";
            var teamId = await GetTeamId();

            // Create a message
            var message = await _service.SaveMessageAsync(teamId, subject, body,
                sendNotifications: false, allowComments: true);
            ValidateRequest(message);
            var messageId = (int)message["body"]["messageId"];

            // Add a comment
            var addComment = await _service.AddMessageCommentAsync(teamId, messageId, comment);
            ValidateRequest(addComment);
            var commentId = (int)addComment["body"]["commentId"];

            // Get all the messages and try to find the one we created
            var getMessages = await _service.GetTeamMessagesAsync(teamId);
            ValidateRequest(getMessages);
            JToken foundMessage = FindMessage(messageId, getMessages);
            Assert.IsNotNull(foundMessage);
            Assert.AreEqual(foundMessage["bodyHtml"], body);
            Assert.AreEqual(foundMessage["title"], subject);

            // Find the message in all the user team messages
            var getUserTeamMessages = await _service.GetUserTeamMessagesAsync();
            ValidateRequest(getUserTeamMessages);
            foundMessage = FindMessage(messageId, getUserTeamMessages);
            Assert.AreEqual(foundMessage["bodyHtml"], body);
            Assert.AreEqual(foundMessage["title"], subject);

            // Get the message directly
            var getMessage = await _service.GetMessageAsync(teamId, messageId, true);
            ValidateRequest(getMessage);
            Assert.AreEqual(getMessage["body"]["bodyHtml"], body);
            Assert.AreEqual(getMessage["body"]["title"], subject);

            // Look for the comment in the message
            JToken foundComment = null;
            foreach (var messageComment in getMessage["body"]["comments"])
            {
                if ((int)messageComment["commentId"] == commentId)
                {
                    foundComment = messageComment;
                }
            }
            Assert.IsNotNull(foundComment);
            Assert.AreEqual(comment, foundComment["comment"]);

            var deleteComment = await _service.DeleteMessageCommentAsync(teamId, messageId, commentId);
            ValidateRequest(deleteComment);

            // Get the message again, the comment should be removed
            getMessage = await _service.GetMessageAsync(teamId, messageId, true);
            ValidateRequest(getMessage);

            // Look for the comment in the message
            foundComment = null;
            foreach (var messageComment in getMessage["body"]["comments"])
            {
                if ((int)messageComment["commentId"] == commentId)
                {
                    foundComment = messageComment;
                }
            }
            Assert.IsNull(foundComment);

            var deleteMessage = await _service.DeleteMessageAsync(teamId, messageId);
            ValidateRequest(deleteMessage);

            // Verify that we can't find the message anymore
            getMessages = await _service.GetTeamMessagesAsync(teamId);
            ValidateRequest(getMessages);
            Assert.IsNull(FindMessage(messageId, getMessages));
        }

        private JToken FindMessage(int messageId, JObject messages)
        {
            JToken foundMessage = null;
            foreach (var getMessage in messages["body"])
            {
                if ((int)getMessage["messageId"] == messageId)
                {
                    foundMessage = getMessage;
                }
            }

            return foundMessage;
        }
    }
}
