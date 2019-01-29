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

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) ) return false;
            if ( ReferenceEquals( this, obj ) ) return true;

            return obj.GetType() == GetType() && Equals( (TestEntity) obj );
        }

        private bool Equals( TestEntity other )
        {
            return string.Equals( Id, other.Id );
        }

        public override int GetHashCode()
        {
            return ( Id != null ? Id.GetHashCode() : 0 );
        }

        public override string ToString()
        {
            return Id;
        }
    }
}