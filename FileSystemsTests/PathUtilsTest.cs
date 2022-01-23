// Copyright (C) 2017  Joey Scarr
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using KFS.FileSystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace KFA.FileSystemsTests.FileSystem {
	[TestClass]
	public class PathUtilsTest {
		[TestMethod]
		public void CombinePath() {
			// This test only works on Windows.
			Assert.AreEqual("foo\\bar", PathUtils.Combine("foo", "bar"));
			Assert.AreEqual("foo\\bar", PathUtils.Combine("foo\\", "bar"));
		}
		[TestMethod]
		public void CombinePathWithDriveLetter() {
			// This test only works on Windows.
			Assert.AreEqual("C:\\blah", PathUtils.Combine("C:", "blah"));
			Assert.AreEqual(
				"C:\\blah\\foo\\bar\\baz\\", PathUtils.Combine("C:", "blah", "foo\\bar\\baz\\"));
		}
		[TestMethod]
		public void CombinePathWithQuestionMark() {
			// This test only works on Windows.
			Assert.AreEqual("C:\\?\\foo.txt", PathUtils.Combine("C:", "?", "foo.txt"));
			Assert.AreEqual("C:\\?\\foo.txt", PathUtils.Combine("C:\\?", "foo.txt"));
			Assert.AreEqual("C:\\?\\foo.txt", PathUtils.Combine("C:", "?\\foo.txt"));
		}
		[TestMethod]
		public void ValidFileNamesAreUnchanged() {
			Assert.AreEqual("abc", PathUtils.MakeFileNameValid("abc"));
			Assert.AreEqual("a_b_c", PathUtils.MakeFileNameValid("a_b_c"));
			Assert.AreEqual("test.jpg", PathUtils.MakeFileNameValid("test.jpg"));
		}
		[TestMethod]
		public void InvalidFileNamesAreSanitized() {
			Assert.AreEqual("abc_t._blah", PathUtils.MakeFileNameValid("abc/t.:blah"));
			Assert.AreEqual("C____file_.txt", PathUtils.MakeFileNameValid("C:/?/file_.txt"));
		}
	}
}
