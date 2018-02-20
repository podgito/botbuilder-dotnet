﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class QnaMakerMiddlewareTests
    {
        public string knowlegeBaseId = TestUtilities.GetKey("QNAKNOWLEDGEBASEID");
        public string subscriptionKey = TestUtilities.GetKey("QNASUBSCRIPTIONKEY");

        //[TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestMiddleware()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .Use(new QnAMakerMiddleware(new QnAMakerMiddlewareOptions()
                {
                    KnowledgeBaseId = knowlegeBaseId,
                    SubscriptionKey = subscriptionKey,
                    Top = 1
                }, new HttpClient()));

            bot.OnReceive((context) =>
                {
                    if (context.Request.AsMessageActivity().Text == "foo")
                    {
                        context.Reply(context.Request.AsMessageActivity().Text);
                    }
                    return Task.CompletedTask;
                });               

            await adapter
                .Send("foo")
                    .AssertReply("foo", "passthrough")
                .Send("how do I clean the stove?")
                    .AssertReply("BaseCamp: You can use a damp rag to clean around the Power Pack. Do not attempt to detach it from the stove body. As with any electronic device, never pour water on it directly. CampStove 2 &amp; CookStove: Power module: Remove the plastic power module from the fuel chamber and wipe it down with a damp cloth with soap and water. DO NOT submerge the power module in water or get it excessively wet. Fuel chamber: Wipe out with a nylon brush as needed. The pot stand at the top of the fuel chamber can be wiped off with a damp cloth and dried well. The fuel chamber can also be washed in a dishwasher. Dry very thoroughly.")
                .StartTest();
        }

    }
}
