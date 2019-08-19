using System;
using Barista.Common.Jobs;
using Xunit;

namespace Barista.Tests.Common.Jobs
{
    public class CronTests
    {
        private static readonly DateTime Now = new DateTime(2019, 5, 23, 0, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Cron_Secondly_ReturnsACronExpressionThatFiresEverySecond()
        {
            // Given
            var expression = Cron.Secondly();

            // When
            var oneSecondFromNow = expression.GetNextOccurrence(Now);
            var twoSecondsFromNow = expression.GetNextOccurrence(Now.AddSeconds(1));

            // Then
            Assert.Equal(Now.AddSeconds(1), oneSecondFromNow);
            Assert.Equal(Now.AddSeconds(2), twoSecondsFromNow);
        }

        [Fact]
        public void Cron_Minutely_ReturnsACronExpressionThatFiresEveryMinute()
        {
            // Given
            var expression = Cron.Minutely();

            // When
            var time = expression.GetNextOccurrence(Now);
            var timeTwo = expression.GetNextOccurrence(Now.AddSeconds(65));

            // Then
            Assert.Equal(Now.AddMinutes(1), time);
            Assert.Equal(Now.AddMinutes(2), timeTwo);
        }

        [Fact]
        public void Cron_SecondInterval_ReturnsACronExpressionThatFiresEveryXSeconds()
        {
            // Given
            var expressionOne = Cron.SecondInterval(5);
            var expressionTwo = Cron.SecondInterval(20);

            // When
            var timeOne = expressionOne.GetNextOccurrence(Now);
            var timeTwo = expressionTwo.GetNextOccurrence(Now);
            var timeTwoNext = expressionTwo.GetNextOccurrence(Now.AddSeconds(25));

            // Then
            Assert.Equal(Now.AddSeconds(5), timeOne);
            Assert.Equal(Now.AddSeconds(20), timeTwo);
            Assert.Equal(Now.AddSeconds(40), timeTwoNext);
        }

        [Fact]
        public void Cron_MinuteInterval_ReturnsACronExpressionThatFiresEveryXMinutes()
        {
            // Given
            var expressionOne = Cron.MinuteInterval(10);
            var expressionTwo = Cron.MinuteInterval(45);

            // When
            var timeOne = expressionOne.GetNextOccurrence(Now);
            var timeTwo = expressionTwo.GetNextOccurrence(Now);
            var timeTwoNext = expressionTwo.GetNextOccurrence(Now.AddMinutes(50), inclusive: true);
            var timeThreeNext = expressionTwo.GetNextOccurrence(Now.AddMinutes(65), inclusive: true);

            // Then
            Assert.Equal(Now.AddMinutes(10), timeOne);
            Assert.Equal(Now.AddMinutes(45), timeTwo);
            Assert.Equal(Now.AddHours(1), timeTwoNext);
            Assert.Equal(Now.AddHours(1).AddMinutes(45), timeThreeNext);
        }

        [Fact]
        public void Cron_HourInterval_ReturnsACronExpressionTahtFiresEveryXHours()
        {
            // Given
            var expressionOne = Cron.HourInterval(interval: 2);
            var expressionTwo = Cron.HourInterval(interval: 5);

            // When
            var timeOne = expressionOne.GetNextOccurrence(Now);
            var timeTwo = expressionTwo.GetNextOccurrence(Now);
            var timeThree = expressionOne.GetNextOccurrence(Now.AddHours(3));

            // Then
            Assert.Equal(Now.AddHours(2), timeOne);
            Assert.Equal(Now.AddHours(5), timeTwo);
            Assert.Equal(Now.AddHours(4), timeThree);
        }

        [Fact]
        public void Cron_DayInterval_ReturnsACronExpressionTahtFiresEveryXDays()
        {
            // Given
            var expressionOne = Cron.DayInterval(interval: 1);

            // When
            var timeOne = expressionOne.GetNextOccurrence(Now);
            var timeTwo = expressionOne.GetNextOccurrence(Now.AddDays(1).AddHours(10));

            // Then
            Assert.Equal(Now.AddDays(1), timeOne);
            Assert.Equal(Now.AddDays(2), timeTwo);
        }
    }
}
