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

namespace MagikInfo.TeamCowboy
{
    using System.Threading.Tasks;
    using System.Net.Http;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System;

    public partial class TeamCowboyService
    {
        #region Server Addresses
        public static string c_SecureServer = "https://api.teamcowboy.com/v1/";
        public static string c_Server = "http://api.teamcowboy.com/v1/";
        #endregion

        #region Parameter Strings
        // Please keep these sorted
        private const string c_addlFemale = "addlFemale";
        private const string c_addlMale = "addlMale";
        private const string c_allowComments = "allowComments";
        private const string c_body = "body";
        private const string c_comment = "comment";
        private const string c_commentId = "commentId";
        private const string c_comments = "comments";
        private const string c_dashboardTeamsOnly = "dashboardTeamsOnly";
        private const string c_endDateTime = "endDateTime";
        private const string c_eventId = "eventId";
        private const string c_filter = "filter";
        private const string c_includeInactive = "includeInactive";
        private const string c_includeRSVPInfo = "includeRSVPInfo";
        private const string c_isHidden = "isHidden";
        private const string c_isPinned = "isPinned";
        private const string c_loadComments = "loadComments";
        private const string c_messageId = "messageId";
        private const string c_offset = "offset";
        private const string c_password = "password";
        private const string c_qty = "qty";
        private const string c_rsvpAsUserId = "rsvpAsUserId";
        private const string c_seasonId = "seasonId";
        private const string c_sendNotifications = "sendNotifications";
        private const string c_sortBy = "sortBy";
        private const string c_sortDirection = "sortDirection";
        private const string c_specificDates = "specificDates";
        private const string c_startDateTime = "startDateTime";
        private const string c_status = "status";
        private const string c_teamId = "teamId";
        private const string c_testParam = "testParam";
        private const string c_title = "title";
        private const string c_token = "token";
        private const string c_userId = "userId";
        private const string c_username = "username";
        private const string c_userToken = "userToken";
        #endregion

        #region ApiCall Strings
        // Please keep these sorted
        private const string c_AddMessageComment = "MessageComment_Add";
        private const string c_DeleteMessage = "Message_Delete";
        private const string c_DeleteMessageComment = "MessageComment_Delete";
        private const string c_GetEvent = "Event_Get";
        private const string c_GetEventAttendanceList = "Event_GetAttendanceList";
        private const string c_GetMessage = "Message_Get";
        private const string c_GetTeam = "Team_Get";
        private const string c_GetTeamEvents = "Team_GetEvents";
        private const string c_GetTeamMessages = "Team_GetMessages";
        private const string c_GetTeamRoster = "Team_GetRoster";
        private const string c_GetTeamSeasons = "Team_GetSeasons";
        private const string c_GetTestRequest = "Test_GetRequest";
        private const string c_GetUser = "User_Get";
        private const string c_GetUserNextTeamEvent = "User_GetNextTeamEvent";
        private const string c_GetUserTeamEvents = "User_GetTeamEvents";
        private const string c_GetUserTeamMessages = "User_GetTeamMessages";
        private const string c_GetUserToken = "Auth_GetUserToken";
        private const string c_PostTestRequest = "Test_PostRequest";
        private const string c_SaveEventRSVP = "Event_SaveRSVP";
        private const string c_SaveMessage = "Message_Save";
        #endregion

        #region Authentication
        /// <summary>
        /// Login to the service. This will set the _userID private member on success
        /// </summary>
        /// <param name="user">The UserID</param>
        /// <param name="password">The password for the user</param>
        /// <returns></returns>
        public async Task<string> LoginAsync(string user, string password)
        {
            AddPendingOp();

            try
            {
                var param = new ValuePair[]
                {
                    new ValuePair(c_username, user),
                    new ValuePair(c_password, password)
                };

                var jsonObject = await TeamCowboyAPIAsync(c_GetUserToken, HttpMethod.Post, param, true);
                _userID = jsonObject[c_body][c_token].Value<string>();
                return _userID;
            }
            finally
            {
                RemovePendingOp();
            }
        }
        #endregion Authentication

