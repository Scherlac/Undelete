// Copyright (C) 2017  Joey Scarr, Josh Oosterman, Lukas Korsika
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

Disk Image Tests
================

Setup
-----

So far these are the only type of test that Kickass Undelete has. 

To run them, first extract test images in the FileSystemsTests\resources directory using 7zip. 

These are available from https://sourceforge.net/projects/kickassundelete/files/Test%20Resources/.
(149MB archived, ~2G extracted)

Then add them to the project, and in the properties (right-click the .img file -> Properties) set 
"Copy to Output Directory" to "Copy if Newer" for each file. 

Usage
-----

The tests will recover the first (and last if longer than 4k) 4k bytes from each deleted file,
and write those into <fs>-output.txt in the test folder (FileSystemTests\bin\<configuration>). 

These files also include the file name, path[0], and file size. 

The output of this process is then compared against saved outputs (resources\<fs>-expected.txt),
and if there's a change, the test fails.

Note that the test checks for consistency, so even an improvement that is able to
recover more files better will cause the test to fail. In this case, manually inspect
the outputs to see whether they're sane/better, and replace -expected with the generated
-output file. 


