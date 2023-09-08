﻿namespace Be.Vlaanderen.Basisregisters.SnapshotVerifier.Tests.GivenPrivateMemberDiffers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Xunit;

    public class WhenPrivateMemberIsIgnored
    {
        private readonly SnapshotVerifier<FakeAggregate, FakeAggregateStreamId> _snapshotVerifier;
        private readonly List<string> _membersToIgnore = new() { "_privateMember" };

        private readonly SnapshotIdentifier _snapshotIdentifier;
        private readonly Mock<ISnapshotVerificationRepository> _snapshotVerificationRepository;

        public WhenPrivateMemberIsIgnored()
        {
            var aggregateSnapshotRepository = new Mock<IAggregateSnapshotRepository<FakeAggregate>>();
            var aggregateEventsRepository = new Mock<IAggregateEventsRepository<FakeAggregate, FakeAggregateStreamId>>();

            var aggregateBySnapshot = new FakeAggregate(1, 1, 1, 1, 1, new List<int> { 1 });
            var aggregateByEvents = aggregateBySnapshot.WithDifferentPrivateMember(2);

            _snapshotIdentifier = new SnapshotIdentifier(1, "1");
            aggregateSnapshotRepository
                .Setup(x => x.GetSnapshotsSinceId(It.IsAny<int?>()))
                .ReturnsAsync(new List<SnapshotIdentifier> { _snapshotIdentifier });
            aggregateSnapshotRepository
                .Setup(x => x.GetAggregateBySnapshot(It.IsAny<int>()))
                .ReturnsAsync(new AggregateWithVersion<FakeAggregate>(aggregateBySnapshot, 1));
            aggregateEventsRepository
                .Setup(x =>
                    x.GetAggregateByEvents(It.IsAny<FakeAggregateStreamId>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(aggregateByEvents);

            _snapshotVerificationRepository = new Mock<ISnapshotVerificationRepository>();

            _snapshotVerifier = new SnapshotVerifier<FakeAggregate, FakeAggregateStreamId>(
                Mock.Of<IHostApplicationLifetime>(),
                _ => new FakeAggregateStreamId(1),
                _membersToIgnore,
                _snapshotVerificationRepository.Object,
                aggregateSnapshotRepository.Object,
                aggregateEventsRepository.Object,
                Mock.Of<ISnapshotVerificationNotifier>(),
                NullLoggerFactory.Instance);
        }

        [Fact]
        public async Task ThenAggregateBySnapshotEqualsAggregateByEvents()
        {
            await _snapshotVerifier.StartAsync(CancellationToken.None);

            _snapshotVerificationRepository
                .Verify(x => x.AddVerificationState(
                    It.Is<SnapshotVerificationState>(y =>
                        y.SnapshotId == _snapshotIdentifier.SnapshotId
                        && y.Status == SnapshotStateStatus.Verified),
                    It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
