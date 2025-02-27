﻿//-----------------------------------------------------------------------
// <copyright file="Bug2751Spec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2021 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2021 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit;
using Xunit;

namespace Akka.Tests.Actor.Dispatch
{
    /// <summary>
    /// Verifies that https://github.com/akkadotnet/akka.net/issues/2751 has been resolved
    /// </summary>
    public class Bug2751Spec : AkkaSpec
    {
        private class StopActor : ReceiveActor
        {
            private readonly IActorRef _testActor;

            public StopActor(IActorRef testActor)
            {
                _testActor = testActor;
                Receive<string>(s =>
                {
                    if (s == "stop")
                    {
                        Self.Tell("Hello");
                        Context.Stop(Self);
                    }
                    else
                    {
                        _testActor.Tell(s);
                    }
                });
            }
        }

        [Fact]
        public async Task ShouldReceiveSysMsgBeforeUserMsg()
        {
            var stopper = Sys.ActorOf(Props.Create(() => new StopActor(TestActor)));
            stopper.Tell("stop");
            await ExpectNoMsgAsync(TimeSpan.FromMilliseconds(250));
            Watch(stopper);
            await ExpectTerminatedAsync(stopper);
            await ExpectNoMsgAsync(TimeSpan.FromMilliseconds(100));
        }
    }

}
