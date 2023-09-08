namespace Be.Vlaanderen.Basisregisters.SnapshotVerifier.Tests
{
    using AggregateSource;

    public class FakeEntity : Entity
    {
        private readonly int _privateMember;
        public int PublicMember;

        private FakeEntity() : base((_ => {}))
        {
        }

        public FakeEntity(int value)
            : this()
        {
            _privateMember = value;
            PublicMember = value;
        }
    }
}
