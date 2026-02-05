using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NRobot.Server.Test
{
    internal static class LegacyAssert
    {
        public static void IsTrue(bool condition)
        {
            NUnit.Framework.Assert.That(condition, Is.True);
        }

        public static void IsTrue(bool condition, string message)
        {
            NUnit.Framework.Assert.That(condition, Is.True, message);
        }

        public static void IsFalse(bool condition)
        {
            NUnit.Framework.Assert.That(condition, Is.False);
        }

        public static void IsFalse(bool condition, string message)
        {
            NUnit.Framework.Assert.That(condition, Is.False, message);
        }

        public static void True(bool condition)
        {
            IsTrue(condition);
        }

        public static void True(bool condition, string message)
        {
            IsTrue(condition, message);
        }

        public static void AreEqual(object expected, object actual)
        {
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected));
        }

        public static void AreEqual(object expected, object actual, string message)
        {
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected), message);
        }

        public static void Contains(object expected, IEnumerable actual)
        {
            NUnit.Framework.Assert.That(actual, Does.Contain(expected));
        }

        public static void That<TActual>(TActual actual, IResolveConstraint expression)
        {
            NUnit.Framework.Assert.That(actual, expression);
        }

        public static TException Throws<TException>(TestDelegate code) where TException : Exception
        {
            return NUnit.Framework.Assert.Throws<TException>(code);
        }
    }
}