        #region Event
        /// <summary>
        /// Get the information about an event
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="eventId"></param>
        /// <param name="includeRSVPInfo"></param>
        /// <returns></returns>
        public async Task<JObject> GetEventAsync(int teamId, int eventId, bool? includeRSVPInfo = null)
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId),
                    new ValuePair(c_eventId, eventId),
                };

                if (includeRSVPInfo.HasValue) { param.Add(new ValuePair(c_includeRSVPInfo, includeRSVPInfo.Value)); }

                return await TeamCowboyAPIAsync(c_GetEvent, HttpMethod.Get, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Get the list of Attendees for an event
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<JObject> GetEventAttendanceListAsync(int teamId, int eventId)
        {
            AddPendingOp();
            try
            {
                var param = new ValuePair[]
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId),
                    new ValuePair(c_eventId, eventId)
                };

                return await TeamCowboyAPIAsync(c_GetEventAttendanceList, HttpMethod.Get, param);
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Save an RSVP for an event
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="eventId"></param>
        /// <param name="status"></param>
        /// <param name="comments"></param>
        /// <param name="rsvpAsUserId"></param>
        /// <param name="addlMale"></param>
        /// <param name="addlFemale"></param>
        /// <returns></returns>
        public async Task<JObject> SaveEventRSVPAsync(
            int teamId,
            int eventId,
            string /*enum*/ status,
            string comments,
            int? rsvpAsUserId = null,
            int? addlMale = null,
            int? addlFemale = null
            )
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId),
                    new ValuePair(c_eventId, eventId),
                    new ValuePair(c_status, status)
                };

                if (!string.IsNullOrEmpty(comments)) { param.Add(new ValuePair(c_comments, comments)); }
                if (rsvpAsUserId.HasValue) { param.Add(new ValuePair(c_rsvpAsUserId, rsvpAsUserId.Value)); }
                if (addlMale.HasValue) { param.Add(new ValuePair(c_addlMale, addlMale.Value)); }
                if (addlFemale.HasValue) { param.Add(new ValuePair(c_addlFemale, addlFemale.Value)); }

                return await TeamCowboyAPIAsync(c_SaveEventRSVP, HttpMethod.Post, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }
        #endregion Event

        #region Message
        /// <summary>
        /// Get a message
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="messageId"></param>
        /// <param name="loadComments"></param>
        /// <returns></returns>
        public async Task<JObject> GetMessageAsync(int teamId, int messageId, bool? loadComments = null)
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId),
                    new ValuePair(c_messageId, messageId),
                };

                if (loadComments.HasValue) { param.Add(new ValuePair(c_loadComments, loadComments.Value)); }

                return await TeamCowboyAPIAsync(c_GetMessage, HttpMethod.Get, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Delete a message from a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<JObject> DeleteMessageAsync(int teamId, int messageId)
        {
            AddPendingOp();
            try
            {
                var param = new ValuePair[]
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId),
                    new ValuePair(c_messageId, messageId)
                };

                return await TeamCowboyAPIAsync(c_DeleteMessage, HttpMethod.Post, param);
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Save a message for a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        /// <param name="messageId"></param>
        /// <param name="isPinned"></param>
        /// <param name="sendNotifications"></param>
        /// <param name="isHidden"></param>
        /// <param name="allowComments"></param>
        /// <returns></returns>
        public async Task<JObject> SaveMessageAsync(
            int teamId,
            string title,
            string body,
            int? messageId = null,
            bool? isPinned = null,
            bool? sendNotifications = null,
            bool? isHidden = null,
            bool? allowComments = null)
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId),
                    new ValuePair(c_title, title),
                    new ValuePair(c_body, body)
                };

                if (messageId.HasValue) { param.Add(new ValuePair(c_messageId, messageId.Value)); }
                if (isPinned.HasValue) { param.Add(new ValuePair(c_isPinned, isPinned.Value)); }
                if (sendNotifications.HasValue) { param.Add(new ValuePair(c_sendNotifications, sendNotifications.Value)); }
                if (isHidden.HasValue) { param.Add(new ValuePair(c_isHidden, isHidden.Value)); }
                if (allowComments.HasValue) { param.Add(new ValuePair(c_allowComments, allowComments.Value)); }

                return await TeamCowboyAPIAsync(c_SaveMessage, HttpMethod.Post, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Delete a comment from a message to a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="messageId"></param>
        /// <param name="commentId"></param>
        /// <returns></returns>
        public async Task<JObject> DeleteMessageCommentAsync(int teamId, int messageId, int commentId)
        {
            AddPendingOp();
            try
            {
                var param = new ValuePair[]
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId),
                    new ValuePair(c_messageId, messageId),
                    new ValuePair(c_commentId, commentId)
                };

                return await TeamCowboyAPIAsync(c_DeleteMessageComment, HttpMethod.Post, param);
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Add a comment to a message for a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="messageId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public async Task<JObject> AddMessageCommentAsync(int teamId, int messageId, string comment)
        {
            AddPendingOp();
            try
            {
                var param = new ValuePair[]
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId),
                    new ValuePair(c_messageId, messageId),
                    new ValuePair(c_comment, comment)
                };

                return await TeamCowboyAPIAsync(c_AddMessageComment, HttpMethod.Post, param);
            }
            finally
            {
                RemovePendingOp();
            }
        }
        #endregion Message

        #region Team

        /// <summary>
        /// Get a Team
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public async Task<JObject> GetTeamAsync(int teamId)
        {
            AddPendingOp();
            try
            {
                var param = new ValuePair[]
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId)
                };

                return await TeamCowboyAPIAsync(c_GetTeam, HttpMethod.Get, param);
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Get the events for a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="seasonId"></param>
        /// <param name="filter"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="offset"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task<JObject> GetTeamEventsAsync(
            int teamId,
            int? seasonId = null,
            string filter = null,
            DateTime? startDateTime = null,
            DateTime? endDateTime = null,
            int? offset = null,
            int? quantity = null
            )
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>()
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId)
                };

                if (seasonId.HasValue) { param.Add(new ValuePair(c_seasonId, seasonId.Value)); }
                if (filter != null)
                {
                    param.Add(new ValuePair(c_filter, filter));
                    if (filter == c_specificDates)
                    {
                        param.Add(new ValuePair(c_startDateTime, startDateTime.Value.ToString()));
                        param.Add(new ValuePair(c_endDateTime, endDateTime.Value.ToString()));
                    }
                }

                if (offset.HasValue) { param.Add(new ValuePair(c_offset, offset.Value)); }
                if (quantity.HasValue) { param.Add(new ValuePair(c_qty, quantity.Value)); }

                return await TeamCowboyAPIAsync(c_GetTeamEvents, HttpMethod.Get, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Get the messages for a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="messageId"></param>
        /// <param name="offset"></param>
        /// <param name="quantity"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public async Task<JObject> GetTeamMessagesAsync(
            int teamId,
            int? messageId = null,
            int? offset = null,
            int? quantity = null,
            string sortBy = null,
            string sortDirection = null
            )
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId)
                };

                if (messageId.HasValue) { param.Add(new ValuePair(c_messageId, messageId.Value)); }
                if (offset.HasValue) { param.Add(new ValuePair(c_offset, offset.Value)); }
                if (quantity.HasValue) { param.Add(new ValuePair(c_qty, quantity.Value)); }
                if (sortBy != null) { param.Add(new ValuePair(c_sortBy, sortBy)); }
                if (sortDirection != null) { param.Add(new ValuePair(c_sortDirection, sortDirection)); }

                return await TeamCowboyAPIAsync(c_GetTeamMessages, HttpMethod.Get, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Get the roster for a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="userId"></param>
        /// <param name="includeInactive"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public async Task<JObject> GetTeamRosterAsync(
            int teamId,
            int? userId = null,
            bool? includeInactive = null,
            string sortBy = null,
            string sortDirection = null
            )
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId)
                };

                if (userId.HasValue) { param.Add(new ValuePair(c_userId, userId.Value)); }
                if (includeInactive.HasValue) { param.Add(new ValuePair(c_includeInactive, includeInactive.Value)); }
                if (sortBy != null) { param.Add(new ValuePair(c_sortBy, sortBy)); }
                if (sortDirection != null) { param.Add(new ValuePair(c_sortDirection, sortDirection)); }

                return await TeamCowboyAPIAsync(c_GetTeamRoster, HttpMethod.Get, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Get the seasons for a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public async Task<JObject> GetTeamSeasonsAsync(int teamId)
        {
            AddPendingOp();
            try
            {
                var param = new ValuePair[]
                {
                    new ValuePair(c_userToken, _userID),
                    new ValuePair(c_teamId, teamId)
                };

                return await TeamCowboyAPIAsync(c_GetTeamSeasons, HttpMethod.Get, param);
            }
            finally
            {
                RemovePendingOp();
            }
        }
        #endregion Team

        #region User

        /// <summary>
        /// Get the user details
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> GetUserAsync()
        {
            AddPendingOp();
            try
            {
                var param = new ValuePair[]
                {
                    new ValuePair(c_userToken, _userID)
                };
                return await TeamCowboyAPIAsync(c_GetUser, HttpMethod.Get, param, false);
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Get the next team event for a user
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="dashboardTeamsOnly"></param>
        /// <returns></returns>
        public async Task<JObject> GetUserNextTeamEventAsync(
            int? teamId = null,
            bool? dashboardTeamsOnly = null
            )
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>
                {
                    new ValuePair(c_userToken, _userID)
                };

                if (teamId.HasValue) { param.Add(new ValuePair(c_teamId, teamId.Value)); }
                if (dashboardTeamsOnly.HasValue) { param.Add(new ValuePair(c_dashboardTeamsOnly, dashboardTeamsOnly.Value)); }

                return await TeamCowboyAPIAsync(c_GetUserNextTeamEvent, HttpMethod.Get, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Get the list of team events for the user
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="teamId"></param>
        /// <param name="dashboardTeamsOnly"></param>
        /// <returns></returns>
        public async Task<JObject> GetUserTeamEventsAsync(
            DateTime? startDateTime = null,
            DateTime? endDateTime = null,
            int? teamId = null,
            bool? dashboardTeamsOnly = null
            )
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>
                {
                    new ValuePair(c_userToken, _userID),
                };

                if (startDateTime.HasValue) { param.Add(new ValuePair(c_startDateTime, startDateTime.Value.ToLongDateString())); }
                if (endDateTime.HasValue) { param.Add(new ValuePair(c_endDateTime, endDateTime.Value.ToLongDateString())); }
                if (teamId.HasValue) { param.Add(new ValuePair(c_teamId, teamId.Value)); }
                if (dashboardTeamsOnly.HasValue) { param.Add(new ValuePair(c_dashboardTeamsOnly, dashboardTeamsOnly.Value)); }

                return await TeamCowboyAPIAsync(c_GetUserTeamEvents, HttpMethod.Get, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Get the messages for a user
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="messageId"></param>
        /// <param name="offset"></param>
        /// <param name="quantity"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public async Task<JObject> GetUserTeamMessagesAsync(
            int? teamId = null,
            int? messageId = null,
            int? offset = null,
            int? quantity = null,
            string sortBy = null,
            string sortDirection = null
            )
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>
                {
                    new ValuePair(c_userToken, _userID),
                };

                if (teamId.HasValue) { param.Add(new ValuePair(c_teamId, teamId.Value)); }
                if (messageId.HasValue) { param.Add(new ValuePair(c_messageId, messageId.Value)); }
                if (offset.HasValue) { param.Add(new ValuePair(c_offset, offset.Value)); }
                if (quantity.HasValue) { param.Add(new ValuePair(c_qty, quantity.Value)); }
                if (sortBy != null) { param.Add(new ValuePair(c_sortBy, sortBy)); }
                if (sortDirection != null) { param.Add(new ValuePair(c_sortDirection, sortDirection)); }

                return await TeamCowboyAPIAsync(c_GetUserTeamMessages, HttpMethod.Get, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Get the teams a user is a member of
        /// </summary>
        /// <param name="dashboardTeamsOnly"></param>
        /// <returns></returns>
        public async Task<JObject> GetUserTeamsAsync(bool? dashboardTeamsOnly = null)
        {
            AddPendingOp();
            try
            {
                var param = new List<ValuePair>
                {
                    new ValuePair(c_userToken, _userID)
                };

                if (dashboardTeamsOnly.HasValue) { param.Add(new ValuePair(c_dashboardTeamsOnly, dashboardTeamsOnly.Value)); }

                return await TeamCowboyAPIAsync("User_GetTeams", HttpMethod.Get, param.ToArray());
            }
            finally
            {
                RemovePendingOp();
            }
        }
        #endregion USER


#if DEBUG
        #region Test
        /// <summary>
        /// Test a Get request
        /// </summary>
        /// <param name="testParam"></param>
        /// <returns></returns>
        public async Task<JObject> TestGetRequestAsync(string testParam)
        {
            AddPendingOp();
            try
            {
                var param = new ValuePair[]
                {
                    new ValuePair(c_testParam, testParam),
                };

                return await TeamCowboyAPIAsync(c_GetTestRequest, HttpMethod.Get, param);
            }
            finally
            {
                RemovePendingOp();
            }
        }

        /// <summary>
        /// Test a post request
        /// </summary>
        /// <param name="testParam"></param>
        /// <returns></returns>
        public async Task<JObject> TestPostRequestAsync(string testParam)
        {
            AddPendingOp();
            try
            {
                var param = new ValuePair[]
                {
                    new ValuePair(c_testParam, testParam),
                };

                return await TeamCowboyAPIAsync(c_PostTestRequest, HttpMethod.Post, param);
            }
            finally
            {
                RemovePendingOp();
            }
        }
        #endregion Test
#endif
    }
}
