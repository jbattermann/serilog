﻿using System;
using Serilog.Tests.Support;
using Xunit;

namespace Serilog.Tests.Core
{
    public class SecondaryLoggerSinkTests
    {
        [Fact]
        public void ModifyingCopiesPassedThroughTheSinkPreservesOriginal()
        {
            var secondary = new CollectingSink();
            var secondaryLogger = new LoggerConfiguration()
                .WriteTo.Sink(secondary)
                .CreateLogger();

            var e = Some.InformationEvent();
            new LoggerConfiguration()
                .WriteTo.Logger(secondaryLogger)
                .CreateLogger()
                .Write(e);
            
            Assert.NotSame(e, secondary.SingleEvent);
            var p = Some.LogEventProperty();
            secondary.SingleEvent.AddPropertyIfAbsent(p);
            Assert.True(secondary.SingleEvent.Properties.ContainsKey(p.Name));
            Assert.False(e.Properties.ContainsKey(p.Name));
        }

        [Fact]
        public void WhenOwnedByCallerSecondaryLoggerIsNotDisposed()
        {
            var secondary = new DisposeTrackingSink();
            var secondaryLogger = new LoggerConfiguration()
                .WriteTo.Sink(secondary)
                .CreateLogger();

            ((IDisposable)new LoggerConfiguration()
                .WriteTo.Logger(secondaryLogger)
                .CreateLogger()).Dispose();

            Assert.False(secondary.IsDisposed);
        }

        [Fact]
        public void WhenOwnedByPrimaryLoggerSecondaryIsDisposed()
        {
            var secondary = new DisposeTrackingSink();

            ((IDisposable)new LoggerConfiguration()
                .WriteTo.Logger(lc => lc.WriteTo.Sink(secondary))
                .CreateLogger()).Dispose();

            Assert.True(secondary.IsDisposed);
        }
    }
}
