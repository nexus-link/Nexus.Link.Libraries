using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Guards;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.NexusLink;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Guards
{
    [TestClass]
    public class TestGuard
    {
        [TestInitialize]
        public void RunBeforeEveryTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(TestGuard));
        }

        [TestMethod]
        public void ScalarExamples()
        {
            var name = "Lars";
            Nl.Api.Parameter(name, nameof(name))
                .Is.Not.NullOrWhitespace()
                .Is.True(name.Length > 2, "have at least 2 characters.", true);

            var currentAge = 56;
            var graduationAge = 19;
            Nl.Dll.Parameter(currentAge, nameof(currentAge))
                .Is.GreaterThanOrEqualTo(18);
            Nl.Art.Value(name)
                .Is.Not.NullOrWhitespace();
            Nl.Art.Is.True(currentAge >= graduationAge || graduationAge == 0, $"Complete message");

            var birthDay = new DateTimeOffset(new DateTime(1963, 4, 25));
            var graduationDay = new DateTimeOffset(new DateTime(2019, 7, 28));
            var yearZero = new DateTimeOffset(new DateTime(0, 1, 1));
            Nl.Exe.Parameter(birthDay, nameof(birthDay))
                .Is.GreaterThan(yearZero);
            Nl.Exe.Parameter(graduationDay, nameof(graduationDay))
                .Is.GreaterThan().Parameter(birthDay, nameof(birthDay));
        }

        [TestMethod]
        public void TypeExamples()
        {
            var o = (object) new Adult()
            {
                Name = "Lars",
                Age = 18
            };
            Nl.Exe.Parameter("o", nameof(o))
                .Is.Not.Null()
                .Is.AssignableTo(out Person person1)
                .Is.Valid();
            Nl.Art.Value(o, "the found object")
                .Is.Not.Null()
                .Is.AssignableTo(out Person person2)
                .Is.Not.AssignableTo<object, Juvenile>()
                .Is.Valid();
        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        private double Multiply(double? x, double? y,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "")
        {
            return new PreciousCall(new CodeLocation(memberName, filePath, lineNumber)).Trace(Nl.Dll, guard =>
            {
                guard
                    .Parameter(x, nameof(x))
                    .Is.Not.Null();
                guard
                    .Parameter(x.Value, nameof(x))
                    .Is.GreaterThanOrEqualTo(0.0);
                guard
                    .Parameter(y, nameof(y))
                    .Is.Not.Null();
                guard
                    .Parameter(y.Value, nameof(y))
                    .Is.GreaterThanOrEqualTo(0.0);

                return x.Value * y.Value;
            });
        }

        private class Adult : Person
        {
            /// <inheritdoc />
            public override void Validate(string errorLocation, string propertyPath = "")
            {
                base.Validate(errorLocation, propertyPath);
                Nl.Val.Property(Age, nameof(Age))
                    .Is.GreaterThanOrEqualTo(18, "$The person needs to be over 18 years to be eligible to be an adult.");
            }
        }

        private class Juvenile : Person
        {
            /// <inheritdoc />
            public override void Validate(string errorLocation, string propertyPath = "")
            {
                base.Validate(errorLocation, propertyPath);
                Nl.Val.Property(Age, nameof(Age))
                    .Is.LessThan(18, "$The person needs to be under 18 years to be eligible to be a juvenile.");
            }
        }

        private class Person : IValidatable
        {
            public string Name { get; set; }
            public int Age { get; set; }

            public Address Address { get; set; }

            /// <inheritdoc />
            public virtual void Validate(string errorLocation, string propertyPath = "")
            {
                Nl.Val.Property(Name, nameof(Name))
                    .Is.Not.NullOrWhitespace()
                    .Is.True(Name.Length > 2, "have at least 2 characters.", true);
                Nl.Val.Property(Age, nameof(Age))
                    .Is.GreaterThanOrEqualTo(0);
            }
        }

        private class Address : IValidatable
        {
            public string Street { get; set; }

            /// <inheritdoc />
            public void Validate(string errorLocation, string propertyPath = "")
            {
                Nl.Val.Property(Street, nameof(Street))
                    .Is.Not.NullOrWhitespace()
                    .Is.True(Street.Length > 2, "have at least 2 characters.", true);
            }
        }

    }
}
