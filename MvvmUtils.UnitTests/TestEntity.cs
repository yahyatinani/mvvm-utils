using System;

namespace UnitTests
{
    public class TestEntity
    {
        public TestEntity()
        {
            Id = Guid.NewGuid().ToString();
        }

        private string Id { get; }

#pragma warning disable 659
        public override bool Equals( object obj )
#pragma warning restore 659
        {
            if ( ReferenceEquals( this, obj ) ) return true;

            return obj.GetType() == GetType() && Equals( (TestEntity) obj );
        }

        private bool Equals( TestEntity other )
        {
            return string.Equals( Id, other.Id );
        }
    }
}