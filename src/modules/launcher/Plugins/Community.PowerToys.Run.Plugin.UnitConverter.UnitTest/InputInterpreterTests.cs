// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.UnitConverter.UnitTest
{
    [TestClass]
    public class InputInterpreterTests
    {
        [DataTestMethod]
        [DataRow(new string[] { "1,5'" }, new object[] { new string[] { "1,5", "'" } })]
        [DataRow(new string[] { "1.5'" }, new object[] { new string[] { "1.5", "'" } })]
        [DataRow(new string[] { "1'" }, new object[] { new string[] { "1", "'" } })]
        [DataRow(new string[] { "1'5\"" }, new object[] { new string[] { "1", "'", "5", "\"" } })]
        [DataRow(new string[] { "5\"" }, new object[] { new string[] { "5", "\"" } })]
        [DataRow(new string[] { "1'5" }, new object[] { new string[] { "1", "'", "5" } })]
        public void RegexSplitsInput(string[] input, string[] expectedResult)
        {
            string[] shortsplit = InputInterpreter.RegexSplitter(input);
            CollectionAssert.AreEqual(expectedResult, shortsplit);
        }

        [DataTestMethod]
        [DataRow(new string[] { "1cm", "to", "mm" }, new object[] { new string[] { "1", "cm", "to", "mm" } })]
        public void InsertsSpaces(string[] input, string[] expectedResult)
        {
            InputInterpreter.InputSpaceInserter(ref input);
            CollectionAssert.AreEqual(expectedResult, input);
        }

        [DataTestMethod]
        [DataRow(new string[] { "1'", "in", "cm" }, new object[] { new string[] { "1", "foot", "in", "cm" } })]
        [DataRow(new string[] { "1\"", "in", "cm" }, new object[] { new string[] { "1", "inch", "in", "cm" } })]
        [DataRow(new string[] { "1'6", "in", "cm" }, new object[] { new string[] { "1.5", "foot", "in", "cm" } })]
        [DataRow(new string[] { "1'6\"", "in", "cm" }, new object[] { new string[] { "1.5", "foot", "in", "cm" } })]
        public void HandlesShorthandFeetInchNotation(string[] input, string[] expectedResult)
        {
            InputInterpreter.ShorthandFeetInchHandler(ref input, CultureInfo.InvariantCulture);
            CollectionAssert.AreEqual(expectedResult, input);
        }

        [DataTestMethod]
        [DataRow(new string[] { "5", "CeLsIuS", "in", "faHrenheiT" }, new object[] { new string[] { "5", "DegreeCelsius", "in", "DegreeFahrenheit" } })]
        [DataRow(new string[] { "5", "f", "in", "celsius" }, new object[] { new string[] { "5", "°f", "in", "DegreeCelsius" } })]
        [DataRow(new string[] { "5", "c", "in", "f" }, new object[] { new string[] { "5", "°c", "in", "°f" } })]
        [DataRow(new string[] { "5", "f", "in", "c" }, new object[] { new string[] { "5", "°f", "in", "°c" } })]
        public void PrefixesDegrees(string[] input, string[] expectedResult)
        {
            InputInterpreter.DegreePrefixer(ref input);
            CollectionAssert.AreEqual(expectedResult, input);
        }

        [DataTestMethod]
        [DataRow(new string[] { ".1", "cm", "to", "mm" }, new object[] { new string[] { "0.1", "cm", "to", "mm" } })]
        public void AddPrefixZero(string[] input, string[] expectedResult)
        {
            InputInterpreter.PrefixZero(ref input, CultureInfo.InvariantCulture);
            CollectionAssert.AreEqual(expectedResult, input);
        }

        [DataTestMethod]
        [DataRow("a f in c")]
        [DataRow("12 f in")]
        public void ParseInvalidQueries(string queryString)
        {
            Query query = new Query(queryString);
            var result = InputInterpreter.Parse(query);
            Assert.AreEqual(null, result);
        }

        [DataTestMethod]
        [DataRow("12 f in c", 12)]
        [DataRow("10m to cm", 10)]
        [DataRow(".1m to cm", 0.1)]
        public void ParseValidQueries(string queryString, double result)
        {
            Query query = new Query(queryString);
            var convertModel = InputInterpreter.Parse(query);
            Assert.AreEqual(result, convertModel.Value);
        }
    }
}
