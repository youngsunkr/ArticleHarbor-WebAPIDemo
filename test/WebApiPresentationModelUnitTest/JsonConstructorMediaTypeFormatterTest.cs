﻿namespace WebApiPresentationModelUnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Jwc.Experiment.Xunit;
    using Ploeh.AutoFixture;
    using WebApiPresentationModel;
    using Xunit;

    public class JsonConstructorMediaTypeFormatterTest : IdiomaticTest<JsonConstructorMediaTypeFormatter>
    {
        [Test]
        public void SubIsJsonMediaTypeFormatter(
            JsonConstructorMediaTypeFormatter sut)
        {
            Assert.IsAssignableFrom<JsonMediaTypeFormatter>(sut);
        }

        [Test]
        public void CanWriteTypeWithAnyTypeReturnsFalse(
            JsonConstructorMediaTypeFormatter sut,
            Type type)
        {
            bool actual = sut.CanWriteType(type);
            Assert.False(actual);
        }

        [Test]
        public IEnumerable<ITestCase> CanReadTypeWithSimpleTypeReturnsFalse()
        {
            var simpleTypes = new[]
            {
                typeof(TimeSpan),
                typeof(DateTime),
                typeof(Guid),
                typeof(string),
                typeof(char),
                typeof(bool),
                typeof(int),
                typeof(uint),
                typeof(byte),
                typeof(sbyte),
                typeof(short),
                typeof(ushort),
                typeof(long),
                typeof(ulong),
                typeof(float),
                typeof(double),
                typeof(decimal)
            };
            return TestCases.WithArgs(simpleTypes).WithAuto<JsonConstructorMediaTypeFormatter>()
                .Create((type, sut) =>
                {
                    var actual = sut.CanReadType(type);
                    Assert.False(actual);
                });
        }

        [Test]
        public IEnumerable<ITestCase> CanReadTypeWithNonSimpleTypeReturnsTrue()
        {
            var simpleTypes = new[]
            {
                typeof(object),
                this.GetType()
            };
            return TestCases.WithArgs(simpleTypes).WithAuto<JsonConstructorMediaTypeFormatter>()
                .Create((type, sut) =>
                {
                    var actual = sut.CanReadType(type);
                    Assert.True(actual);
                });
        }

        [Test]
        public async Task ReadFromStreamAsyncReturnsCorrectResult(
            string value,
            object expected,
            Type type,
            StreamContent content,
            IFormatterLogger formatterLogger,
            IFixture fixture)
        {
            Func<Type, string, object> formatter = (t, s) =>
            {
                Assert.Equal(type, t);
                Assert.Equal(value, s);
                return expected;
            };
            fixture.Inject(formatter);
            var sut = fixture.Create<JsonConstructorMediaTypeFormatter>();
            var stream = new MemoryStream(Encoding.Unicode.GetBytes(value));
            content.Headers.Add("Content-Type", "text/html; charset=utf-16");
            content.Headers.Add("Content-Length", "100");

            var actual = await sut.ReadFromStreamAsync(type, stream, content, formatterLogger);

            Assert.Equal(expected, actual);
        }

        [Test]
        public void ReadFromStreamAsyncWithNullStreamThrows(
            JsonConstructorMediaTypeFormatter sut,
            Type type,
            HttpContent content,
            IFormatterLogger formatterLogger)
        {
            var exception = Assert.Throws<AggregateException>(
                () => sut.ReadFromStreamAsync(type, null, content, formatterLogger).Result);
            Assert.IsType<ArgumentNullException>(exception.InnerException);
        }
        
        [Test]
        public void ReadFromStreamAsyncWithNullHttpContentThrows(
            JsonConstructorMediaTypeFormatter sut,
            Type type,
            Stream stream,
            IFormatterLogger formatterLogger)
        {
            var exception = Assert.Throws<AggregateException>(
                () => sut.ReadFromStreamAsync(type, stream, null, formatterLogger).Result);
            Assert.IsType<ArgumentNullException>(exception.InnerException);
        }

        protected override IEnumerable<MemberInfo> ExceptToVerifyGuardClause()
        {
            yield return this.Methods.Select(x => x.CanWriteType(null));
            yield return this.Methods.Select(x => x.ReadFromStreamAsync(null, null, null, null));
        }
    }
}