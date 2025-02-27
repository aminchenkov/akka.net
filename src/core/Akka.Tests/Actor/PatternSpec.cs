﻿//-----------------------------------------------------------------------
// <copyright file="PatternSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2021 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2021 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.TestKit;
using Akka.TestKit.Extensions;
using Akka.Tests.Util;
using Xunit;

namespace Akka.Tests.Actor
{
    public class PatternSpec : AkkaSpec
    {
        [Fact]
        public async Task GracefulStop_must_provide_Task_for_stopping_an_actor()
        {
            //arrange
            var target = Sys.ActorOf<TargetActor>();

            //act
            var result = await target.GracefulStop(TimeSpan.FromSeconds(5))
                .AwaitWithTimeout(TimeSpan.FromSeconds(6));;

            //assert
            Assert.True(result);

        }

        [Fact]
        public async Task GracefulStop_must_complete_Task_when_actor_already_terminated()
        {
            //arrange
            var target = Sys.ActorOf<TargetActor>();

            //act
            

            //assert
            Assert.True(await target.GracefulStop(TimeSpan.FromSeconds(5)));
            Assert.True(await target.GracefulStop(TimeSpan.FromSeconds(5)));
        }

        [Fact]
        public async Task GracefulStop_must_complete_Task_with_TaskCanceledException_when_actor_not_terminated_within_timeout()
        {
            //arrange
            var target = Sys.ActorOf<TargetActor>();
            var latch = new TestLatch();

            //act
            target.Tell((latch, TimeSpan.FromSeconds(2)));

            //assert
            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await target.GracefulStop(TimeSpan.FromMilliseconds(50));
            });
            latch.Open();

        }

        [Fact]
        public async Task GracefulStop_must_not_send_unnecessary_Deadletter_bug_2157()
        {
            //arrange  
            var target = Sys.ActorOf<TargetActor>();
            Sys.EventStream.Subscribe(TestActor, typeof(DeadLetter));

            //act  
            var stopped = await target.GracefulStop(TimeSpan.FromSeconds(5));

            //assert  
            Assert.True(stopped);
            await ExpectNoMsgAsync(TimeSpan.Zero);
        }

        #region Actors

        public sealed class Work
        {
            public Work(TimeSpan duration)
            {
                Duration = duration;
            }

            public TimeSpan Duration { get; private set; }
        }

        public class TargetActor : UntypedActor
        {
            protected override void OnReceive(object message)
            {
                PatternMatch.Match(message)
                    .With<(TestLatch, TimeSpan)>(t => t.Item1.Ready(t.Item2));
            }
        }

        #endregion
    }
}

