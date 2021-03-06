﻿namespace ArticleHarbor.DomainModel.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Ploeh.AutoFixture.Xunit;
    using Xunit;

    public class InClausePredicateTest : IdiomaticTest<InClausePredicate>
    {
        [Test]
        public void SutIsPredicate(InClausePredicate sut)
        {
            Assert.IsAssignableFrom<IPredicate>(sut);
        }

        [Test]
        public void SqlTextIsCorrect(InClausePredicate sut)
        {
            var expected = string.Format(
                "{0} IN ({1})",
                sut.ColumnName,
                string.Join(", ", sut.Parameters.Select(p => p.Name)));
            var actual = sut.SqlText;
            Assert.Equal(expected, actual);
        }

        [Test]
        public void ParameterValuesIsCorrect(InClausePredicate sut)
        {
            var expected = sut.Parameters.Select(x => x.Value);
            var actual = sut.Values;
            Assert.Equal(expected, actual);
        }

        [Test]
        public void ParametersIsCorrectWhenInitializedWithArray(
            [FavorArrays] InClausePredicate sut)
        {
            var actual = sut.Parameters;

            Assert.Equal(sut.Values, actual.Select(x => x.Value));
            Assert.DoesNotThrow(() => actual.Select(x =>
            {
                Assert.True(x.Name.StartsWith("@" + sut.ColumnName));
                return Guid.Parse(x.Name.Remove(0, sut.ColumnName.Length + 1));
            }).ToArray());
        }

        [Test]
        public void ParametersIsCorrectWhenInitializedWithEnumerable(
            [FavorEnumerables] InClausePredicate sut)
        {
            var actual = sut.Parameters;

            Assert.Equal(sut.Values, actual.Select(x => x.Value));
            Assert.DoesNotThrow(() => actual.Select(x =>
            {
                Assert.True(x.Name.StartsWith("@" + sut.ColumnName));
                return Guid.Parse(x.Name.Remove(0, sut.ColumnName.Length + 1));
            }).ToArray());
        }

        [Test]
        public void ParametersReturnsAlwaysSameValueWhenInitializedWithArray(
            InClausePredicate sut)
        {
            var actual = sut.Parameters;
            Assert.Equal(sut.Parameters, actual);
        }

        [Test]
        public void ParametersReturnsAlwaysSameValueWhenInitializedWithEnumerable(
            [FavorEnumerables] InClausePredicate sut)
        {
            var actual = sut.Parameters;
            Assert.Equal(sut.Parameters, actual);
        }

        [Test]
        public void EqualsDoesNotEqualOtherWithDifferentValues(
            InClausePredicate sut,
            InClausePredicate other)
        {
            var actual = sut.Equals(other);
            Assert.False(actual);
        }

        [Test]
        public void EqualsEqualsOtherWithSameValues(InClausePredicate sut)
        {
            var other = new InClausePredicate(sut.ColumnName, sut.Values.ToArray());
            var actual = sut.Equals(other);
            Assert.True(actual);
        }

        [Test]
        public void EqualsIsCaseInsensitiveForColumnName(InClausePredicate sut)
        {
            var other = new InClausePredicate(sut.ColumnName.ToUpper(), sut.Values);
            var actual = sut.Equals(other);
            Assert.True(actual);
        }

        [Test]
        public void GetHasCodeReturnsCorrectResult(InClausePredicate sut)
        {
            var other = new InClausePredicate(
                sut.ColumnName.ToUpper(),
                sut.Values.ToArray());
            var actual = sut.GetHashCode();
            Assert.Equal(other.GetHashCode(), actual);
        }

        [Test]
        public void GetHasCodeWithEmptyValuesReturnsCorrectResult(string columnName)
        {
            var sut = new InClausePredicate(columnName.ToUpper());
            var other = new InClausePredicate(columnName);

            var actual = sut.GetHashCode();

            Assert.Equal(other.GetHashCode(), actual);
        }

        protected override IEnumerable<MemberInfo> ExceptToVerifyInitialization()
        {
            yield return this.Properties.Select(x => x.SqlText);
            yield return this.Properties.Select(x => x.Parameters);
        }
    }
}